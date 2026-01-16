using Spectre.Console;
using System.Collections.Concurrent;

namespace Inamsoft.MediaFileRenamer.Services.FileSystemServices;

public static class SpectreFileScan
{
    public static async Task<IReadOnlyList<FileScanResult>> ScanWithProgressAndTreeAsync(
        DirectoryInfo root,
        FileScanOptions options,
        CancellationToken cancellationToken = default)
    {
        var results = new ConcurrentBag<FileScanResult>();

        var directoryStats = new ConcurrentDictionary<string, DirectoryAggregation>();
        directoryStats[root.FullName] = new DirectoryAggregation
        {
            DirectoryPath = root.FullName
        };

        var tree = new Tree($"[bold]{root.FullName}[/]");
        var dirNodes = new ConcurrentDictionary<string, TreeNode>();

        // two-pass: pre-scan for accurate file count
        var totalFiles = CountFilesForScan(root, options.MinFileSizeInBytes, options.SearchPattern, options.OnError);
        if (totalFiles == 0)
            totalFiles = 1; // avoid division by zero

        options = options with
        {
            OnDirectoryEntered = d =>
            {
                var parent = d.Parent?.FullName;
                if (parent is null) return;

                if (parent == root.FullName)
                {
                    var node = tree.AddNode($"📁 {d.Name}");
                    dirNodes.TryAdd(d.FullName, node);
                }
                else if (dirNodes.TryGetValue(parent, out var parentNode))
                {
                    var node = parentNode.AddNode($"📁 {d.Name}");
                    dirNodes.TryAdd(d.FullName, node);
                }

                if (directoryStats.TryGetValue(parent, out var stats))
                {
                    Interlocked.Increment(ref stats.SubdirectoryCount);
                }

                directoryStats.TryAdd(d.FullName, new DirectoryAggregation()
                { 
                    DirectoryPath = d.FullName
                });

            },

            OnFileFound = r =>
            {
                results.Add(r);

                var label = $"{r.Icon} {r.File.Name} [grey]({r.MimeType}, {r.Size} bytes)[/]";

                if (dirNodes.TryGetValue(r.DirectoryPath, out var parentNode))
                {
                    parentNode.AddNode(label);
                }
                else if (r.DirectoryPath == root.FullName)
                {
                    tree.AddNode(label);
                }

                if (directoryStats.TryGetValue(r.DirectoryPath, out var stats))
                {
                    Interlocked.Add(ref stats.TotalSize, r.Size);
                    Interlocked.Increment(ref stats.FileCount);
                }

            }
        };

        await AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Scanning...", autoStart: true, maxValue: totalFiles);

                long processed = 0;

                await AnsiConsole.Live(tree)
                    .StartAsync(async liveCtx =>
                    {
                        await foreach (var _ in ChannelFileScanner.ScanAsync(root, options, cancellationToken))
                        {
                            Interlocked.Increment(ref processed);
                            task.Value = Math.Min(processed, totalFiles);
                            liveCtx.Refresh();
                        }

                        task.StopTask();
                    });
            });

        foreach (var (path, stats) in directoryStats)
        {
            if (dirNodes.TryGetValue(path, out var node)) 
            { 
                node.AddNode(
                    $"[grey]Files: {stats.FileCount}, " + 
                    $"Size: {stats.TotalSize} bytes, " + 
                    $"Subdirs: {stats.SubdirectoryCount}[/]"); 
            }
        }


        return results.ToList();
    }

    private static long CountFilesForScan(DirectoryInfo root, long minSize, string searchPattern, Action<Exception>? onError)
    {
        long count = 0;

        var stack = new Stack<DirectoryInfo>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            IEnumerable<FileInfo> SafeFiles()
            {
                try { return current.EnumerateFiles(searchPattern); }
                catch (Exception ex) { onError?.Invoke(ex); return Enumerable.Empty<FileInfo>(); }
            }

            IEnumerable<DirectoryInfo> SafeDirs()
            {
                try { return current.EnumerateDirectories(); }
                catch (Exception ex) { onError?.Invoke(ex); return Enumerable.Empty<DirectoryInfo>(); }
            }

            foreach (var file in SafeFiles())
            {
                try
                {
                    if (file.Length >= minSize)
                        count++;
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            }

            foreach (var dir in SafeDirs())
                stack.Push(dir);
        }

        return count;
    }

}

