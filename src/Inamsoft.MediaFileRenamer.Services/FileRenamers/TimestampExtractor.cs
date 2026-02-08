using Inamsoft.MediaFileRenamer.Services.Abstractions;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Formats.Xmp;
using System.Globalization;
using System.Text.RegularExpressions;
using MetadataExtractor.Formats.Png;
using TinyResult;
using TinyResult.Enums;

namespace Inamsoft.MediaFileRenamer.Services.FileRenamers;

public partial class TimestampExtractor
{
    #region Declarations

    private static readonly string[] TimestampPatterns =
    [
        "yyyyMMdd_HHmmss",
        "yyyyMMddHHmmss",
        "yyyy-MM-dd_HHmmss",
        "yyyy-MM-dd HHmmss"
    ];

    #endregion

    public static bool TryExtractTimestamp(FileInfo fileInfo, out DateTime? timestamp)
    {
        var result = ExtractTimestamp(fileInfo);
        if (result.IsSuccess)
        {
            timestamp = result.Value.ResultingTimestamp;
            return true;
        }

        timestamp = null;
        return false;
    }

    public static Result<TimestampResult> ExtractTimestamp(FileInfo file)
    {
        var request = new GetFileTimestampRequest(file);

        var tryResult = ResultPipeline<GetFileTimestampResponse>
                .Start(Result<GetFileTimestampResponse>.Success(
                    new GetFileTimestampResponse(DateTime.Now, TimestampSource.None, "")))
                // 1. Camera-specific heuristics
                .Then(p => GetCanonPatternFromName(request))
                .Then(p => GetNikonPatternFromName(request))
                .Then(p => GetSonyPatternFromName(request))
                .Then(p => GetSmartphonePatternFromName(request))
                // 2. Generic heuristics
                .Then(p => GetGenericTimestampFromName(request))
                .Then(p => GetEmbeddedTimestampFromName(request))
                .Then(p => GetEpochSecondsFromName(request))
                .Then(p => GetEpochMillisecondsFromName(request))
                // 3. AVCHD (.mts / .m2ts)
                .Then(p => GetTimestampFromAvchd(request))
                .Build()
            ;

        var combinedResult = ResultCombiner.FirstSuccess(
            // 1. Camera-specific heuristics
            () => GetCanonPatternFromName(request),
            () => GetSmartphonePatternFromName(request),
            () => GetNikonPatternFromName(request),
            () => GetSonyPatternFromName(request),
            
            // 2. Generic heuristics
            () => GetGenericTimestampFromName(request),
            () => GetEmbeddedTimestampFromName(request),
            () => GetEpochSecondsFromName(request),
            () => GetEpochMillisecondsFromName(request),
            
            // 3. AVCHD (.mts / .m2ts)
            () => GetTimestampFromAvchd(request),
            
            // 4.Video metadata (MP4/MOV)
            () => GetTimestampFromVideoFileMetadata(request),
            
            // 5. Metadata (EXIF) fallback (photos only)
            () => GetTimestampFromPhotoFileMetadata(request),
            () => GetTimestampFromPngMetadata(request),
            () => GetTimestampFromPsdMetadata(request),
            
            // 6. Sidecar files
            () => GetTimestampFromSidecars(request)
        );

        if (combinedResult.IsSuccess)
        {
            var getFileNameResult = tryResult.Value;
            var result = new TimestampResult()
            {
                OriginalName = file.Name,
                ResultingTimestamp = getFileNameResult.Timestamp,
                Source = getFileNameResult.Source,
                Response = getFileNameResult
            };
            return result;
        }
        else
        {
            return Result<TimestampResult>.Failure(ErrorCode.NotFound,
                $"Failed to extract timestamp from file '{file.Name}': {tryResult.Error}");
        }
    }

    #region Public Methods for Extracting Camera Specific Timestamps from Filename

