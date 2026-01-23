using Spectre.Console;
using Spectre.Console.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;

namespace Inamsoft.MediaFileRenamer.Services.FileSystemServices;

public static class CleanFileScanner
{
    public static async Task RunDashboardAsync(
            DirectoryInfo root,
            FileScanOptions options,
            CancellationToken cancellationToken)
    {
        var uiChannel = Channel.CreateUnbounded<FileScanResult>();

        // Precompute total files before starting the background scanner to avoid
        // concurrent disk scans and to have an accurate progress denominator.
        long totalFiles = 0;
        AnsiConsole.Status()
            .Start("Counting files...", ctx =>
            {
                totalFiles = ChannelFileScanner.CountFilesForScan(root, options.MinFileSizeInBytes, options.SearchPattern, options.Recursive, options.OnError);
            });

        AnsiConsole.MarkupLine($"[green]Found {totalFiles:N0} files to scan.[/]");

        if (totalFiles == 0)
            totalFiles = 1; // avoid division by zero

        // Start scanning in background
        var scanTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var evt in ChannelFileScanner.ScanAsync(root, options, cancellationToken))
                {
                    // propagate events to UI channel; if caller cancels this will throw and go to finally
                    await uiChannel.Writer.WriteAsync(evt, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // honor cancellation
            }
            catch (Exception ex)
            {
                // surface errors via callback if provided
                options.OnError?.Invoke(ex);
            }
            finally
            {
                // Ensure UI consumer sees completion even on error/cancellation
                uiChannel.Writer.TryComplete();
            }
        }, cancellationToken);

        long processed = 0;
        var stopwatch = Stopwatch.StartNew();

        await AnsiConsole.Progress()
            .Columns(
                new SpinnerColumn(),
                new TaskDescriptionColumn(),
                new ProgressBarColumn
                {
                    CompletedStyle = new Style(Color.Blue),
                    FinishedStyle = new Style(Color.Green),
                    RemainingStyle = new Style(Color.Grey)
                },
                new PercentageColumn(),
                new ElapsedTimeColumn(),
                new RemainingTimeColumn())
            .StartAsync(async ctx =>
            {
                var processTask = ctx.AddTask("Processing files", maxValue: totalFiles);
                while (!ctx.IsFinished)
                {
                    await foreach (var evt in uiChannel.Reader.ReadAllAsync(cancellationToken))
                    {
                        processed++;

                        // Update progress panel (fraction available if needed)
                        double fraction = totalFiles > 0
                            ? (double)processed / totalFiles
                            : 0.0;

                        processTask.Description = $"Processing file {processed:N0} of {totalFiles:N0} ({fraction:P1})";

                        //options.OnFileFound?.Invoke(evt);
                        options.ProcessFile?.Invoke(evt);

                        // Update stats
                        //var stats = new BreakdownChart()
                        //        .AddItem("Processed", processed, Color.Green)
                        //        .AddItem("Remaining", Math.Max(0, totalFiles - processed), Color.Grey);
                        //AnsiConsole.Write(stats);
                        processTask.Increment(1);

                    }
                }
            });



        stopwatch.Stop();

        // Await the background scan task to observe any exceptions it might have thrown.
        await scanTask;

        ////////////////////////


        ////////////////////////

    }

}
