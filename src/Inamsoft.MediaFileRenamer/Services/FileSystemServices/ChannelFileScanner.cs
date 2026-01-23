using Spectre.Console;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Inamsoft.MediaFileRenamer.Services.FileSystemServices;

public static class ChannelFileScanner
{
    public static async IAsyncEnumerable<FileScanResult> ScanAsync(
        DirectoryInfo root,
        FileScanOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var fileChannel = Channel.CreateUnbounded<FileInfo>();
        var resultChannel = Channel.CreateUnbounded<FileScanResult>();

        // PRODUCER: directory traversal (single producer, stack-based)
        var producer = Task.Run(async () =>
        {
            try
            {
                var stack = new Stack<DirectoryInfo>();
                stack.Push(root);

                while (stack.Count > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var dir = stack.Pop();
                    options.OnDirectoryEntered?.Invoke(dir);

                    IEnumerable<FileInfo> SafeFiles()
                    {
                        try { return dir.EnumerateFiles(options.SearchPattern); }
                        catch (Exception ex) { options.OnError?.Invoke(ex); return Enumerable.Empty<FileInfo>(); }
                    }

                    IEnumerable<DirectoryInfo> SafeDirs()
                    {
                        try { return dir.EnumerateDirectories(); }
                        catch (Exception ex) { options.OnError?.Invoke(ex); return Enumerable.Empty<DirectoryInfo>(); }
                    }

                    foreach (var file in SafeFiles())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        try
                        {
                            if (file.Length >= options.MinFileSizeInBytes)
                                await fileChannel.Writer.WriteAsync(file, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            options.OnError?.Invoke(ex);
                        }
                    }

                    if (options.Recursive)
                    {
                        foreach (var sub in SafeDirs())
                        {
                            stack.Push(sub);
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // respect cancellation — fall through to complete writer
            }
            catch (Exception ex)
            {
                options.OnError?.Invoke(ex);
            }
            finally
            {
                // Signal files done (consumers will complete)
                fileChannel.Writer.TryComplete();
            }
        }, cancellationToken);

        // CONSUMERS: file processing (parallel)
        var fileConsumers = Enumerable.Range(0, options.MaxDegreeOfParallelism)
            .Select(_ => Task.Run(async () =>
            {
                await foreach (var file in fileChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    try
                    {
                        var mime = Utils.GetMimeType(file);
                        var icon = Utils.GetFileIcon(mime);

                        var result = new FileScanResult
                        {
                            File = file,
                            Size = file.Length,
                            LastModifiedUtc = file.LastWriteTimeUtc,
                            DirectoryPath = file.DirectoryName!,
                            IsReadOnly = file.IsReadOnly,
                            IsHidden = (file.Attributes & FileAttributes.Hidden) != 0,
                            IsSystem = (file.Attributes & FileAttributes.System) != 0,
                            Sha256Hex = options.ComputeSha256
                                ? await Utils.ComputeSha256Async(file.FullName, cancellationToken)
                                : null,
                            MimeType = mime,
                            Icon = icon
                        };

                        options.OnFileFound?.Invoke(result);
                        await resultChannel.Writer.WriteAsync(result, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        options.OnError?.Invoke(ex);
                    }
                }
            }, cancellationToken)).ToList();

        // COMPLETION: close result channel when consumers finish
        var resultCloser = Task.Run(async () =>
        {
            await Task.WhenAll(fileConsumers);
            resultChannel.Writer.TryComplete();
        }, cancellationToken);

        // STREAM RESULTS: yield to caller
        await foreach (var result in resultChannel.Reader.ReadAllAsync(cancellationToken))
            yield return result;

        // Ensure producer/closer finished
        await producer;
        await resultCloser;
    }

    public static long CountFilesForScan(DirectoryInfo root, long minSize, string searchPattern, bool recursive, Action<Exception>? onError)
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

                    if (count % 1000 == 0)
                    {
                        // yield to keep UI responsive
                        Thread.Sleep(0);
                        AnsiConsole.MarkupLine($"[grey]Scanned {count:N0} files...[/]");
                    }
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            }

            // Only enqueue subdirectories when recursive is true
            if (recursive)
            {
                foreach (var dir in SafeDirs())
                    stack.Push(dir);
            }
        }

        return count;
    }

}
