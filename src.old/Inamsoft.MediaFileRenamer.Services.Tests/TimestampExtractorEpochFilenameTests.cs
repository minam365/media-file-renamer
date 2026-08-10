using Inamsoft.MediaFileRenamer.Services.FileRenamers;

namespace Inamsoft.MediaFileRenamer.Services.Tests;

public class TimestampExtractorEpochFilenameTests
{
    [Fact]
    public void ExtractTimestamp_EpochSeconds_ReturnsCorrectTimestamp()
    {
        var file = TestFileHelper.GetVideo("SmartCam_1449172790.mp4");

        var found = TimestampExtractor.TryExtractTimestamp(file, out var timestamp);

        Assert.True(found);
        Assert.NotNull(timestamp);
        Assert.Equal(
            DateTimeOffset.FromUnixTimeSeconds(1449172790).LocalDateTime,
            timestamp);
    }

    [Fact]
    public void ExtractTimestamp_EpochMilliseconds_ReturnsCorrectTimestamp()
    {
        var file = TestFileHelper.GetVideo("SmartCam_1449172790453.mp4");

        var found = TimestampExtractor.TryExtractTimestamp(file, out var timestamp);

        Assert.True(found);
        Assert.NotNull(timestamp);
        Assert.Equal(
            DateTimeOffset.FromUnixTimeMilliseconds(1449172790453).LocalDateTime,
            timestamp);
    }
}
