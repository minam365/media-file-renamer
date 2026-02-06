using Inamsoft.MediaFileRenamer.Services.FileRenamers;

namespace Inamsoft.MediaFileRenamer.Services.Tests;

public class ThreeGpTests
{
    [Fact]
    public void ExtractTimestamp_3gp_ReturnsCorrectTimestamp()
    {
        var file = TestFileHelper.GetVideo("sample_3gp.3gp");

        var ts = TimestampHelper.ExtractTimestamp(file);

        Assert.NotNull(ts);
        Assert.Equal(new DateTime(2022, 2, 14, 16, 20, 5), ts.Value);
    }
}
