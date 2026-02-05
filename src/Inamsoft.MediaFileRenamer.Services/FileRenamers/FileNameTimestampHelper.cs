using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Mpeg;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Formats.Xmp;
using System;
using System.Globalization;
using System.Text.RegularExpressions;


namespace Inamsoft.MediaFileRenamer.Services.FileRenamers;


public static partial class FileNameTimestampHelper
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

    public static DateTime? ExtractTimestamp(FileInfo file)
    {
        string name = Path.GetFileNameWithoutExtension(file.Name);

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

            // 3.Video metadata
            (IsVideo(file) ? TryVideoMetadata(file) : null) ??

            // 4. EXIF fallback (photos only)
            (IsPhoto(file) ? TryExifTimestamp(file) : null) ??

            // 5. Sidecar files
            TrySidecars(file);

    }

    private static DateTime? TryVideoMetadata(FileInfo file)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(file.FullName);

            // QuickTime (MOV, MP4)
            var qt = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
            if (qt != null)
            {
                if (qt.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagCreated, out var created))
                    return created;

                if (qt.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagModified, out var modified))
                    return modified;
            }

            // MP4
            var mp4 = directories.OfType<>().FirstOrDefault();
            if (mp4 != null)
            {
                if (mp4.TryGetDateTime(Mp3Directory., out var created))
                    return created;

                if (mp4.TryGetDateTime(Mp4Directory.TagModificationTime, out var modified))
                    return modified;
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