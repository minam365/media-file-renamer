using Inamsoft.MediaFileRenamer.Services.MetadataProviderServices.Models;

namespace Inamsoft.MediaFileRenamer.Services.MetadataProviderServices;

/// <summary>
/// Defines a provider that supplies metadata information for files.
/// </summary>
/// <remarks>Implementations of this interface enable retrieval of file-specific metadata, such as attributes,
/// timestamps, or custom properties. This interface extends <see cref="IMetadataProvider{FileMetadata}"/>, allowing
/// integration with systems that consume generic metadata providers.</remarks>
public interface IFileMetadataProvider : IMetadataProvider<FileMetadata>
{
    
}