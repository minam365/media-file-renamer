using Inamsoft.Libs.MetadataProviders.Abstractions;

namespace Inamsoft.Libs.MediaFileRenaming
{
    public interface IFileNamingService
    {
        string GenerateDefaultFileName(FileMetadata fileMetadata, string? targetFileNamePrefix = null);
        string GenerateDefaultFileName(PhotoFileMetadata photoFileMetadata, bool includeImageDimensions = true, bool includeGpsInfo = false, string? targetFileNamePrefix = null);
        string GenerateDefaultFileName(VideoFileMetadata videoFileMetadata, bool includeImageDimensions = true, string? targetFileNamePrefix = null);
        
        /// <summary>
        /// Generates the target file path for a source file path based on the provided target folder path
        /// and an optional file name prefix.
        /// </summary>
        /// <param name="sourceFilePath">The full path to the source file.</param>
        /// <param name="targetFolderPath">The destination folder path where the file will be placed.</param>
        /// <param name="targetFileNamePrefix">An optional prefix to include in the generated file name.</param>
        /// <returns>The full path to the target file including file name and extension.</returns>
        string GetTargetFilePath(string sourceFilePath, string targetFolderPath, string? targetFileNamePrefix = null);

        /// <summary>
        /// Generates a unique file path for a source file in the specified target folder,
        /// ensuring no conflicts with existing files, and optionally including a file name prefix.
        /// </summary>
        /// <param name="sourceFilePath">The full path to the source file that needs to be renamed or moved.</param>
        /// <param name="targetFolderPath">The destination folder path where the file will be placed.</param>
        /// <param name="targetFileNamePrefix">An optional prefix to include in the generated file name. Defaults to null.</param>
        /// <returns>The full path to the unique target file, including the file name and extension.</returns>
        string MakeUniqueTargetFilePath(string sourceFilePath, string targetFolderPath,
            string? targetFileNamePrefix = null);
    }
}