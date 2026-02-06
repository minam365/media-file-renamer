using Inamsoft.MediaFileRenamer.Services.FileRenamers;

namespace Inamsoft.MediaFileRenamer.Services.Tests;

public class NoTimestampTests
{
    [Fact]
    public void ExtractTimestamp_NoMetadata_NoFilename_FallsBackToLastModified()
    {
        var file = TestFileHelper.GetVideo("DSC_2100.MP4");

        var ts = TimestampHelper.ExtractTimestamp(file);

        Assert.NotNull(ts);
        Assert.Equal(file.LastWriteTime, ts.Value);
    }
}