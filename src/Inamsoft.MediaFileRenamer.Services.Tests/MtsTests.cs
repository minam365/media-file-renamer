using Inamsoft.MediaFileRenamer.Services.FileRenamers;

namespace Inamsoft.MediaFileRenamer.Services.Tests;

public class MtsTests
{
    [Fact]
    public void ExtractTimestamp_Mts_UsesCpiSidecar()
    {
        var file = TestFileHelper.GetVideo("sample.mts");

        var found = TimestampExtractor.TryExtractTimestamp(file, out var timestamp);

        Assert.True(found);
        Assert.NotNull(timestamp);
        Assert.Equal(new DateTime(2020, 6, 15, 14, 22, 10), timestamp);
    }
}
