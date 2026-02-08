using Inamsoft.MediaFileRenamer.Services.FileRenamers;

namespace Inamsoft.MediaFileRenamer.Services.Tests;

public class TimestampExtractorFilenamePatternTests
{
    [Fact]
    public void ExtractTimestamp_FilenamePrefixTimestamp_ReturnsCorrectTimestamp()
    {
        var file = TestFileHelper.GetVideo("20170304_191830.mp4");

        var found = TimestampExtractor.TryExtractTimestamp(file, out var timestamp);

        Assert.True(found);
        Assert.NotNull(timestamp);
        Assert.Equal(new DateTime(2017, 3, 4, 19, 18, 30), timestamp);
    }
}
