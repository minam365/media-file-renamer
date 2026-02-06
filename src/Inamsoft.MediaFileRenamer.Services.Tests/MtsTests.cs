using Inamsoft.MediaFileRenamer.Services.FileRenamers;

namespace Inamsoft.MediaFileRenamer.Services.Tests;

public class MtsTests
{
    [Fact]
    public void ExtractTimestamp_Mts_UsesCpiSidecar()
    {
        var file = TestFileHelper.GetVideo("sample.mts");

        var ts = TimestampHelper.ExtractTimestamp(file);

        Assert.NotNull(ts);
        Assert.Equal(new DateTime(2020, 6, 15, 14, 22, 10), ts.Value);
    }
}
