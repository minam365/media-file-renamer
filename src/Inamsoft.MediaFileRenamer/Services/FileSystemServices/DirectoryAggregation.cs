namespace Inamsoft.MediaFileRenamer.Services.FileSystemServices;

public sealed class DirectoryAggregation
{
    public required string DirectoryPath { get; init; }
    public long TotalSize = 0;
    public long FileCount = 0;
    public long SubdirectoryCount = 0;
}
