namespace Inamsoft.MediaFileRenamer.Services.FileSystemServices;

public record FileScanOptions
{
    public long MinFileSizeInBytes { get; init; } = 0;
    public int MaxDegreeOfParallelism { get; init; } = Environment.ProcessorCount;
    public bool ComputeSha256 { get; init; } = false;

    public string SearchPattern { get; init; } = "*"; // default: all files

    // New: whether to enumerate directories recursively. Default true preserves current behavior.
    public bool Recursive { get; init; } = true;

    public Action<Exception>? OnError { get; init; }
    public Action<DirectoryInfo>? OnDirectoryEntered { get; init; }
    public Action<FileScanResult>? OnFileFound { get; init; }

    public Action<FileScanResult>? ProcessFile { get; init; }
}