    /// <summary>
    /// Canon (IMG_YYYYMMDD_HHMMSS)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static Result<GetFileTimestampResponse> GetCanonPatternFromName(GetFileTimestampRequest request)
    {
        var name = request.FileNameWithoutExtension;
        var match = MatchCanonCamFilePattern().Match(name);
        if (!match.Success)
            return Result<GetFileTimestampResponse>.Failure(ErrorCode.NotFound,
                $"No Canon camera specific timestamp pattern found in name '{name}'.");

        var combined = $"{match.Groups[1].Value}_{match.Groups[2].Value}";
        if (!DateTime.TryParseExact(
                combined,
                "yyyyMMdd_HHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDateTime))
        {
            return Result<GetFileTimestampResponse>.Failure(ErrorCode.NotFound,
                $"Canon camera specific timestamp pattern found but failed to parse: {combined}");
        }

        return Result<GetFileTimestampResponse>.Success(new GetFileTimestampResponse()
        {
            Timestamp = parsedDateTime,
            Source = TimestampSource.FileName,
            StatusMessage = "Canon camera specific timestamp extracted from filename."
        });
    }

    /// <summary>
    /// Nikon (DSC_#### → no timestamp)
    /// </summary>
    /// <remarks>Nikon DSLRs often use counters only. We return null.</remarks>
    /// <param name="request"></param>
    /// <returns></returns>
    public static Result<GetFileTimestampResponse> GetNikonPatternFromName(GetFileTimestampRequest request)
    {
        var name = request.FileNameWithoutExtension;
        return Result<GetFileTimestampResponse>.Failure(ErrorCode.NotFound, MatchNikonCamFilePattern().IsMatch(name)
            ? $"Nikon camera specific timestamp pattern found but it doesn't have any timestamp in name '{name}'."
            : $"No Nikon camera specific timestamp pattern found in name '{name}'.");
    }

    /// <summary>
    /// Sony (DSC0#### or DSC0#### → no timestamp)
    /// </summary>
    /// <remarks>Sony DSLRs often use counters prefixed with DSC0 only. We return null.</remarks>
    /// <param name="request"></param>
    /// <returns></returns>
    public static Result<GetFileTimestampResponse> GetSonyPatternFromName(GetFileTimestampRequest request)
    {
        var name = request.FileNameWithoutExtension;
        return Result<GetFileTimestampResponse>.Failure(ErrorCode.NotFound, MatchSonyCamFilePattern().IsMatch(name)
            ? $"Sony camera specific timestamp pattern found but it doesn't have any timestamp in name '{name}'."
            : $"No Sony camera specific timestamp pattern found in name '{name}'.");
    }

    /// <summary>
    /// Smartphones (IMG_YYYYMMDD_HHMMSS or PXL_YYYYMMDD_HHMMSS)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static Result<GetFileTimestampResponse> GetSmartphonePatternFromName(GetFileTimestampRequest request)
    {
        var name = request.FileNameWithoutExtension;
        var match = MatchSmartPhoneCamFilePattern().Match(name);
        if (!match.Success)
            return Result<GetFileTimestampResponse>.Failure(ErrorCode.NotFound,
                $"No smartphone camera timestamp pattern found in name '{name}'.");

        var combined = $"{match.Groups[2].Value}_{match.Groups[3].Value}";
        if (!DateTime.TryParseExact(
                combined,
                "yyyyMMdd_HHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDateTime))
        {
            return Result<GetFileTimestampResponse>.Failure(ErrorCode.NotFound,
                $"Smartphone camera specific timestamp pattern found but failed to parse: {combined}");
        }

        return Result<GetFileTimestampResponse>.Success(new GetFileTimestampResponse()
        {
            Timestamp = parsedDateTime,
            Source = TimestampSource.FileName,
            StatusMessage = "Smartphone camera specific timestamp extracted from filename."
        });
    }

    #endregion

    #region Public Methods for Extracting Timestamps from Filename

    public static Result<GetFileTimestampResponse> GetGenericTimestampFromName(GetFileTimestampRequest request)
    {
        var name = request.FileNameWithoutExtension;
        var match = GenericTimestampMatchPattern().Match(name);
        if (!match.Success)
        {
            return Result<GetFileTimestampResponse>.Failure(ErrorCode.NotFound,
                $"No timestamp pattern found in name '{name}'.");
        }

        var combined = match.Value;
        foreach (var pattern in TimestampPatterns)
        {
            if (!DateTime.TryParseExact(
                    combined,
                    pattern,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var dt))
                continue;

            var result = new GetFileTimestampResponse()
            {
                Timestamp = dt,
                Source = TimestampSource.FileName,
                StatusMessage = $"Timestamp extracted from filename using pattern '{pattern}'."
            };
            return result;
        }

        return Result<GetFileTimestampResponse>.Failure(ErrorCode.NotFound,
            $"Timestamp pattern found but failed to parse: {combined}");
    }

    public static Result<GetFileTimestampResponse> GetEmbeddedTimestampFromName(GetFileTimestampRequest request)
    {
        var name = request.FileNameWithoutExtension;
        var match = MatchEmbeddedTimestampPattern().Match(name);
        if (!match.Success)
        {
            return Result<GetFileTimestampResponse>.Failure(ErrorCode.NotFound,
                $"No embedded timestamp pattern found in name '{name}'.");
        }

        var combined = match.Value;
        foreach (var pattern in TimestampPatterns)
        {
            if (!DateTime.TryParseExact(
                    combined,
                    pattern,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var dt))
                continue;

            var result = new GetFileTimestampResponse()
            {
                Timestamp = dt,
                Source = TimestampSource.FileName,
                StatusMessage = $"Embedded timestamp extracted from filename using pattern '{pattern}'."
            };
            return result;
        }

        return Result<GetFileTimestampResponse>.Failure(ErrorCode.NotFound,
            $"Embedded timestamp pattern found but failed to parse: {combined}");
    }

    public static Result<GetFileTimestampResponse> GetEpochSecondsFromName(GetFileTimestampRequest request)
    {
        var name = request.FileNameWithoutExtension;
        var match = MatchEpochSecondsPattern().Match(name);
        if (!match.Success)
        {
            return Result<GetFileTimestampResponse>.Failure(ErrorCode.NotFound,
                $"No epoch seconds pattern found in name '{name}'.");
        }

        if (!long.TryParse(match.Value, out var sec))
        {
            return Result<GetFileTimestampResponse>.Failure(ErrorCode.NotFound,
                $"Epoch seconds pattern found but failed to parse as long: {match.Value}");
        }

        try
        {
            var epoch = DateTimeOffset.FromUnixTimeSeconds(sec);
            var result = new GetFileTimestampResponse()
            {
                Timestamp = epoch.LocalDateTime,
                Source = TimestampSource.FileName,
                StatusMessage = "Epoch seconds timestamp extracted from filename."
            };

            return result;
        }
        catch (Exception e)
        {
            return Result<GetFileTimestampResponse>.Failure(ErrorCode.NotFound,
                $"Epoch seconds ({sec}) pattern found but failed to convert to DateTime: {e.Message}");
        }
    }

    public static Result<GetFileTimestampResponse> GetEpochMillisecondsFromName(GetFileTimestampRequest request)
    {
        var name = request.FileNameWithoutExtension;
        var match = MatchEpochMillisecondsPattern().Match(name);
        if (!match.Success)
            return Result<GetFileTimestampResponse>.Failure(ErrorCode.NotFound,
                $"No epoch milliseconds pattern found in name '{name}'.");

        if (!long.TryParse(match.Value, out var ms))
            return Result<GetFileTimestampResponse>.Failure(ErrorCode.NotFound,
                $"Epoch milliseconds pattern found but failed to parse as long: {match.Value}");
        try
        {
            var epoch = DateTimeOffset.FromUnixTimeMilliseconds(ms);
            var result = new GetFileTimestampResponse()
            {
                Timestamp = epoch.LocalDateTime,
                Source = TimestampSource.FileName,
                StatusMessage = "Epoch milliseconds timestamp extracted from filename."
            };
            return result;
        }
        catch (Exception e)
        {
            return Result<GetFileTimestampResponse>.Failure(ErrorCode.NotFound,
                $"Epoch milliseconds ({ms}) pattern found but failed to convert to DateTime: {e.Message}");
        }
    }

    #endregion

    #region Media File Specific Patterns

    /// <summary>
    /// Use MetadataExtractor (NuGet: MetadataExtractor).
    /// Supported formats: JPG, DNG, TIFF, HEIC, ARW, CR2, NEF, ORF, RW2, etc.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static Result<GetFileTimestampResponse> GetTimestampFromPhotoFileMetadata(GetFileTimestampRequest request)
    {
        var file = request.File;
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(file.FullName);

            var subIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (subIfd != null)
            {
                if (subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal,
                        out var dto))
                    return new GetFileTimestampResponse(dto,
                        TimestampSource.PhotoMetadata,
                        $"DateTaken value  found in photo file '{file.Name}'.");

                if (subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized,
                        out var dtd))
                    return new GetFileTimestampResponse(dtd,
                        TimestampSource.PhotoMetadata,
                        $"DateDigitized value found in photo file '{file.Name}'.");
            }

            var ifd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            if (ifd0 != null &&
                ifd0.TryGetDateTime(ExifDirectoryBase.TagDateTime,
                    out var dt))
                return new GetFileTimestampResponse(dt,
                    TimestampSource.PhotoMetadata,
                    $"Timestamp value found in photo file '{file.Name}'.");
        }
        catch (Exception e)
        {
            var error = Error.Create(ErrorCode.Unknown,
                $"Unable to extract timestamp from photo file '{file.Name}'. Error: {e.Message}");
            return error;
        }

        return Error.Create(ErrorCode.NotFound, $"Unable to extract timestamp from photo file '{file.Name}'.");
    }

    private static Result<GetFileTimestampResponse> GetTimestampFromPngMetadata(GetFileTimestampRequest request)
    {
        var file = request.File;
        if (!file.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
            return Error.Create(ErrorCode.NotFound, $"File '{file.Name}' is not a PNG file.");

        try
        {
            var directories = ImageMetadataReader.ReadMetadata(file.FullName);
            var png = directories.OfType<PngDirectory>().FirstOrDefault();

            if (png != null && png.TryGetDateTime(PngDirectory.TagLastModificationTime, out var dt))
            {
                return new GetFileTimestampResponse(dt, TimestampSource.PngMetadata, "PNG metadata timestamp found.");
            }
        }
        catch (Exception e)
        {
            var error = Error.Create(ErrorCode.Unknown,
                $"Unable to extract timestamp from PNG file '{file.Name}'. Error: {e.Message}");
            return error;
        }

        return Error.Create(ErrorCode.NotFound, $"Unable to extract timestamp from PNG file '{file.Name}'.");
    }

    private static Result<GetFileTimestampResponse> GetTimestampFromPsdMetadata(GetFileTimestampRequest request)
    {
        var file = request.File;
        if (!file.Extension.Equals(".psd", StringComparison.OrdinalIgnoreCase))
            return Error.Create(ErrorCode.NotFound, $"File '{file.Name}' is not a PSD file.");

        try
        {
            var directories = ImageMetadataReader.ReadMetadata(file.FullName);
            var xmp = directories.OfType<XmpDirectory>().FirstOrDefault();

            var dto = xmp?.XmpMeta?.GetPropertyDate("http://ns.adobe.com/xap/1.0/", "CreateDate");
            if (dto != null && dto.HasDate)
            {
                return new GetFileTimestampResponse(
                    new DateTime(dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, dto.Second,
                        DateTimeKind.Unspecified), TimestampSource.PsdMetadata, "PSD XMP CreateDate found");
            }
        }
        catch (Exception e)
        {
            var error = Error.Create(ErrorCode.Unknown,
                $"Unable to extract timestamp from PSD file '{file.Name}'. Error: {e.Message}");
            return error;
        }

        return Error.Create(ErrorCode.NotFound, $"Unable to extract timestamp from PSD file '{file.Name}'.");

    }
    
    private static Result<GetFileTimestampResponse> GetTimestampFromVideoFileMetadata(GetFileTimestampRequest request)
    {
        var file = request.File;
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(file.FullName);

            // QuickTime Movie Header (most reliable)
            var movie = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
            if (movie != null)
            {
                if (movie.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagCreated, out var created))
                    return new GetFileTimestampResponse(created, TimestampSource.VideoMetadata,
                        $"Created date value found in video file '{file.Name}'.");

                if (movie.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagModified, out var modified))
                    return new GetFileTimestampResponse(modified, TimestampSource.VideoMetadata,
                        $"Modified date value found in video file '{file.Name}'.");
            }

            // QuickTime Track Header (sometimes more accurate)
            var track = directories.OfType<QuickTimeTrackHeaderDirectory>().FirstOrDefault();
            if (track != null)
            {
                if (track.TryGetDateTime(QuickTimeTrackHeaderDirectory.TagCreated, out var created))
                    return new GetFileTimestampResponse(created, TimestampSource.VideoMetadata,
                        $"Created date value found in video file '{file.Name}'.");

                if (track.TryGetDateTime(QuickTimeTrackHeaderDirectory.TagModified, out var modified))
                    return new GetFileTimestampResponse(modified, TimestampSource.VideoMetadata,
                        $"Modified date value found in video file '{file.Name}'.");
            }

            // QuickTime Metadata Header (rare but valid)
            var meta = directories.OfType<QuickTimeMetadataHeaderDirectory>().FirstOrDefault();
            if (meta != null)
            {
                if (meta.TryGetDateTime(QuickTimeMetadataHeaderDirectory.TagCreationDate, out var created))
                    return new GetFileTimestampResponse(created, TimestampSource.VideoMetadata,
                        $"Created date value found in video file '{file.Name}'.");
            }
        }
        catch (Exception e)
        {
            var error = Error.Create(ErrorCode.Unknown,
                $"Unable to extract timestamp from video file '{file.FullName}'. Error: {e.Message}");
            return error;
        }

