using Inamsoft.MediaFileRenamer.Services.FileRenamers;

namespace Inamsoft.MediaFileRenamer.Services.Tests;

public class ThreeGpTests
{
    [Fact]
    public void ExtractTimestamp_3gp_ReturnsCorrectTimestamp()
    {
        var file = TestFileHelper.GetVideo("sample_3gp.3gp");

        var found = TimestampExtractor.TryExtractTimestamp(file, out var timestamp);

        Assert.True(found);
        Assert.NotNull(timestamp);
        Assert.Equal(new DateTime(2022, 2, 14, 16, 20, 5), timestamp);
    }
}
