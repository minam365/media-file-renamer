using Inamsoft.Libs.MetadataProviders.Abstractions;
using Microsoft.Extensions.Logging;

namespace Inamsoft.Libs.MetadataProviders
{
    public class FileMetadataProvider : BaseMetadataProvider<FileMetadataProvider>, IMetadataProvider<FileMetadata>
    {
        public FileMetadataProvider(ILogger<FileMetadataProvider> logger) : base(logger)
        {
        }

        public FileMetadata GetMetadata(string fileName)
        {
            return GetFileMetadata(fileName);
        }
    }
}
