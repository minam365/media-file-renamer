using Inamsoft.MediaFileRenamer.Services.FileRenamers;

namespace Inamsoft.MediaFileRenamer.Services.Tests;

public class AviTests
{
    [Fact]
    public void ExtractTimestamp_Avi_FallsBackToLastModified()
    {
        var file = TestFileHelper.GetVideo("sample_avi.avi");

        var found = TimestampExtractor.TryExtractTimestamp(file, out var timestamp);
        
        Assert.True(found);
        Assert.NotNull(timestamp);
        Assert.Equal(file.LastWriteTime, timestamp);
    }
}
