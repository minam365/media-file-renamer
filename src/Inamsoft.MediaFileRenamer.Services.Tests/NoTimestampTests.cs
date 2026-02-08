using Inamsoft.MediaFileRenamer.Services.FileRenamers;

namespace Inamsoft.MediaFileRenamer.Services.Tests;

public class NoTimestampTests
{
    [Fact]
    public void ExtractTimestamp_NoMetadata_NoFilename_FallsBackToLastModified()
    {
        var file = TestFileHelper.GetVideo("DSC_2100.MP4");

        var found = TimestampExtractor.TryExtractTimestamp(file, out var timestamp);

        Assert.True(found);
        Assert.NotNull(timestamp);
        Assert.Equal(file.LastWriteTime, timestamp);
    }
}