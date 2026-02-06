using Inamsoft.MediaFileRenamer.Services.FileRenamers;

namespace Inamsoft.MediaFileRenamer.Services.Tests;

public class FilenamePatternTests
{
    [Fact]
    public void ExtractTimestamp_FilenamePrefixTimestamp_ReturnsCorrectTimestamp()
    {
        var file = TestFileHelper.GetVideo("20170304_191830.mp4");

        var ts = TimestampHelper.ExtractTimestamp(file);

        Assert.NotNull(ts);
        Assert.Equal(new DateTime(2017, 3, 4, 19, 18, 30), ts.Value);
    }
}
