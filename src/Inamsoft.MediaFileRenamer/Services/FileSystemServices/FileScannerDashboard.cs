using Spectre.Console;
using System.Diagnostics;
using System.Threading.Channels;

namespace Inamsoft.MediaFileRenamer.Services.FileSystemServices;

public static class FileScannerDashboard
{
    public static async Task RunDashboardAsync(
        string root,
        ScanOptions options,
        CancellationToken cancellationToken)
    {
        var uiChannel = Channel.CreateUnbounded<FileScanEvent>();

        // Start scanning in background
        var scanTask = Task.Run(async () =>
        {
            await foreach (var evt in ChannelFileScanner.ScanAsync(root, options, cancellationToken))
            {
                await uiChannel.Writer.WriteAsync(evt, cancellationToken);
            }

            uiChannel.Writer.Complete();
        }, cancellationToken);

        // UI elements
        var tree = new Tree($"[yellow]{root}[/]");
        var progressPanel = CreateProgressPanel(0.0, 0, options.TotalFilesHint ?? 0);

        var stats = new BreakdownChart()
            .AddItem("Processed", 0, Color.Green)
            .AddItem("Remaining", options.TotalFilesHint ?? 0, Color.Grey);

        var layout = new Layout()
            .SplitRows(
                new Layout("header")
                    .Size(3)
                    .Update(new Panel("[bold blue]File Scanning Dashboard[/]")),
                new Layout("content")
                    .SplitColumns(
                        new Layout("tree").Ratio(2).Update(tree),
                        new Layout("right")
                            .SplitRows(
                                new Layout("progress").Size(5).Update(progressPanel),
                                new Layout("stats").Update(stats)
                            )
                    )
            );

        long processed = 0;
        long totalFiles = options.TotalFilesHint ?? 0;
        var stopwatch = Stopwatch.StartNew();

        await AnsiConsole.Live(layout)
            .StartAsync(async ctx =>
            {
                await foreach (var evt in uiChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    processed++;

                    // Update progress panel
                    double fraction = totalFiles > 0
                        ? (double)processed / totalFiles
                        : 0.0;

                    progressPanel = CreateProgressPanel(fraction, processed, totalFiles);
                    layout["progress"].Update(progressPanel);

                    // Update stats
                    stats.Clear();
                    stats.AddItem("Processed", processed, Color.Green);
                    stats.AddItem("Remaining", Math.Max(0, totalFiles - processed), Color.Grey);

                    // Update tree
                    AddFileToTree(tree, evt.FilePath);

                    ctx.Refresh();
                }

                stopwatch.Stop();
            });

        await scanTask;
    }

    private static Panel CreateProgressPanel(double fraction, long processed, long total)
    {
        const int width = 40;
        int filled = (int)(fraction * width);
        filled = Math.Clamp(filled, 0, width);

        var bar = new string('█', filled) + new string('─', width - filled);
        var percent = total > 0 ? $"{fraction:P0}" : "N/A";

        var text = new Markup(
            $"[green]{bar}[/]\n[grey]Processed:[/] {processed} / {total}  ([yellow]{percent}[/])");

        var panel = new Panel(text)
        {
            Header = new PanelHeader("Progress", Justify.Center),
            Border = BoxBorder.Rounded
        };

        return panel;
    }

    private static void AddFileToTree(Tree tree, string filePath)
    {
        var parts = filePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        TreeNode current = tree.Root;

        foreach (var part in parts.Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            var existing = current.Children
                .OfType<TreeNode>()
                .FirstOrDefault(n => n.Title.Equals(part, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                current = existing;
            }
            else
            {
                current = current.AddNode(part);
            }
        }
    }

    public sealed record FileScanEvent(string FilePath);
}



