using Inamsoft.MediaFileRenamer.Abstractions;
using Inamsoft.MediaFileRenamer.Services.FileRenamers.Rules;
using System.Globalization;

namespace Inamsoft.MediaFileRenamer.Services.FileRenamers;

public static class VideoFileRenamer
{
    public static string Rename(FileInfo file, VideoFileRenamingTimestampStrategy timestampStrategy, FileNameCollisionStrategy nameCollisionStrategy)
    {
        var pipeline = new RenamingPipeline(new IRenamingRule[]
        {
            new TimestampPrefixRule(timestampStrategy),
            // Add more rules here (sanitization, uniqueness, etc.)
        });

        var context = new RenamingContext(file, FileNameTimestampHelper.TryParseTimestampFromName);

        return pipeline.Execute(context);
    }

    private static DateTime? TryParseTimestampFromName(string name)
    {
        // Example: 20240101_153000 MyVideo.mp4
        if (DateTime.TryParseExact(
            name[..15],
            "yyyyMMdd_HHmmss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dt))
        {
            return dt;
        }

        return null;
    }
}