using Inamsoft.Libs.MetadataProviders;

namespace Inamsoft.Libs.MediaFileRenaming.Tests
{
    public class FileNamingServiceTest : IClassFixture<MediaFileRenamingFixture>
    {
        private readonly MediaFileRenamingFixture _fixture;

        private IPhotoFileMetadataProvider PhotoFileMetadataProvider => _fixture.PhotoFileMetadataProvider;
        private IVideoFileMetadataProvider VideoFileMetadataProvider => _fixture.VideoFileMetadataProvider;
        private IFileNamingService FileNamingService => _fixture.FileNamingService;

        public FileNamingServiceTest(MediaFileRenamingFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void GetTargetFilePath_FileExists_ReturnsExpectedFilePath_iPhone13()
        {
            // Arrange
            var filePath = @"..\..\..\..\..\assets\media-files\20251119_110721068_iOS.jpg";
            var canonicalFilePath = Path.GetFullPath(filePath);
            var fileInfo = new FileInfo(filePath);
            var tempFolderPath = Path.GetTempPath();
            var expectedTargetFilePath = Path.Combine(tempFolderPath, "2025", "11. November", "20251119_120721_(Apple iPhone 13)_(4032x3024)_20251119_110721068_iOS.jpg");

            // Act
            var result = FileNamingService.GetTargetFilePath(canonicalFilePath, tempFolderPath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTargetFilePath, result);
        }

        [Fact]
        public void GetTargetFilePath_FileExists_ReturnsExpectedFilePath_Rw2()
        {
            // Arrange
            var filePath = @"..\..\..\..\..\assets\media-files\P1080216.RW2";
            var canonicalFilePath = Path.GetFullPath(filePath);
            var fileInfo = new FileInfo(filePath);
            var tempFolderPath = Path.GetTempPath();
            var expectedTargetFilePath = Path.Combine(tempFolderPath, "2016", "02. February", "20160207_182646_(Panasonic DMC-FZ45)_(4396x2454)_P1080216.RW2");

            // Act
            var result = FileNamingService.GetTargetFilePath(canonicalFilePath, tempFolderPath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTargetFilePath, result);
        }

        [Fact]
        public void MakeUniqueTargetFilePath_FileExists_ReturnsExpectedFilePath_iPhone13()
        {
            // Arrange
            var filePath = @"..\..\..\..\..\assets\media-files\20251119_110721068_iOS.jpg";
            var canonicalFilePath = Path.GetFullPath(filePath);
            var fileInfo = new FileInfo(filePath);
            var tempFolderPath = Path.GetTempPath();
            var expectedTargetFilePath = Path.Combine(tempFolderPath, "2025", "11. November", "20251119_120721_(Apple iPhone 13)_(4032x3024)_20251119_110721068_iOS.jpg");

            // Act
            var result = FileNamingService.MakeUniqueTargetFilePath(canonicalFilePath, tempFolderPath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTargetFilePath, result);
        }


        [Fact]
        public void MakeUniqueTargetFilePath_FileExists_ReturnsNumberedFilePath_Mts_on_Mac()
        {
            // Arrange
            var filePath = @"/Volumes/personal_folder/Seagate 2TB/partition 1/inam.photo2/Photos/00003.MTS";
            var fileInfo = new FileInfo(filePath);
            var tempFolderPath = Path.GetTempPath();
            var expectedTargetFilePath0 = Path.Combine(tempFolderPath, "2025", "11. November", "20251119_120721_(Apple iPhone 13)_(4032x3024)_20251119_110721068_iOS.jpg");
            var expectedTargetFilePath1 = Path.Combine(tempFolderPath, "2025", "11. November", "20251119_120721_(Apple iPhone 13)_(4032x3024)_20251119_110721068_iOS (1).jpg");

            // Act
            var result0 = FileNamingService.MakeUniqueTargetFilePath(filePath, tempFolderPath);
            
            // Assert
            Assert.NotNull(result0);
            Assert.Equal(expectedTargetFilePath0, result0);

            // Clean up
            File.Delete(result0);
            Directory.Delete(Path.Combine(tempFolderPath, "2025"), true);
        }
    }
}
