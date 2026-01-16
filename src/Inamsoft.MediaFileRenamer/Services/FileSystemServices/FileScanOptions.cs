namespace Inamsoft.MediaFileRenamer.Services.FileSystemServices;

public record FileScanOptions
{
    public long MinFileSizeInBytes { get; init; } = 0;
    public int MaxDegreeOfParallelism { get; init; } = Environment.ProcessorCount;
    public bool ComputeSha256 { get; init; } = false;

     public string SearchPattern { get; init; } = "*"; // default: all files

    public Action<Exception>? OnError { get; init; }
    public Action<DirectoryInfo>? OnDirectoryEntered { get; init; }
    public Action<FileScanResult>? OnFileFound { get; init; }
}
