using Inamsoft.Libs.MetadataProviders.Abstractions;
using Microsoft.Extensions.Logging;

namespace Inamsoft.Libs.MetadataProviders;

public class VideoFileMetadataProvider : BaseMetadataProvider<VideoFileMetadataProvider>, IVideoFileMetadataProvider
{
    public VideoFileMetadataProvider(ILogger<VideoFileMetadataProvider> logger) : base(logger)
    {
    }

    public VideoFileMetadata GetMetadata(string fileName)
    {
        throw new NotImplementedException();
    }
}