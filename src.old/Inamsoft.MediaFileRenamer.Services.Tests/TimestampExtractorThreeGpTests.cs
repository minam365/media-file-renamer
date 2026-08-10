using Inamsoft.MediaFileRenamer.Services.FileRenamers;

namespace Inamsoft.MediaFileRenamer.Services.Tests;

public class TimestampExtractorThreeGpTests
{
    [Fact]
    public void ExtractTimestamp_3gp_ReturnsCorrectTimestamp()
    {
        var file = TestFileHelper.GetVideo("MOV00002.3gp");

        var found = TimestampExtractor.TryExtractTimestamp(file, out var timestamp);

        Assert.True(found);
        Assert.NotNull(timestamp);
        Assert.Equal(new DateTime(2005, 4, 12, 22, 27, 14), timestamp);
    }
}
