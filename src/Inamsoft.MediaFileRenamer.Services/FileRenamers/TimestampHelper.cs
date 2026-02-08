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