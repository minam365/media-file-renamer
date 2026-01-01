using Inamsoft.Libs.MetadataProviders.Abstractions;
using Microsoft.Extensions.Logging;

namespace Inamsoft.Libs.MetadataProviders
{
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

        public FileMetadata GetMetadata(string fileName)
        {
            return GetFileMetadata(fileName);
        }
    }
}
