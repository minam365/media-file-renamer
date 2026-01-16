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
        var dirChannel = Channel.CreateUnbounded<DirectoryInfo>();
        var fileChannel = Channel.CreateUnbounded<FileInfo>();
        var resultChannel = Channel.CreateUnbounded<FileScanResult>();

        // PRODUCER: directory traversal
        var dirProducer = Task.Run(async () =>
        {
            try
            {
                await dirChannel.Writer.WriteAsync(root, cancellationToken);

                var workers = Enumerable.Range(0, options.MaxDegreeOfParallelism)
                    .Select(_ => Task.Run(async () =>
                    {
                        await foreach (var dir in dirChannel.Reader.ReadAllAsync(cancellationToken))
                        {
                            options.OnDirectoryEntered?.Invoke(dir);

                            IEnumerable<FileInfo> SafeFiles()
                            {
                                try { return dir.EnumerateFiles(); }
                                catch (Exception ex) { options.OnError?.Invoke(ex); return Enumerable.Empty<FileInfo>(); }
                            }

                            IEnumerable<DirectoryInfo> SafeDirs()
                            {
                                try { return dir.EnumerateDirectories(); }
                                catch (Exception ex) { options.OnError?.Invoke(ex); return Enumerable.Empty<DirectoryInfo>(); }
                            }

                            foreach (var file in SafeFiles())
                            {
                                if (file.Length >= options.MinFileSizeInBytes)
                                    await fileChannel.Writer.WriteAsync(file, cancellationToken);
                            }

                            foreach (var sub in SafeDirs())
                                await dirChannel.Writer.WriteAsync(sub, cancellationToken);
                        }
                    }, cancellationToken));

                await Task.WhenAll(workers);
            }
            finally
            {
                dirChannel.Writer.TryComplete();
                fileChannel.Writer.TryComplete();
            }
        }, cancellationToken);

        // CONSUMERS: file processing
        var fileConsumers = Enumerable.Range(0, options.MaxDegreeOfParallelism)
            .Select(_ => Task.Run(async () =>
            {
                await foreach (var file in fileChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    try
                    {
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
                                ? await HashingHelper.ComputeSha256Async(file.FullName, cancellationToken)
                                : null
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

        // COMPLETION: close result channel when all consumers finish
        var resultCloser = Task.Run(async () =>
        {
            await Task.WhenAll(fileConsumers);
            resultChannel.Writer.TryComplete();
        }, cancellationToken);

        // STREAM RESULTS: this is the only place we yield
        await foreach (var result in resultChannel.Reader.ReadAllAsync(cancellationToken))
            yield return result;

        await dirProducer;
        await resultCloser;
    }
}
