using Inamsoft.Libs.MetadataProviders;
using Inamsoft.Libs.MetadataProviders.Abstractions;
using System.Text;
using System.Text.RegularExpressions;

namespace Inamsoft.Libs.MediaFileRenaming;

public class FileNamingService : IFileNamingService
{
    private const string DefaultMediaTimestampFormat = "yyyyMMdd_HHmmss";

    private readonly IPhotoFileMetadataProvider _photoFileMetadataProvider;
    private readonly IVideoFileMetadataProvider _videoFileMetadataProvider;
    private readonly IFileMetadataProvider _fileMetadataProvider;
    private static readonly HashSet<string> _supportedPhotoFileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".tiff",
        ".bmp",
        ".gif",
        ".heic",
        ".webp",
        ".dng",
        ".raw",
        ".cr2",
        ".rw2",
    };

    private static readonly HashSet<string> _supportedVideoFileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4",
        ".mov",
        ".avi",
        ".mkv",
        ".wmv",
        ".flv",
        ".webm",
        ".mts",
        ".m2ts"
    };

    public FileNamingService(IPhotoFileMetadataProvider photoFileMetadataProvider, IVideoFileMetadataProvider videoFileMetadataProvider, IFileMetadataProvider fileMetadataProvider)
    {
        _photoFileMetadataProvider = photoFileMetadataProvider;
        _videoFileMetadataProvider = videoFileMetadataProvider;
        _fileMetadataProvider = fileMetadataProvider;
    }

    public static bool IsSupportedPhotoFileExtension(string fileExtension)
    {
        return _supportedPhotoFileExtensions.Contains(fileExtension);
    }

    public static bool IsSupportedVideoFileExtension(string fileExtension)
    {
        return _supportedVideoFileExtensions.Contains(fileExtension);
    }

    public static bool IsSupportedMediaFileExtension(string fileExtension)
    {
        return IsSupportedPhotoFileExtension(fileExtension) || IsSupportedVideoFileExtension(fileExtension);
    }

    public string GetTargetFilePath(string sourceFilePath, string targetFolderPath)
    {
        FileInfo sourceFileInfo = new(sourceFilePath);
        if (!sourceFileInfo.Exists)
        {
            throw new FileNotFoundException("Source file does not exist.", sourceFilePath);
        }

        if (_supportedPhotoFileExtensions.Contains(sourceFileInfo.Extension))
        {
            var photoMetadata = _photoFileMetadataProvider.GetMetadata(sourceFilePath);
            return BuildTargetFilePath(targetFolderPath, photoMetadata);
        }
        else if (_supportedVideoFileExtensions.Contains(sourceFileInfo.Extension))
        {
            var videoMetadata = _videoFileMetadataProvider.GetMetadata(sourceFilePath);
            return BuildTargetFilePath(targetFolderPath, videoMetadata);
        }

        // If neither photo nor video metadata exists, return the original file name
        var fileMetadata = _fileMetadataProvider.GetMetadata(sourceFilePath);
        return BuildTargetFilePath (targetFolderPath, fileMetadata);
    }

    public string MakeUniqueTargetFilePath(string sourceFilePath, string targetFolderPath)
    {
        var targetFilePath = GetTargetFilePath(sourceFilePath, targetFolderPath);

        DirectoryInfo targetDirectoryInfo = new(Path.GetDirectoryName(targetFilePath)!);
        if (!targetDirectoryInfo.Exists)
        {
            targetDirectoryInfo.Create();
        }

        FileInfo targetFileInfo = new(targetFilePath);
        if (!targetFileInfo.Exists)
        {
            return targetFilePath;
        }

        int counter = 1;
        string extension = targetFileInfo.Extension;
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(targetFileInfo.Name);

        while (File.Exists(targetFilePath))
        {

            string newFileName = $"{fileNameWithoutExtension} ({counter}){extension}";
            targetFilePath = Path.Combine(targetDirectoryInfo.FullName, newFileName);

            counter++;
        }

        return targetFilePath;
    }

    private string BuildTargetFilePath(string targetFolderPath, PhotoFileMetadata photoMetadata)
    {
        string newFileName = SanitizeFileName(GenerateDefaultFileName(photoMetadata));

        var pickedTimestamp = PickTimestamp(photoMetadata.TakenAt, photoMetadata.DigitizedAt, photoMetadata.FileMetadata.ModifiedAt);
        var yearFolderName = pickedTimestamp.ToString("yyyy");
        var monthFolderName = $"{pickedTimestamp.ToString("MM")}. {pickedTimestamp.ToString("MMMM")}";

        string targetFilePath = Path.Combine(targetFolderPath, yearFolderName, monthFolderName, newFileName);
        
        return targetFilePath;
    }

    private string BuildTargetFilePath(string targetFolderPath, VideoFileMetadata videoMetadata)
    {
        string newFileName = SanitizeFileName(GenerateDefaultFileName(videoMetadata));

        var pickedTimestamp = PickTimestamp(videoMetadata.CreatedAt, videoMetadata.ModifiedAt, videoMetadata.FileMetadata.ModifiedAt);
        var yearFolderName = pickedTimestamp.ToString("yyyy");
        var monthFolderName = $"{pickedTimestamp.ToString("MM")}. {pickedTimestamp.ToString("MMMM")}";

        string targetFilePath = Path.Combine(targetFolderPath, yearFolderName, monthFolderName, newFileName);

        return targetFilePath;
    }

    private string BuildTargetFilePath(string targetFolderPath, FileMetadata fileMetadata)
    {
        string newFileName = SanitizeFileName(GenerateDefaultFileName(fileMetadata));

        var pickedTimestamp = PickTimestamp(fileMetadata.CreatedAt, fileMetadata.ModifiedAt, fileMetadata.ModifiedAt);
        var yearFolderName = pickedTimestamp.ToString("yyyy");
        var monthFolderName = $"{pickedTimestamp.ToString("MM")}. {pickedTimestamp.ToString("MMMM")}";

        string targetFilePath = Path.Combine(targetFolderPath, yearFolderName, monthFolderName, newFileName);

        return targetFilePath;
    }

    public string GenerateDefaultFileName(PhotoFileMetadata photoFileMetadata, bool includeImageDimensions = true, bool includeGpsInfo = false)
    {
        var sb = new StringBuilder();

        string timestamp = FormatTimestamp(photoFileMetadata.TakenAt, photoFileMetadata.DigitizedAt, photoFileMetadata.FileMetadata.ModifiedAt);
        sb.Append(timestamp);

        var cameraInfo = FormatCameraInfo(photoFileMetadata.CameraMake, photoFileMetadata.CameraModel);
        if (!string.IsNullOrEmpty(cameraInfo))
        {
            sb.Append($"_({cameraInfo})");
        }

        if (includeImageDimensions)
        {
            string? imageDimensions = includeImageDimensions ? FormatMediaDimensions(photoFileMetadata.Width, photoFileMetadata.Height) : null;
            if (!string.IsNullOrEmpty(imageDimensions))
            {
                sb.Append($"_({imageDimensions})");
            }
        }

        if (includeGpsInfo)
        {
            string? gpsCoordinates = FormatGpsCoordinates(photoFileMetadata.Latitude, photoFileMetadata.Longitude);
            if (!string.IsNullOrEmpty(gpsCoordinates))
            {
                sb.Append($"_(Gps={gpsCoordinates})");
            }
        }

        sb.Append($"_{photoFileMetadata.FileMetadata.Name}");

        return sb.ToString();
    }

    public string GenerateDefaultFileName(VideoFileMetadata videoFileMetadata, bool includeImageDimensions = true)
    {
        var sb = new StringBuilder();
        string timestamp = FormatTimestamp(videoFileMetadata.CreatedAt, videoFileMetadata.ModifiedAt, videoFileMetadata.FileMetadata.ModifiedAt);
        sb.Append(timestamp);
        if (includeImageDimensions)
        {
            string? mediaDimensions = FormatMediaDimensions(videoFileMetadata.Width, videoFileMetadata.Height);
            if (!string.IsNullOrEmpty(mediaDimensions))
            {
                sb.Append($"_({mediaDimensions})");
            }
        }
        sb.Append($"_{videoFileMetadata.FileMetadata.Name}");
        return sb.ToString();
    }

    public string GenerateDefaultFileName(FileMetadata fileMetadata)
    {
        var sb = new StringBuilder();
        string timestamp = FormatTimestamp(fileMetadata.CreatedAt, fileMetadata.ModifiedAt, fileMetadata.ModifiedAt);
        sb.Append(timestamp);
        sb.Append($"_{fileMetadata.Name}");
        return sb.ToString();
    }

    static string SanitizePath(string input, string replacementChar=" ")
    {
        // Combine invalid path and file name characters
        string invalidChars = Regex.Escape(new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars()));
        string invalidCharsPattern = $"[{invalidChars}]";

        // Replace invalid characters with the specified replacement character
        return Regex.Replace(input, invalidCharsPattern, replacementChar);
    }

    static string SanitizeFileName(string input, string replacementChar = "-")
    {
        // Split the input by invalid filename chars
        var parts = input.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries);
        
        var sanitized = string.Join(replacementChar, parts);
        return sanitized;
    }

    static string FormatTimestamp(DateTime? timestamp1, DateTime? timestamp2, DateTime fallbackDateTime)
    {
        var pickedTimestamp = PickTimestamp(timestamp1, timestamp2, fallbackDateTime);
        return pickedTimestamp.ToString(DefaultMediaTimestampFormat);
    }

    static string? FormatCameraInfo(string? make, string? model)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(make))
        {
            parts.Add(make);
        }
        if (!string.IsNullOrEmpty(model))
        {
            parts.Add(model);
        }
        return string.Join(" ", parts).Trim();
    }


    static string? FormatMediaDimensions(int? width, int? height)
    {
        if (width.HasValue && height.HasValue)
        {
            return $"{width.Value}x{height.Value}";
        }
        return null;
    }

    static string? FormatGpsCoordinates(string? latitude, string? longitude)
    {
        if (!string.IsNullOrEmpty(latitude) && !string.IsNullOrEmpty(longitude))
        {
            return $"{latitude} {longitude}";
        }
        return null;

    }

    static string GetFormattedTimestampYear(DateTime? timestamp1, DateTime? timestamp2, DateTime fallbackDateTime)
    {
        var pickedTimestamp = PickTimestamp(timestamp1, timestamp2, fallbackDateTime);
        return pickedTimestamp.ToString("yyyy");
    }

    static string GetFormattedTimestampMonth(DateTime? timestamp1, DateTime? timestamp2, DateTime fallbackDateTime)
    {
        var pickedTimestamp = PickTimestamp(timestamp1, timestamp2, fallbackDateTime);
        return $"{pickedTimestamp.ToString("MM")}. {pickedTimestamp.ToString("MMMM")}";
    }

    static DateTime PickTimestamp(DateTime? timestamp1, DateTime? timestamp2, DateTime fallbackDateTime)
    {
        if (timestamp1.HasValue)
        {
            return timestamp1.Value;
        }
        if (timestamp2.HasValue)
        {
            return timestamp2.Value;
        }
        return fallbackDateTime;
    }
}
