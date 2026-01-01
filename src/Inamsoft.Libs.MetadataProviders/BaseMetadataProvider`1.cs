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

        FileInfo fileInfo = new(filePath);
        var metadata = new FileMetadata(fileInfo)
        {
            FilePath = fileInfo.FullName,
            FileName = fileInfo.Name,
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath),
            FileExtension = fileInfo.Extension,
            DirectoryPath = fileInfo.DirectoryName ?? string.Empty,    
            Exists = fileInfo.Exists
        };

        if (!fileInfo.Exists) 
            return metadata;

        metadata.DirectoryName = fileInfo.DirectoryName ?? string.Empty;

        metadata.CreatedAt = fileInfo.CreationTime;
        metadata.ModifiedAt = fileInfo.LastWriteTime;

        metadata.Length = fileInfo.Length;
        
        return metadata;
    }
}