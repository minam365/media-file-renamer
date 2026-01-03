using Inamsoft.Libs.MetadataProviders.Abstractions;

namespace Inamsoft.Libs.MediaFileRenaming
{
    public interface IFileNamingService
    {
        string GenerateDefaultFileName(PhotoFileMetadata photoFileMetadata, bool includeImageDimensions = true, bool includeGpsInfo = false);
        string GenerateDefaultFileName(VideoFileMetadata videoFileMetadata, bool includeImageDimensions = true);
        string GetTargetFilePath(string sourceFilePath, string targetFolderPath);
        string MakeUniqueTargetFilePath(string sourceFilePath, string targetFolderPath);
    }
}