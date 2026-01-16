using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Inamsoft.MediaFileRenamer.Services.FileSystemServices;

public static class ParallelFileScanner
{
    public static IEnumerable<FileScanResult> Scan(
        DirectoryInfo root,
        FileScanOptions options,
        CancellationToken cancellationToken)
    {
        var results = new ConcurrentBag<FileScanResult>(); 
        
        using var dirs = new BlockingCollection<DirectoryInfo>(new ConcurrentQueue<DirectoryInfo>()); 
        
        dirs.Add(root);

        Parallel.ForEach(
            dirs.GetConsumingEnumerable(cancellationToken),
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = options.MaxDegreeOfParallelism
            },
            dir =>
            {
                options.OnDirectoryEntered?.Invoke(dir);

                IEnumerable<FileInfo> SafeFiles()
                {
                    try { return dir.EnumerateFiles(); }
                    catch (Exception ex) { options.OnError?.Invoke(ex); return []; }
                }

                IEnumerable<DirectoryInfo> SafeDirs()
                {
                    try { return dir.EnumerateDirectories(); }
                    catch (Exception ex) { options.OnError?.Invoke(ex); return []; }
                }

                foreach (var file in SafeFiles())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        if (file.Length < options.MinFileSizeInBytes)
                            continue;

                        var result = new FileScanResult
                        {
                            File = file,
                            Size = file.Length,
                            LastModifiedUtc = file.LastWriteTimeUtc,
                            DirectoryPath = file.DirectoryName!,
                            IsReadOnly = file.IsReadOnly,
                            IsHidden = (file.Attributes & FileAttributes.Hidden) != 0,
                            IsSystem = (file.Attributes & FileAttributes.System) != 0
                        };

                        results.Add(result);
                        options.OnFileFound?.Invoke(result);
                    }
                    catch (Exception ex)
                    {
                        options.OnError?.Invoke(ex);
                    }
                }

                foreach (var sub in SafeDirs())
                    dirs.Add(sub);
            });

        dirs.CompleteAdding();

        return results;
    }
}

