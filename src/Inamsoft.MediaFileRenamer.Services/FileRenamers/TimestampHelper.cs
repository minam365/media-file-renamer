using MetadataExtractor;
using MetadataExtractor.Formats.Avi;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Png;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Formats.Xmp;
using System.Globalization;
using System.Text.RegularExpressions;


namespace Inamsoft.MediaFileRenamer.Services.FileRenamers;


public static partial class TimestampHelper
{
    // Regex matches both timestamp formats:
    // 1) yyyyddmm_hhmmss
    // 2) yyyy-dd-mm HH-mm-ss
    private static readonly Regex TimestampRegex = GenericTimestampMatchPattern();



    /// <summary>
    /// Replaces timestamps in the filename with a newly generated timestamp.
    /// The new timestamp will match the format of the original timestamp.
    /// </summary>
    public static string ReplaceTimestampAuto(string filename, DateTime? newTime = null)
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be null or empty.", nameof(filename));

        return TimestampRegex.Replace(filename, match =>
        {
            var newTimestamp = GenerateNewTimestamp(match, newTime ?? DateTime.Now);
            return newTimestamp;
        });
    }

    /// <summary>
    /// Generates a new timestamp string based on the format of the matched timestamp.
    /// </summary>
    private static string GenerateNewTimestamp(Match match, DateTime dt)
    {
        // Format 1: yyyyddmm_hhmmss
        if (match.Groups["f1"].Success)
        {
            return dt.ToString("yyyyddMM_HHmmss");
        }

        // Format 2: yyyy-dd-mm HH-mm-ss
        if (match.Groups["y"].Success)
        {
            return dt.ToString("yyyy-MM-dd HH-mm-ss");
        }

        throw new InvalidOperationException("Unknown timestamp format matched.");
    }

    /// <summary>
    /// Replaces timestamps with a custom timestamp string you provide.
    /// </summary>
    public static string ReplaceTimestampWith(string filename, string newTimestamp)
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be null or empty.", nameof(filename));

        if (string.IsNullOrWhiteSpace(newTimestamp))
            throw new ArgumentException("New timestamp cannot be null or empty.", nameof(newTimestamp));

        return TimestampRegex.Replace(filename, newTimestamp);
    }

    private static readonly string[] TimestampPatterns =
    [
        "yyyyMMdd_HHmmss",
        "yyyyMMddHHmmss",
        "yyyy-MM-dd_HHmmss",
        "yyyy-MM-dd HHmmss"
    ];

    public static DateTime? TryParseTimestampFromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        // 1. Direct prefix match: 20170304_191830
        foreach (var pattern in TimestampPatterns)
        {
            int len = pattern.Length;
            if (name.Length >= len &&
                DateTime.TryParseExact(
                    name.Substring(0, len),
                    pattern,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var dt))
            {
                return dt;
            }
        }

        // 2. Embedded timestamp: IMG_20181230_111438
        foreach (var pattern in TimestampPatterns)
        {
            var match = MatchEmbeddedTimestampPattern().Match(name);
            if (match.Success &&
                DateTime.TryParseExact(
                    match.Value,
                    pattern,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var dt))
            {
                return dt;
            }
        }

        // 3. Epoch milliseconds: SmartCamera_1449172790453
        var epochMsMatch = MatchEpochMillisecondsPattern().Match(name);
        if (epochMsMatch.Success &&
            long.TryParse(epochMsMatch.Value, out long ms))
        {
            try
            {
                var epoch = DateTimeOffset.FromUnixTimeMilliseconds(ms);
                return epoch.LocalDateTime;
            }
            catch { /* ignore */ }
        }

        // 4. Epoch seconds: e.g. 1449172790
        var epochSecMatch = MatchEpochSecondsPattern().Match(name);
        if (epochSecMatch.Success &&
            long.TryParse(epochSecMatch.Value, out long sec))
        {
            try
            {
                var epoch = DateTimeOffset.FromUnixTimeSeconds(sec);
                return epoch.LocalDateTime;
            }
            catch { /* ignore */ }
        }

        // 5. No timestamp found
        return null;
    }

    /// <summary>
    /// Canon (IMG_YYYYMMDD_HHMMSS)
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static DateTime? TryCanonPattern(string name)
    {
        var match = MatchCanonCamFilePattern().Match(name);
        if (!match.Success) return null;

        var combined = $"{match.Groups[1].Value}_{match.Groups[2].Value}";
        return DateTime.TryParseExact(
            combined,
            "yyyyMMdd_HHmmss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dt)
            ? dt
            : null;
    }

    /// <summary>
    /// Nikon (DSC_#### → no timestamp)
    /// </summary>
    /// <remarks>Nikon DSLRs often use counters only. We return null.</remarks>
    /// <param name="name"></param>
    /// <returns></returns>
    private static DateTime? TryNikonPattern(string name)
    {
        if (MatchNikonCamFilePattern().IsMatch(name))
            return null;

        return null;
    }

    /// <summary>
    /// Sony (DSC0#### or DSC0#### → no timestamp)
    /// </summary>
    /// <remarks>Sony DSLRs often use counters prefixed with DSC0 only. We return null.</remarks>
    /// <param name="name"></param>
    /// <returns></returns>
    private static DateTime? TrySonyPattern(string name)
    {
        if (MatchSonyCamFilePattern().IsMatch(name))
            return null;

        return null;
    }

    /// <summary>
    /// Smartphones (IMG_YYYYMMDD_HHMMSS or PXL_YYYYMMDD_HHMMSS)
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static DateTime? TrySmartphonePattern(string name)
    {
        var match = MatchSmartPhoneCamFilePattern().Match(name);
        if (!match.Success) return null;

        var combined = $"{match.Groups[2].Value}_{match.Groups[3].Value}";
        return DateTime.TryParseExact(
            combined,
            "yyyyMMdd_HHmmss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dt)
            ? dt
            : null;
    }

    /// <summary>
    /// Use MetadataExtractor (NuGet: MetadataExtractor).
    /// Supported formats: JPG, DNG, TIFF, HEIC, ARW, CR2, NEF, ORF, RW2, etc.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private static DateTime? TryExifTimestamp(FileInfo file)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(file.FullName);

            var subIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (subIfd != null)
            {
                if (subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dto))
                    return dto;

                if (subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out var dtd))
                    return dtd;
            }

            var ifd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            if (ifd0 != null &&
                ifd0.TryGetDateTime(ExifDirectoryBase.TagDateTime, out var dt))
                return dt;
        }
        catch
        {
            // ignore EXIF errors
        }

        return null;
    }

    private static DateTime? TryPngMetadata(FileInfo file, TimestampResult result)
    {
        if (!file.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
            return null;

        try
        {
            var directories = ImageMetadataReader.ReadMetadata(file.FullName);
            var png = directories.OfType<PngDirectory>().FirstOrDefault();

            if (png != null &&
                png.TryGetDateTime(PngDirectory.TagLastModificationTime, out var dt))
            {
                DiagnosticLoggingHelper.Log(result, "PNG metadata timestamp found");
                return dt;
            }
        }
        catch
        {
            DiagnosticLoggingHelper.Log(result, "PNG metadata read failed");
        }

        return null;
    }

    private static DateTime? TryWmvMetadata(FileInfo file, TimestampResult result)
    {
        var ext = file.Extension.ToLowerInvariant();
        if (ext is not ".wmv" and not ".asf")
            return null;

        try
        {
            var directories = ImageMetadataReader.ReadMetadata(file.FullName);
            var asf = directories.OfType<AsfDirectory>().FirstOrDefault();

            if (asf != null &&
                asf.TryGetDateTime(AsfDirectory.TagCreationDate, out var dt))
            {
                DiagnosticLoggingHelper.Log(result, "WMV/ASF metadata timestamp found");
                return dt;
            }
        }
        catch
        {
            DiagnosticLoggingHelper.Log(result, "WMV/ASF metadata read failed");
        }

        return null;
    }

    private static DateTime? TryPsdMetadata(FileInfo file, TimestampResult result)
    {
        if (!file.Extension.Equals(".psd", StringComparison.OrdinalIgnoreCase))
            return null;

        try
        {
            var directories = ImageMetadataReader.ReadMetadata(file.FullName);
            var xmp = directories.OfType<XmpDirectory>().FirstOrDefault();

            if (xmp?.XmpMeta != null)
            {
                var dto = xmp.XmpMeta.GetPropertyDate("http://ns.adobe.com/xap/1.0/", "CreateDate");
                if (dto != null)
                {
                    DiagnosticLoggingHelper.Log(result, "PSD XMP CreateDate found");
                    return dto.HasDate ? new DateTime(dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, dto.Second, DateTimeKind.Utc) : null;
                }
            }
        }
        catch
        {
            DiagnosticLoggingHelper.Log(result, "PSD metadata read failed");
        }

        return null;
    }

    public static DateTime? TryGenericTimestamp(string name)
    {
        var match = GenericTimestampMatchPattern().Match(name);
        if (!match.Success) return null;
        var combined = match.Value;
        foreach (var pattern in TimestampPatterns)
        {
            if (DateTime.TryParseExact(
                combined,
                pattern,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dt))
            {
                return dt;
            }
        }
        return null;
    }

    public static DateTime? TryEmbeddedTimestamp(string name)
    {
        var match = MatchEmbeddedTimestampPattern().Match(name);
        if (!match.Success) return null;
        var combined = match.Value;
        foreach (var pattern in TimestampPatterns)
        {
            if (DateTime.TryParseExact(
                combined,
                pattern,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dt))
            {
                return dt;
            }
        }
        return null;
    }

    public static DateTime? TryEpochSeconds(string name)
    {
        var match = MatchEpochSecondsPattern().Match(name);
        if (!match.Success) return null;
        if (long.TryParse(match.Value, out long sec))
        {
            try
            {
                var epoch = DateTimeOffset.FromUnixTimeSeconds(sec);
                return epoch.LocalDateTime;
            }
            catch { /* ignore */ }
        }
        return null;
    }

    public static DateTime? TryEpochMilliseconds(string name)
    {
        var match = MatchEpochMillisecondsPattern().Match(name);
        if (!match.Success) return null;
        if (long.TryParse(match.Value, out long ms))
        {
            try
            {
                var epoch = DateTimeOffset.FromUnixTimeMilliseconds(ms);
                return epoch.LocalDateTime;
            }
            catch { /* ignore */ }
        }
        return null;
    }

    public static TimestampResult ExtractTimestamp(FileInfo file)
    {
        var result = new TimestampResult
        {
            OriginalName = Path.GetFileNameWithoutExtension(file.Name)
        };

        string name = result.OriginalName;

        DateTime? ts =
            // 1. Camera-specific heuristics
            TryCanonPattern(name, result) ??
            TrySmartphonePattern(name, result) ??
            TryNikonPattern(name, result) ??

            // 2. Generic patterns
            TryGenericTimestamp(name, result) ??
            TryEmbeddedTimestamp(name, result) ??
            TryEpochMilliseconds(name, result) ??
            TryEpochSeconds(name, result) ??

            // 3. AVCHD (.MTS / .M2TS)
            TryAvchdTimestamp(file) ??
            TryAvchdTimestamp(file, result) ??

            // 4.Video metadata (MP4/MOV)
            TryVideoMetadata(file, result) ??
            TryPngMetadata(file, result) ??
            TryWmvMetadata(file, result) ??

            // 5. EXIF fallback (photos only)
             (IsPhoto(file) ? TryExifTimestamp(file, result) : null) ??

        // 6. PSD metadata
        TryPsdMetadata(file, result);

        if (ts != null)
        {
            result.ResultingTimestamp = ts;
            return result;
        }

        // Fallback
        result.ResultingTimestamp = file.LastWriteTime;
        result.Source = TimestampSource.FileSystemModifiedDate;
        Log(result, "Falling back to file system modified date");

        return result;


        // 1. Camera-specific heuristics
        return
            TryCanonPattern(name) ??
            //TryDjiPattern(name) ??
            TrySmartphonePattern(name) ??
            TryNikonPattern(name) ??     // returns null
                                         //TryGoProPattern(name) ??     // returns null

            // 2. Generic patterns
            TryGenericTimestamp(name) ??
            TryEmbeddedTimestamp(name) ??
            TryEpochMilliseconds(name) ??
            TryEpochSeconds(name) ??

            // 3. AVCHD (.MTS / .M2TS)
            TryAvchdTimestamp(file) ??

            // 4.Video metadata (MP4/MOV)
            (IsVideo(file) ? TryVideoMetadata(file) : null) ??

            // 5. EXIF fallback (photos only)
            (IsPhoto(file) ? TryExifTimestamp(file) : null) ??

            // 6. Sidecar files
            TrySidecars(file);

    }

    private static DateTime? TryVideoMetadata(FileInfo file)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(file.FullName);

            // QuickTime Movie Header (most reliable)
            var movie = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
            if (movie != null)
            {
                if (movie.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagCreated, out var created))
                    return created;

                if (movie.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagModified, out var modified))
                    return modified;
            }

            // QuickTime Track Header (sometimes more accurate)
            var track = directories.OfType<QuickTimeTrackHeaderDirectory>().FirstOrDefault();
            if (track != null)
            {
                if (track.TryGetDateTime(QuickTimeTrackHeaderDirectory.TagCreated, out var created))
                    return created;

                if (track.TryGetDateTime(QuickTimeTrackHeaderDirectory.TagModified, out var modified))
                    return modified;
            }

            // QuickTime Metadata Header (rare but valid)
            var meta = directories.OfType<QuickTimeMetadataHeaderDirectory>().FirstOrDefault();
            if (meta != null)
            {
                if (meta.TryGetDateTime(QuickTimeMetadataHeaderDirectory.TagCreationDate, out var created))
                    return created;
            }
        }
        catch
        {
            // ignore metadata errors
        }

        return null;
    }

    private static DateTime? TryXmpSidecar(FileInfo file)
    {
        var xmpPath = Path.ChangeExtension(file.FullName, ".xmp");
        if (!File.Exists(xmpPath))
            return null;

        try
        {
            var directories = ImageMetadataReader.ReadMetadata(xmpPath);
            var xmp = directories.OfType<XmpDirectory>().FirstOrDefault();
            if (xmp == null) return null;
            if (xmp.XmpMeta == null) return null;

            var dto = xmp.XmpMeta.GetPropertyDate("http://ns.adobe.com/xap/1.0/", "CreateDate");
            if (dto != null) return dto.HasDate ? new DateTime(dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, dto.Second, DateTimeKind.Local) : null;

            var dtd = xmp.XmpMeta.GetPropertyDate("http://ns.adobe.com/xap/1.0/", "ModifyDate");
            if (dtd != null) return dtd.HasDate ? new DateTime(dtd.Year, dtd.Month, dtd.Day, dtd.Hour, dtd.Minute, dtd.Second, DateTimeKind.Local) : null;
        }
        catch { }

        return null;
    }

    private static DateTime? TryThmSidecar(FileInfo file)
    {
        var thmPath = Path.ChangeExtension(file.FullName, ".thm");
        if (!File.Exists(thmPath))
            return null;

        try
        {
            var directories = ImageMetadataReader.ReadMetadata(thmPath);

            var subIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (subIfd != null &&
                subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dto))
                return dto;
        }
        catch { }

        return null;
    }

    private static DateTime? TryAvchdCpiSidecar(FileInfo mtsFile)
    {
        var cpiPath = Path.ChangeExtension(mtsFile.FullName, ".CPI");
        if (!File.Exists(cpiPath))
            return null;

        try
        {
            var bytes = File.ReadAllBytes(cpiPath);

            // Timestamp offset in CPI (bytes 0x60–0x63)
            const int offset = 0x60;

            if (bytes.Length < offset + 4)
                return null;

            uint seconds = BitConverter.ToUInt32(bytes, offset);

            // AVCHD epoch: 2006-01-01
            var epoch = new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return epoch.AddSeconds(seconds).ToLocalTime();
        }
        catch
        {
            return null;
        }
    }

    private static DateTime? TryAvchdMplSidecar(FileInfo mtsFile)
    {
        var mplPath = Path.ChangeExtension(mtsFile.FullName, ".MPL");
        if (!File.Exists(mplPath))
            return null;

        try
        {
            var bytes = File.ReadAllBytes(mplPath);

            // Timestamp offset in MPL (bytes 0x50–0x53)
            const int offset = 0x50;

            if (bytes.Length < offset + 4)
                return null;

            uint seconds = BitConverter.ToUInt32(bytes, offset);

            var epoch = new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return epoch.AddSeconds(seconds).ToLocalTime();
        }
        catch
        {
            return null;
        }
    }

    private static DateTime? TryAvchdTimestamp(FileInfo file)
    {
        if (!IsMts(file))
            return null;

        return
            TryAvchdCpiSidecar(file) ??
            TryAvchdMplSidecar(file);
    }

    private static DateTime? TrySidecars(FileInfo file)
    {
        return
            TryXmpSidecar(file) ??
            TryThmSidecar(file);
    }


    private static bool IsPhoto(FileInfo file)
    {
        var ext = file.Extension.ToLowerInvariant();
        return ext is ".jpg" or ".jpeg" or ".dng" or ".tiff" or ".tif" or ".heic" or ".arw" or ".cr2" or ".nef" or ".rw2";
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
}