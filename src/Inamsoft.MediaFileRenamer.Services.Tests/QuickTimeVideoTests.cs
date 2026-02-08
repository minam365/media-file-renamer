using Inamsoft.MediaFileRenamer.Services.FileRenamers;

namespace Inamsoft.MediaFileRenamer.Services.Tests;

public class QuickTimeVideoTests
{
    [Theory]
    [InlineData("20251227_073849000_iOS.MOV", 2025, 12, 27, 7, 38, 49)]
    [InlineData("105085eb-cdb5-4c0c-a58a-7757ac068e23.mp4", 2018, 1, 18, 15, 9, 32)]
    [InlineData("M2U00004.MPG", 2019, 11, 1, 18, 5, 0)]
    [InlineData("VID_20181114_010700.mp4", 2019, 11, 1, 18, 5, 0)]
    [InlineData("VID-20180622-WA0021.mp4", 2019, 11, 1, 18, 5, 0)]
    [InlineData("20170625_073106.mp4", 2017, 6, 25, 7, 31, 6)]
    public void ExtractTimestamp_QuickTimeFormats_ReturnsCorrectTimestamp(
        string fileName,
        int year, int month, int day,
        int hour, int minute, int second)
    {
        var file = TestFileHelper.GetVideo(fileName);

        var found = TimestampExtractor.TryExtractTimestamp(file, out var timestamp);

        Assert.True(found);
        Assert.NotNull(timestamp);
        Assert.Equal(new DateTime(year, month, day, hour, minute, second), timestamp);
    }
}
