using Inamsoft.Libs.MetadataProviders.Abstractions;
using JetBrains.Annotations;
using System;
using System.IO;
using Xunit;

namespace Inamsoft.Libs.MetadataProviders.Tests;

[TestSubject(typeof(FileMetadataProvider))]
public class FileMetadataProviderTest : IClassFixture<MetadataProviderFixture>
{
    private readonly MetadataProviderFixture _fixture;

    public FileMetadataProviderTest(MetadataProviderFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void GetMetadata_WithValidFilePath_ReturnsFileMetadata()
    {
        // Arrange
        var filePath = Path.GetTempFileName();
        var fileInfo = new FileInfo(filePath);
        var provider = _fixture.FileMetadataProvider;

        // Act
        var result = provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fileInfo.FullName, result.Path);
        Assert.Equal(fileInfo.Name, result.Name);
        Assert.Equal(Path.GetFileNameWithoutExtension(filePath), result.NameWithoutExtension);
        Assert.Equal(fileInfo.Extension, result.Extension);
        Assert.True(result.Exists);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void GetMetadata_WithNonexistentFile_ReturnsFileMetadataWithExistsFalse()
    {
        // Arrange
        var filePath = "nonexistent_file_12345.txt";
        var provider = _fixture.FileMetadataProvider;

        // Act
        var result = provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Exists);
        Assert.Equal(0, result.Length);
        Assert.Equal(default(DateTime), result.CreatedAt);
        Assert.Equal(default(DateTime), result.ModifiedAt);
    }

    [Fact]
    public void GetMetadata_WithExistingFile_PopulatesAllProperties()
    {
        // Arrange
        var filePath = Path.GetTempFileName();
        var fileInfo = new FileInfo(filePath);
        var provider = _fixture.FileMetadataProvider;

        // Act
        var result = provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fileInfo.FullName, result.Path);
        Assert.Equal(fileInfo.Name, result.Name);
        Assert.Equal(Path.GetFileNameWithoutExtension(filePath), result.NameWithoutExtension);
        Assert.Equal(fileInfo.Extension, result.Extension);
        Assert.Equal(fileInfo.DirectoryName ?? string.Empty, result.DirectoryName);
        Assert.True(result.Exists);
        Assert.Equal(fileInfo.Length, result.Length);
        Assert.Equal(fileInfo.CreationTime, result.CreatedAt);
        Assert.Equal(fileInfo.LastWriteTime, result.ModifiedAt);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void GetMetadata_WithFileWithoutExtension_ReturnsCorrectData()
    {
        // Arrange
        var tempDir = Path.GetTempPath();
        var filePath = Path.Combine(tempDir, "file_without_extension");
        File.WriteAllText(filePath, "test content");
        var provider = _fixture.FileMetadataProvider;

        // Act
        var result = provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("file_without_extension", result.Name);
        Assert.Equal("file_without_extension", result.NameWithoutExtension);
        Assert.Equal(string.Empty, result.Extension);
        Assert.True(result.Exists);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void GetMetadata_WithFileWithMultipleDots_ReturnsCorrectExtension()
    {
        // Arrange
        var tempDir = Path.GetTempPath();
        var filePath = Path.Combine(tempDir, "file.backup.txt");
        File.WriteAllText(filePath, "test content");
        var provider = _fixture.FileMetadataProvider;

        // Act
        var result = provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("file.backup.txt", result.Name);
        Assert.Equal("file.backup", result.NameWithoutExtension);
        Assert.Equal(".txt", result.Extension);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void GetMetadata_WithNullFilePath_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = _fixture.FileMetadataProvider;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => provider.GetMetadata(null!));
    }

