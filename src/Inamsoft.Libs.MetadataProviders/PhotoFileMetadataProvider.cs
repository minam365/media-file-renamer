using Inamsoft.Libs.MetadataProviders.Abstractions;
using Microsoft.Extensions.Logging;

namespace Inamsoft.Libs.MetadataProviders;

public class PhotoFileMetadataProvider : BaseMetadataProvider<PhotoFileMetadataProvider>, IPhotoFileMetadataProvider
{
    public PhotoFileMetadataProvider(ILogger<PhotoFileMetadataProvider> logger) : base(logger)
    {
    }

    public PhotoFileMetadata GetMetadata(string fileName)
    {
        var fileMetadata = GetFileMetadata(fileName);
        
        throw new NotImplementedException();
    }
}