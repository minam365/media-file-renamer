using Inamsoft.Libs.MetadataProviders.Abstractions;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
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
        var photoMetadata = new PhotoFileMetadata()
        {
            FileMetadata = fileMetadata
        };
        
        var directories = ImageMetadataReader.ReadMetadata(fileName);
        
        var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        if (subIfdDirectory is not null)
        {
            var subIfdDirDescriptor = new ExifSubIfdDescriptor(subIfdDirectory);
            if (subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var originalDateTime))
                photoMetadata.TakenAt = originalDateTime;
            if (subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out var digitizedDateTime))
                photoMetadata.TakenAt = digitizedDateTime;
        }
        
        var id0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
        if (id0Directory is not null)
        {
            var id0DirDescriptor = new ExifIfd0Descriptor(id0Directory);
        }
        
        return photoMetadata;
    }
}