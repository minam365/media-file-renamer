using Inamsoft.MediaFileRenamer.Services.FileRenamers;

namespace Inamsoft.MediaFileRenamer.Services.Tests;

public class AviTests
{
    [Fact]
    public void ExtractTimestamp_Avi_FallsBackToLastModified()
    {
        var file = TestFileHelper.GetVideo("sample_avi.avi");

        var ts = TimestampHelper.ExtractTimestamp(file);

        Assert.NotNull(ts);
        Assert.Equal(file.LastWriteTime, ts.Value);
    }
}
