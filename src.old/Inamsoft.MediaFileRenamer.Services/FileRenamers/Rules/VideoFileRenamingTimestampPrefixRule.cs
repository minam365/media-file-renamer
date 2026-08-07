using Inamsoft.MediaFileRenamer.Abstractions;

namespace Inamsoft.MediaFileRenamer.Services.FileRenamers.Rules;

public sealed class VideoFileRenamingTimestampPrefixRule : IRenamingRule
{
    private readonly VideoFileRenamingTimestampStrategy _strategy;

    public VideoFileRenamingTimestampPrefixRule(VideoFileRenamingTimestampStrategy strategy)
    {
        _strategy = strategy;
    }

    public string Apply(IRenamingContext context, string currentName)
    {
        if (_strategy == VideoFileRenamingTimestampStrategy.None)
            return currentName;

        var fromName = context.ParsedTimestampFromName;
        var fromModified = context.File.LastWriteTime;

        DateTime? chosen = _strategy switch
        {
            VideoFileRenamingTimestampStrategy.FileNameOrLastModifiedDate =>
                fromName ?? fromModified,

            VideoFileRenamingTimestampStrategy.LastModifiedDateOrFileName =>
                fromModified != default ? fromModified : fromName,

            _ => null
        };

        if (chosen is null)
            return currentName;

        var prefix = chosen.Value.ToString("yyyyMMdd_HHmmss");
        return $"{prefix} {currentName}";
    }
}