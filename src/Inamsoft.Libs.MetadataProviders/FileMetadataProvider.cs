using CommunityToolkit.Diagnostics;
using Inamsoft.Libs.MetadataProviders.Abstractions;
using Microsoft.Extensions.Logging;

namespace Inamsoft.Libs.MetadataProviders;

/// <summary>
/// Provides functionality to retrieve metadata for files using their file paths.
/// </summary>
/// <remarks>
/// This class extends <see cref="BaseMetadataProvider{FileMetadataProvider}"/> to leverage
/// the base functionality for metadata retrieval and implements <see cref="IMetadataProvider{TMetadata}"/>
/// to provide specific metadata processing for files using the <see cref="FileMetadata"/> container.
/// </remarks>
public class FileMetadataProvider : BaseMetadataProvider<FileMetadataProvider>, IFileMetadataProvider
{
    /// <summary>
    /// Represents a provider for retrieving metadata of files based on their paths or names.
    /// </summary>
    /// <remarks>
    /// The <see cref="FileMetadataProvider"/> class provides functionality to retrieve file information
    /// encapsulated within instances of the <see cref="FileMetadata"/> class. It relies on the
    /// <see cref="BaseMetadataProvider{TMetadataProvider}"/> to access underlying file metadata operations
    /// and implements <see cref="IMetadataProvider{TMetadata}"/> for metadata management specific to
    /// file-based resources.
    /// </remarks>
    public FileMetadataProvider(ILogger<FileMetadataProvider> logger) : base(logger)
    {
    }

    /// <summary>
    /// Retrieves metadata information for the specified file, including its path, name, extension, directory,
    /// existence, timestamps, and size.
    /// </summary>
    /// <param name="filePath">The name or path of the file for which to retrieve metadata. Cannot be null, empty, or consist only of
    /// white-space characters.</param>
    /// <returns>A <see cref="FileMetadata"/> instance containing metadata about the specified file. If the file does not exist,
    /// the returned object will indicate <see langword="false"/> for <c>Exists</c> and default values for other
    /// properties.</returns>
    public FileMetadata GetMetadata(string filePath)
    {
        Logger.LogDebug("Getting metadata info from the file: {Path}", filePath);

        Guard.IsNotNullOrWhiteSpace(filePath, nameof(filePath));

        FileInfo fileInfo = new(filePath);
        var metadata = new FileMetadata(fileInfo)
        {
            Path = fileInfo.FullName,
            Name = fileInfo.Name,
            NameWithoutExtension = Path.GetFileNameWithoutExtension(filePath),
            Extension = fileInfo.Extension,
            DirectoryName = fileInfo.DirectoryName ?? string.Empty,
            Exists = fileInfo.Exists
        };

        if (fileInfo.Exists)
        {
            metadata.CreatedAt = fileInfo.CreationTime;
            metadata.ModifiedAt = fileInfo.LastWriteTime;
            metadata.Length = fileInfo.Length;
        }

        Logger.LogDebug("Metadata info retrieved successfully from the file: {Path}", filePath);

        return metadata;
    }
}
