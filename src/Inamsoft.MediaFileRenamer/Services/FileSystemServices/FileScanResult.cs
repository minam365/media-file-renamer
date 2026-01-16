namespace Inamsoft.MediaFileRenamer.Services.FileSystemServices;

public sealed class FileScanResult
{
    public required FileInfo File { get; init; }
    public required long Size { get; init; }
    public required DateTime LastModifiedUtc { get; init; }
    public required string DirectoryPath { get; init; }
    public required bool IsReadOnly { get; init; }
    public required bool IsHidden { get; init; }
    public required bool IsSystem { get; init; }
    public string? Sha256Hex { get; init; }
}




