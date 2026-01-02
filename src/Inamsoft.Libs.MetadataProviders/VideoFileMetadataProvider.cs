using Inamsoft.Libs.MetadataProviders.Abstractions;
using Inamsoft.Libs.MetadataProviders.Extensions;
using Inamsoft.Libs.MetadataProviders.Helpers;
using MetadataExtractor;
using Microsoft.Extensions.Logging;

namespace Inamsoft.Libs.MetadataProviders;

public class VideoFileMetadataProvider : BaseMetadataProvider<VideoFileMetadataProvider>, IVideoFileMetadataProvider
{
    private readonly IFileMetadataProvider _fileMetadataProvider;

    public VideoFileMetadataProvider(IFileMetadataProvider fileMetadataProvider, ILogger<VideoFileMetadataProvider> logger) : base(logger)
    {
        _fileMetadataProvider = fileMetadataProvider;
    }

    public VideoFileMetadata GetMetadata(string filePath)
    {
        var fileMetadata = _fileMetadataProvider.GetMetadata(filePath);
        var videoFileMetadata = new VideoFileMetadata()
        {
            FileMetadata = fileMetadata
        };

        if (!fileMetadata.Exists)
            return videoFileMetadata;

        var directories = ImageMetadataReader.ReadMetadata(filePath);
        var dirToTagsMap = directories.ToTagDictionary();
        var tagList = directories.ToTagList();

        (DateTime? CreatedAt, DateTime? ModifiedAt) timestamps = GetTimestamps(tagList);
        videoFileMetadata.CreatedAt = timestamps.CreatedAt;
        videoFileMetadata.ModifiedAt = timestamps.ModifiedAt;

        var durationTag = tagList.FirstOrDefault(t => t.Name.Equals("Duration", StringComparison.OrdinalIgnoreCase));
        if (durationTag.HasValue && TimeSpan.TryParse(durationTag.Value, out var duration))
        {
            videoFileMetadata.Duration = duration;
        }

        (int Height, int Width)? dimensions = GetDimensions(tagList);
        if (dimensions.HasValue)
        {
            videoFileMetadata.Height = dimensions.Value.Height;
            videoFileMetadata.Width = dimensions.Value.Width;
        }



        return videoFileMetadata;
    }

    private (DateTime? CreatedAt, DateTime? ModifiedAt) GetTimestamps(IReadOnlyList<MetadataTag> tagList)
    {
        var createdAtTag = tagList.FirstOrDefault(t => t.Name.Equals("Created", StringComparison.OrdinalIgnoreCase) ||
                                                      t.Name.Equals("Created Date", StringComparison.OrdinalIgnoreCase));
        DateTime? createdAt = null;
        if (createdAtTag.HasValue && DateTimeHelper.TryParseDateTime(createdAtTag.Value!, out var createdAtValue))
        {
            createdAt = createdAtValue;
        }

        var modifiedAtTag = tagList.FirstOrDefault(t => t.Name.Equals("Modified", StringComparison.OrdinalIgnoreCase) ||
                                                       t.Name.Equals("Modified Date", StringComparison.OrdinalIgnoreCase));
        DateTime? modifiedAt = null;
        if (modifiedAtTag.HasValue && DateTimeHelper.TryParseDateTime(modifiedAtTag.Value!, out var modifiedAtValue))
        {
            modifiedAt = modifiedAtValue;
        }
        return (createdAt, modifiedAt);
    }

    private (int Height, int Width) GetDimensions(IReadOnlyList<MetadataTag> tagList)
    {
        var heightTag = tagList.FirstOrDefault(t => t.Name.Equals("Height", StringComparison.OrdinalIgnoreCase));
        int height = heightTag.HasValue && int.TryParse(heightTag.Value, out var h) ? h : -1;

        var widthTag = tagList.FirstOrDefault(t => t.Name.Equals("Width", StringComparison.OrdinalIgnoreCase));
        int width = widthTag.HasValue && int.TryParse(widthTag.Value, out var w) ? w : -1;

        return (height, width);
    }
}