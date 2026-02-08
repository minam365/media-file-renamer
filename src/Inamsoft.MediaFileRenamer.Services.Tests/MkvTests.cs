using Inamsoft.MediaFileRenamer.Services.FileRenamers;

namespace Inamsoft.MediaFileRenamer.Services.Tests;

public class MkvTests
{
    [Fact]
    public void ExtractTimestamp_Mkv_FallsBackToLastModified()
    {
        var file = TestFileHelper.GetVideo("sample_mkv.mkv");

        var found = TimestampExtractor.TryExtractTimestamp(file, out var timestamp);

        Assert.True(found);
        Assert.NotNull(timestamp);
        Assert.Equal(file.LastWriteTime, timestamp);
    }
}