        return Error.Create(ErrorCode.NotFound, $"Unable to extract timestamp from video file '{file.FullName}'.");
    }


    private static Result<GetFileTimestampResponse> GetTimestampFromXmpSidecar(GetFileTimestampRequest request)
    {
        var file = request.File;
        var xmpPath = Path.ChangeExtension(file.FullName, ".xmp");
        if (!File.Exists(xmpPath))
            return Error.Create(ErrorCode.NotFound, $"Xmp sidecar file '{xmpPath}' does not exist.");

        try
        {
            var directories = ImageMetadataReader.ReadMetadata(xmpPath);
            var xmp = directories.OfType<XmpDirectory>().FirstOrDefault();
            if (xmp?.XmpMeta == null)
                return Error.Create(ErrorCode.NotFound, $"XMP directory not found in '{xmpPath}'.");

            var dto = xmp.XmpMeta.GetPropertyDate("http://ns.adobe.com/xap/1.0/", "CreateDate");
            if (dto is { HasDate: true })
                return new GetFileTimestampResponse(
                    new DateTime(dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, dto.Second, DateTimeKind.Local),
                    TimestampSource.VideoMetadata, $"CreateDate value found in XMP directory.");

            var dtd = xmp.XmpMeta.GetPropertyDate("http://ns.adobe.com/xap/1.0/", "ModifyDate");
            if (dtd is { HasDate: true })
                return new GetFileTimestampResponse(
                    new DateTime(dtd.Year, dtd.Month, dtd.Day, dtd.Hour, dtd.Minute, dtd.Second, DateTimeKind.Local),
                    TimestampSource.VideoMetadata, $"ModifyDate value found in XMP directory.");
        }
        catch (Exception e)
        {
            var error = Error.Create(ErrorCode.Unknown,
                $"Unable to extract timestamp from video file '{file.FullName}'. Error: {e.Message}");
            return error;
        }

        return Error.Create(ErrorCode.NotFound, $"Unable to extract timestamp from video file '{file.FullName}'.");
    }

    private static Result<GetFileTimestampResponse> GetTimestampFromThmSidecar(GetFileTimestampRequest request)
    {
        var file = request.File;
        var thmPath = Path.ChangeExtension(file.FullName, ".thm");
        if (!File.Exists(thmPath))
            return Error.Create(ErrorCode.NotFound, $"Thm sidecar file '{thmPath}' does not exist.");

        try
        {
            var directories = ImageMetadataReader.ReadMetadata(thmPath);

            var subIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (subIfd != null &&
                subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dto))
                return new GetFileTimestampResponse(
                    dto,
                    TimestampSource.VideoMetadata, $"DateTimeOriginal value found in XMP directory.");
        }
        catch (Exception e)
        {
            var error = Error.Create(ErrorCode.Unknown,
                $"Unable to extract timestamp from video file '{file.FullName}'. Error: {e.Message}");
            return error;
        }

        return Error.Create(ErrorCode.NotFound, $"Unable to extract timestamp from video file '{file.FullName}'.");
    }

    private static Result<GetFileTimestampResponse> GetTimestampFromAvchdCpiSidecar(GetFileTimestampRequest request)
    {
        var file = request.File;
        var cpiPath = Path.ChangeExtension(file.FullName, ".CPI");
        if (!File.Exists(cpiPath))
            return Error.Create(ErrorCode.NotFound, $"AVCHD CPI sidecar file '{cpiPath}' does not exist.");

        try
        {
            var bytes = File.ReadAllBytes(cpiPath);

            // Timestamp offset in CPI (bytes 0x60–0x63)
            const int offset = 0x60;

            if (bytes.Length < offset + 4)
                return Error.Create(ErrorCode.NotFound, $"No timestamp found in MTS video file '{file.FullName}'.");

            var seconds = BitConverter.ToUInt32(bytes, offset);

            // AVCHD epoch: 2006-01-01
            var epoch = new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return new GetFileTimestampResponse(epoch.AddSeconds(seconds).ToLocalTime(), TimestampSource.VideoMetadata,
                $"Timestamp value found in AVCHD CPI file '{file.Name}'.");
        }
        catch (Exception e)
        {
            var error = Error.Create(ErrorCode.Unknown,
                $"Unable to extract timestamp from video file '{file.Name}'. Error: {e.Message}");
            return error;
        }

        return Error.Create(ErrorCode.NotFound, $"Unable to extract timestamp from video file '{file.Name}'.");
    }

    private static Result<GetFileTimestampResponse> GetTimestampFromAvchdMplSidecar(GetFileTimestampRequest request)
    {
        var file = request.File;
        var mplPath = Path.ChangeExtension(file.FullName, ".MPL");
        if (!File.Exists(mplPath))
            return Error.Create(ErrorCode.NotFound, $"AVCHD MPL sidecar file '{mplPath}' does not exist.");

        try
        {
            var bytes = File.ReadAllBytes(mplPath);

            // Timestamp offset in MPL (bytes 0x50–0x53)
            const int offset = 0x50;

            if (bytes.Length < offset + 4)
                return Error.Create(ErrorCode.NotFound,
                    $"No AVCHD timestamp found in MTS video file '{file.FullName}'.");

            var seconds = BitConverter.ToUInt32(bytes, offset);

            var epoch = new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return new GetFileTimestampResponse(epoch.AddSeconds(seconds).ToLocalTime(), TimestampSource.VideoMetadata,
                $"Timestamp value found in AVCHD MPL file '{file.Name}'.");
        }
        catch (Exception e)
        {
            var error = Error.Create(ErrorCode.Unknown,
                $"Unable to extract timestamp from video file '{file.Name}'. Error: {e.Message}");
            return error;
        }

        return Error.Create(ErrorCode.NotFound, $"Unable to extract timestamp from video file '{file.Name}'.");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private static Result<GetFileTimestampResponse> GetTimestampFromAvchd(GetFileTimestampRequest request)
    {
        var file = request.File;

        if (!IsMts(file))
            return Error.Create(ErrorCode.NotFound,
                $"Specified file '{file.FullName}' is not a AVCHD (MTS) video file.");

        return ResultCombiner.FirstSuccess(
            () => GetTimestampFromAvchdCpiSidecar(request),
            () => GetTimestampFromAvchdMplSidecar(request)
        );
    }

    private static Result<GetFileTimestampResponse> GetTimestampFromSidecars(GetFileTimestampRequest request)
    {
        return ResultCombiner.FirstSuccess(
            () => GetTimestampFromXmpSidecar(request),
            () => GetTimestampFromAvchdMplSidecar(request)
        );
    }

    private static bool IsPhoto(FileInfo file)
    {
        var ext = file.Extension.ToLowerInvariant();
        return ext is ".jpg" or ".jpeg" or ".dng" or ".tiff" or ".tif" or ".heic" or ".arw" or ".cr2" or ".nef"
            or ".rw2";
    }

    private static bool IsVideo(FileInfo file)
    {
        var ext = file.Extension.ToLowerInvariant();
        return ext is ".mp4" or ".mov" or ".avi" or ".mkv" or ".3gp" or ".wmv" or ".m4v";
    }

    private static bool IsMts(FileInfo file)
    {
        var ext = file.Extension.ToLowerInvariant();
        return ext is ".mts" or ".m2ts";
    }

    #endregion

    #region Generated Regex Patterns

    [GeneratedRegex(@"DSC_\d{4}")]
    private static partial Regex MatchNikonCamFilePattern();

    [GeneratedRegex(@"DSC0\d{4}")]
    private static partial Regex MatchSonyCamFilePattern();

    [GeneratedRegex(@"IMG_(\d{8})_(\d{6})")]
    private static partial Regex MatchCanonCamFilePattern();

    [GeneratedRegex(@"(IMG|PXL)_(\d{8})_(\d{6})")]
    private static partial Regex MatchSmartPhoneCamFilePattern();

    [GeneratedRegex(@"(?<ts>(
            (?<f1>\d{4})(?<f2>\d{2})(?<f3>\d{2})_(?<f4>\d{2})(?<f5>\d{2})(?<f6>\d{2}) |
            (?<y>\d{4})-(?<m>\d{2})-(?<d>\d{2})\s(?<H>\d{2})-(?<M>\d{2})-(?<S>\d{2})
        ))", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace)]
    private static partial Regex GenericTimestampMatchPattern();

    [GeneratedRegex(@"\d{10}")]
    private static partial Regex MatchEpochSecondsPattern();

    [GeneratedRegex(@"\d{13}")]
    private static partial Regex MatchEpochMillisecondsPattern();

    [GeneratedRegex(@"\d{8}_\d{6}")]
    private static partial Regex MatchEmbeddedTimestampPattern();

    #endregion
}