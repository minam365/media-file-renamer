using Inamsoft.Libs.MetadataProviders.Abstractions;
using Inamsoft.Libs.MetadataProviders.Extensions;
using Inamsoft.Libs.MetadataProviders.Helpers;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;
using Microsoft.Extensions.Logging;

namespace Inamsoft.Libs.MetadataProviders;

public class PhotoFileMetadataProvider : BaseMetadataProvider<PhotoFileMetadataProvider>, IPhotoFileMetadataProvider
{
    private readonly IFileMetadataProvider _fileMetadataProvider;

    public PhotoFileMetadataProvider(IFileMetadataProvider fileMetadataProvider, ILogger<PhotoFileMetadataProvider> logger) : base(logger)
    {
        _fileMetadataProvider = fileMetadataProvider;
    }

    public PhotoFileMetadata GetMetadata(string filePath)
    {
        var fileMetadata = _fileMetadataProvider.GetMetadata(filePath);
        var photoMetadata = new PhotoFileMetadata()
        {
            FileMetadata = fileMetadata
        };

        if (!fileMetadata.Exists)
            return photoMetadata;

        if (!TryReadMetadata(filePath, out GetMetadataResult getMetadataResult))
        {
            photoMetadata.TakenAt = fileMetadata.ModifiedAt;
            photoMetadata.DigitizedAt = fileMetadata.ModifiedAt;

            return photoMetadata;
        }

        var directories = getMetadataResult.Directories;
        var dirToTagsMap = getMetadataResult.DirectoryToTagsMap;
        var tagList = getMetadataResult.MetadataTags;

        // Get camera make and model
        var cameraInfo = GetCameraInfo(tagList);
        if (cameraInfo.HasValue)
        {
            photoMetadata.CameraMake = cameraInfo.Value.Make;
            photoMetadata.CameraModel = cameraInfo.Value.Model;
        }

        (DateTime TakenAt, DateTime DigitizedAt) = GetTimestamps(directories!, tagList, photoMetadata.FileMetadata);
        photoMetadata.TakenAt = TakenAt != DateTime.MinValue ? TakenAt : null;
        photoMetadata.DigitizedAt = DigitizedAt != DateTime.MinValue ? DigitizedAt : null;

        // Get image dimensions
        var dimensions = GetImageDimensions(getMetadataResult);
        if (dimensions.HasValue)
        {
            photoMetadata.Height = dimensions.Value.Height;
            photoMetadata.Width = dimensions.Value.Width;
        }

        // Get GPS directory (if available)
        (string Latitude, string Longitude)? gpsCoordinates = GetGpsCoordinates(directories!);
        if (gpsCoordinates.HasValue)
        {
            photoMetadata.Latitude = gpsCoordinates.Value.Latitude;
            photoMetadata.Longitude = gpsCoordinates.Value.Longitude;
        }

        return photoMetadata;
    }

