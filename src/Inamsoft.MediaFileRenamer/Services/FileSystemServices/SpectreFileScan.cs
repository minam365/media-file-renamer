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

        // Tree is the root renderable
        var tree = new Tree(root.FullName);

        // Dictionary maps directory paths → TreeNode
        var dirNodes = new ConcurrentDictionary<string, TreeNode>();

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
            },

            OnFileFound = r =>
            {
                results.Add(r);

                if (dirNodes.TryGetValue(r.DirectoryPath, out var parentNode))
                {
                    parentNode.AddNode($"📄 {r.File.Name}");
                }
                else if (r.DirectoryPath == root.FullName)
                {
                    tree.AddNode($"📄 {r.File.Name}");
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
                var task = ctx.AddTask("Scanning...", autoStart: true);

                await AnsiConsole.Live(tree)
                    .StartAsync(async liveCtx =>
                    {
                        await foreach (var _ in ChannelFileScanner.ScanAsync(root, options, cancellationToken))
                        {
                            task.Increment(0.1);
                            liveCtx.Refresh();
                        }

                        task.StopTask();
                    });
            });

        return results.ToList();
    }
}

