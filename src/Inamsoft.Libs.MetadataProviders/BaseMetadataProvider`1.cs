using CommunityToolkit.Diagnostics;
using Inamsoft.Libs.MetadataProviders.Abstractions;
using Microsoft.Extensions.Logging;

namespace Inamsoft.Libs.MetadataProviders;

public abstract class BaseMetadataProvider<TMetadataProvider>
{
    public ILogger<TMetadataProvider> Logger { get; }

    protected BaseMetadataProvider(ILogger<TMetadataProvider> logger)
    {
        Logger = logger;
    }


    protected virtual FileMetadata GetFileMetadata(string filePath)
    {
        Logger.LogInformation("Getting info for file: {FilePath}", filePath);

        Guard.IsNotNullOrWhiteSpace(filePath, nameof(filePath));

        var metadata = new FileMetadata
        {
            FilePath = filePath,
            FileName = Path.GetFileName(filePath),
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath),
            FileExtension = Path.GetExtension(filePath),
            DirectoryPath = Path.GetDirectoryName(filePath) ?? string.Empty,
        };

        if (!File.Exists(filePath)) 
            return metadata;
        
        var fi = new FileInfo(filePath);

        metadata.DirectoryName = fi.Directory?.Name ?? string.Empty;

        metadata.CreatedAt = fi.CreationTime;
        metadata.ModifiedAt = fi.LastWriteTime;

        metadata.Length = fi.Length;
        return metadata;
    }
}