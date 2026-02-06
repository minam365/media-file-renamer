using Inamsoft.MediaFileRenamer.Services.FileRenamers;

namespace Inamsoft.MediaFileRenamer.Services.Tests;

public class MkvTests
{
    [Fact]
    public void ExtractTimestamp_Mkv_FallsBackToLastModified()
    {
        var file = TestFileHelper.GetVideo("sample_mkv.mkv");

        var ts = TimestampHelper.ExtractTimestamp(file);

        Assert.NotNull(ts);
        Assert.Equal(file.LastWriteTime, ts.Value);
    }
}