    [Fact]
    public void GetMetadata_WithEmptyFilePath_ThrowsArgumentException()
    {
        // Arrange
        var provider = _fixture.FileMetadataProvider;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => provider.GetMetadata(string.Empty));
    }

    [Fact]
    public void GetMetadata_WithWhitespaceFilePath_ThrowsArgumentException()
    {
        // Arrange
        var provider = _fixture.FileMetadataProvider;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => provider.GetMetadata("   "));
    }

    [Fact]
    public void GetMetadata_WithRelativeFilePath_ReturnsCorrectAbsolutePath()
    {
        // Arrange
        var tempDir = Path.GetTempPath();
        var fileName = "test_relative.txt";
        var filePath = Path.Combine(tempDir, fileName);
        File.WriteAllText(filePath, "test content");
        var provider = _fixture.FileMetadataProvider;

        // Act
        var result = provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Path.GetFullPath(filePath), result.Path);
        Assert.True(result.Exists);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void GetMetadata_WithLargeFile_ReturnsCorrectLength()
    {
        // Arrange
        var filePath = Path.GetTempFileName();
        const int fileSize = 1024 * 1024; // 1 MB
        var fileContent = new byte[fileSize];
        File.WriteAllBytes(filePath, fileContent);
        var provider = _fixture.FileMetadataProvider;

        // Act
        var result = provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fileSize, result.Length);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void GetMetadata_WithEmptyFile_ReturnsZeroLength()
    {
        // Arrange
        var filePath = Path.GetTempFileName();
        File.WriteAllText(filePath, string.Empty);
        var provider = _fixture.FileMetadataProvider;

        // Act
        var result = provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Length);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void GetMetadata_WithSpecialCharactersInFileName_ReturnsCorrectData()
    {
        // Arrange
        var tempDir = Path.GetTempPath();
        var filePath = Path.Combine(tempDir, "file_with_@special#chars_123.txt");
        File.WriteAllText(filePath, "test content");
        var provider = _fixture.FileMetadataProvider;

        // Act
        var result = provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("file_with_@special#chars_123.txt", result.Name);
        Assert.Equal("file_with_@special#chars_123", result.NameWithoutExtension);
        Assert.Equal(".txt", result.Extension);
        Assert.True(result.Exists);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void GetMetadata_WithUnicodeCharactersInFileName_ReturnsCorrectData()
    {
        // Arrange
        var tempDir = Path.GetTempPath();
        var filePath = Path.Combine(tempDir, "файл_文件_🎵.txt");
        File.WriteAllText(filePath, "test content");
        var provider = _fixture.FileMetadataProvider;

        // Act
        var result = provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("файл_文件_🎵.txt", result.Name);
        Assert.Equal("файл_文件_🎵", result.NameWithoutExtension);
        Assert.Equal(".txt", result.Extension);
        Assert.True(result.Exists);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void GetMetadata_WithFileInNestedDirectory_ReturnsCorrectDirectoryPath()
    {
        // Arrange
        var tempDir = Path.GetTempPath();
        var nestedDir = Path.Combine(tempDir, "nested", "folder", "structure");
        Directory.CreateDirectory(nestedDir);
        var filePath = Path.Combine(nestedDir, "test.txt");
        File.WriteAllText(filePath, "test content");
        var fileInfo = new FileInfo(filePath);
        var provider = _fixture.FileMetadataProvider;

        // Act
        var result = provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fileInfo.DirectoryName ?? string.Empty, result.DirectoryName);
        Assert.True(result.Exists);

        // Cleanup
        File.Delete(filePath);
        System.IO.Directory.Delete(nestedDir, true);
    }

    [Fact]
    public void GetMetadata_CalledMultipleTimes_ReturnsSameMetadata()
    {
        // Arrange
        var filePath = Path.GetTempFileName();
        var provider = _fixture.FileMetadataProvider;

        // Act
        var result1 = provider.GetMetadata(filePath);
        var result2 = provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.Path, result2.Path);
        Assert.Equal(result1.Name, result2.Name);
        Assert.Equal(result1.Exists, result2.Exists);
        Assert.Equal(result1.Length, result2.Length);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void GetMetadata_AfterFileModification_ReturnsUpdatedModifiedAtTime()
    {
        // Arrange
        var filePath = Path.GetTempFileName();
        File.WriteAllText(filePath, "initial content");
        var provider = _fixture.FileMetadataProvider;
        var result1 = provider.GetMetadata(filePath);

        // Wait a bit to ensure different timestamp
        System.Threading.Thread.Sleep(100);

        // Act
        File.WriteAllText(filePath, "modified content");
        var result2 = provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.True(result2.ModifiedAt >= result1.ModifiedAt);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void GetMetadata_WithFileHavingReadOnlyAttribute_ReturnsCorrectData()
    {
        // Arrange
        var filePath = Path.GetTempFileName();
        File.WriteAllText(filePath, "test content");
        var fileInfo = new FileInfo(filePath);
        fileInfo.Attributes = FileAttributes.ReadOnly;
        var provider = _fixture.FileMetadataProvider;

        // Act
        var result = provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Exists);
        Assert.Equal(fileInfo.Name, result.Name);

        // Cleanup
        fileInfo.Attributes = FileAttributes.Normal;
        File.Delete(filePath);
    }

    [Fact]
    public void GetMetadata_WithHiddenFile_ReturnsCorrectData()
    {
        // Arrange
        var filePath = Path.GetTempFileName();
        File.WriteAllText(filePath, "test content");
        var fileInfo = new FileInfo(filePath);
        fileInfo.Attributes = FileAttributes.Hidden;
        var provider = _fixture.FileMetadataProvider;

        // Act
        var result = provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Exists);
        Assert.Equal(fileInfo.Name, result.Name);

        // Cleanup
        fileInfo.Attributes = FileAttributes.Normal;
        File.Delete(filePath);
    }

    [Fact]
    public void GetMetadata_ReturnsFileMetadataInstance()
    {
        // Arrange
        var filePath = Path.GetTempFileName();
        var provider = _fixture.FileMetadataProvider;

        // Act
        var result = provider.GetMetadata(filePath);

        // Assert
        Assert.IsType<FileMetadata>(result);
        Assert.NotNull(result.FileInfo);

        // Cleanup
        File.Delete(filePath);
    }
}
