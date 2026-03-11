using Inamsoft.MediaFileRenamer.Services.FileRenamers;

namespace Inamsoft.MediaFileRenamer.Services.Tests;

public class MtsTests
{
    [Fact]
    public void ExtractTimestamp_Mts_UsesCpiSidecar()
    {
        var file = TestFileHelper.GetVideo("00068.MTS");

        var found = TimestampExtractor.TryExtractTimestamp(file, out var timestamp);

        Assert.True(found);
        Assert.NotNull(timestamp);
        Assert.Equal(new DateTime(2013, 5, 26, 15, 40, 14), timestamp);
    }
}
