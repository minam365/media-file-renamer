using Inamsoft.MediaFileRenamer.Services.FileRenamers;

namespace Inamsoft.MediaFileRenamer.Services.Tests;

public class TimestampExtractorAviTests
{
    [Fact]
    public void ExtractTimestamp_Avi_FallsBackToLastModified()
    {
        var file = TestFileHelper.GetVideo("DSCF0007.AVI");

        var found = TimestampExtractor.TryExtractTimestamp(file, out var timestamp);
        
        Assert.True(found);
        Assert.NotNull(timestamp);
        Assert.Equal(file.LastWriteTime, timestamp);
    }
}