    private (DateTime TakenAt, DateTime DigitizedAt) GetTimestamps(IReadOnlyList<MetadataExtractor.Directory> directories, IReadOnlyList<MetadataTag> tagList, FileMetadata fileMetadata)
    {
        var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        if (subIfdDirectory is not null)
        {
            var subIfdDirDescriptor = new ExifSubIfdDescriptor(subIfdDirectory);

            if (subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var originalDateTime) &&
                subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out var digitizedDateTime))
                return (originalDateTime, digitizedDateTime);
        }

        var takenAtTag = tagList.FirstOrDefault(t => t.Type == ExifDirectoryBase.TagDateTimeOriginal);
        var digitizedAtTag = tagList.FirstOrDefault(t => t.Type == ExifDirectoryBase.TagDateTimeDigitized);

        DateTime? takenAt = default;
        DateTime? digitizedAt = default;
        if (takenAtTag.HasValue && DateTimeHelper.TryParseDateTime(takenAtTag.Value!, out var takenAtValue))
        {
            takenAt = takenAtValue;
        }
        if (digitizedAtTag.HasValue && DateTimeHelper.TryParseDateTime(digitizedAtTag.Value!, out var digitiuedAtValue))
        {
            digitizedAt = digitiuedAtValue;
        }

        if (takenAt.HasValue && digitizedAt.HasValue)
            return (takenAt.Value, digitizedAt.Value);
        if (takenAt.HasValue && !digitizedAt.HasValue)
            return (takenAt.Value, takenAt.Value);
        if (!takenAt.HasValue && digitizedAt.HasValue)
            return (digitizedAt.Value, digitizedAt.Value);

        return (fileMetadata.ModifiedAt, fileMetadata.ModifiedAt);
    }

    (string? Make, string? Model)? GetCameraInfo(IReadOnlyList<MetadataTag> tags)
    {
        var make = tags.Where(t => t.Type == ExifDirectoryBase.TagMake).Select(t => t.Value).FirstOrDefault();
        var model = tags.Where(t => t.Type == ExifDirectoryBase.TagModel).Select(t => t.Value).FirstOrDefault();
        return (make, model);
    }

    (string Latitude, string Longitude)? GetGpsCoordinates(IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        var gpsDirectory = directories.OfType<GpsDirectory>().FirstOrDefault();
        if (gpsDirectory is null)
            return null;

        if (gpsDirectory.TryGetGeoLocation(out GeoLocation geoLocation))
        {
            return (geoLocation.Latitude.ToString(), geoLocation.Longitude.ToString());
        }
        return null;
    }

    (int Height, int Width)? GetImageDimensions(GetMetadataResult getMetadataResult)
    {
        var directories = getMetadataResult.Directories;
        if (directories is null || directories.Count == 0)
            return null;

        // Panasonic Raw Exif IFD0
        //
        if (getMetadataResult.DirectoryToTagsMap.ContainsKey("PanasonicRaw Exif IFD0"))
        {
            var panasonicTags = getMetadataResult.DirectoryToTagsMap["PanasonicRaw Exif IFD0"];
            var panaHeightTag = panasonicTags.FirstOrDefault(t => t.Name.Equals("Sensor Height", StringComparison.InvariantCultureIgnoreCase)); // Tag for Image Height
            var panaWidthTag = panasonicTags.FirstOrDefault(t => t.Name.Equals("Sensor Width", StringComparison.InvariantCultureIgnoreCase));   // Tag for Image Width
            if (panaHeightTag.HasValue && panaWidthTag.HasValue)
            {
                if (int.TryParse(panaHeightTag.Value, out var height) &&
                    int.TryParse(panaWidthTag.Value, out var width))
                {
                    return (height, width);
                }
                else
                {
                    height = ParseImageDimensionValue(panaHeightTag.Value);
                    width = ParseImageDimensionValue(panaWidthTag.Value);
                    if (height > 0 && width > 0)
                    {
                        return (height, width);
                    }
                }
            }
        }

        var jpegDirectory = directories.OfType<JpegDirectory>().FirstOrDefault();
        if (jpegDirectory is not null)
        {
            if (jpegDirectory.TryGetInt32(JpegDirectory.TagImageHeight, out var height) &&
                jpegDirectory.TryGetInt32(JpegDirectory.TagImageWidth, out var width))
            {
                return (height, width);
            }
        }

        var pngDirectory = directories.OfType<PngDirectory>().FirstOrDefault();
        if (pngDirectory is not null)
        {
            if (pngDirectory.TryGetInt32(PngDirectory.TagImageHeight, out var height) &&
                pngDirectory.TryGetInt32(PngDirectory.TagImageWidth, out var width))
            {
                return (height, width);
            }
        }


        var heightTag = getMetadataResult.MetadataTags.FirstOrDefault(t => t.Type == ExifDirectoryBase.TagImageHeight);
        var widthTag = getMetadataResult.MetadataTags.FirstOrDefault(t => t.Type == ExifDirectoryBase.TagImageWidth);

        if (heightTag.HasValue && widthTag.HasValue)
        {
            if (int.TryParse(heightTag.Value, out var height) &&
                int.TryParse(widthTag.Value, out var width))
            {
                return (height, width);
            }
            else
            {
                height = ParseImageDimensionValue(heightTag.Value);
                width = ParseImageDimensionValue(widthTag.Value);

                if (height > 0 && width > 0)
                {
                    return (height, width);
                }
            }
        }



        return null;
    }

    static int ParseImageDimensionValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return -1;

        // Some metadata values might include units (e.g., "1024 pixels"), so we need to extract the numeric part.
        var numericPart = new string(value.TakeWhile(c => char.IsDigit(c)).ToArray());
        if (int.TryParse(numericPart, out var dimension))
        {
            return dimension;
        }
        return -1;
    }


}