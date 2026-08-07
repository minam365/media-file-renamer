namespace Inamsoft.MediaFileRenamer.Services.Tests;

public static class TestFileHelper
{
    public static FileInfo GetVideo(string fileName)
    {
        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "TestData", "Videos", fileName);

        var filePath = @$"..\..\..\..\..\assets\media-files\{fileName}";
        return new FileInfo(filePath);
    }
}
