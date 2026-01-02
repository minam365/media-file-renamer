using System;
using System.IO;
using Inamsoft.Libs.MetadataProviders;
using Inamsoft.Libs.MetadataProviders.Abstractions;
using JetBrains.Annotations;
using MetadataExtractor;
using NSubstitute;
using Xunit;

namespace Inamsoft.Libs.MetadataProviders.Tests;

[TestSubject(typeof(VideoFileMetadataProvider))]
public class VideoFileMetadataProviderTest : IClassFixture<MetadataProviderFixture>
{
    private readonly MetadataProviderFixture _fixture;

    public VideoFileMetadataProviderTest(MetadataProviderFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void GetMetadata_FileDoesNotExist_ReturnsVideoFileMetadataExistsFalse()
    {
        // Arrange
        var fileName = "nonexistent_video.mp4";
        var provider = _fixture.VideoFileMetadataProvider;

        // Act
        var result = provider.GetMetadata(fileName);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.FileMetadata);
        Assert.False(result.FileMetadata.Exists);
    }

    [Fact]
    public void GetMetadata_FileExists_ReturnsPopulatedVideoFileMetadata_MovFile()
    {
        // Arrange
        // Arrange
        var filePath = @"..\..\..\..\..\assets\media-files\20251120_064505000_iOS.MOV";
        var canonicalFilePath = Path.GetFullPath(filePath);
        var fileInfo = new FileInfo(filePath);
        var provider = _fixture.VideoFileMetadataProvider;

        // Act
        var result = provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.FileMetadata);
        Assert.Equal(fileInfo.FullName, result.FileMetadata.Path);
        Assert.Equal(fileInfo.Name, result.FileMetadata.Name);
        Assert.Equal(Path.GetFileNameWithoutExtension(filePath), result.FileMetadata.NameWithoutExtension);
        Assert.Equal(fileInfo.Extension, result.FileMetadata.Extension);
        Assert.Equal(fileInfo.DirectoryName, result.FileMetadata.DirectoryName);
        Assert.True(result.FileMetadata.Exists);
        Assert.Equal(fileInfo.Length, result.FileMetadata.Length);
        Assert.Equal(fileInfo.CreationTime, result.FileMetadata.CreatedAt);
        Assert.Equal(fileInfo.LastWriteTime, result.FileMetadata.ModifiedAt);
    }

    [Fact]
    public void GetMetadata_FileExists_ReturnsPopulatedVideoFileMetadata_Mp4()
    {
        // Arrange
        // Arrange
        var filePath = @"..\..\..\..\..\assets\media-files\20180105_191746.mp4";
        var canonicalFilePath = Path.GetFullPath(filePath);
        var fileInfo = new FileInfo(filePath);
        var provider = _fixture.VideoFileMetadataProvider;

        // Act
        var result = provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.FileMetadata);
        Assert.Equal(fileInfo.FullName, result.FileMetadata.Path);
        Assert.Equal(fileInfo.Name, result.FileMetadata.Name);
        Assert.Equal(Path.GetFileNameWithoutExtension(filePath), result.FileMetadata.NameWithoutExtension);
        Assert.Equal(fileInfo.Extension, result.FileMetadata.Extension);
        Assert.Equal(fileInfo.DirectoryName, result.FileMetadata.DirectoryName);
        Assert.True(result.FileMetadata.Exists);
        Assert.Equal(fileInfo.Length, result.FileMetadata.Length);
        Assert.Equal(fileInfo.CreationTime, result.FileMetadata.CreatedAt);
        Assert.Equal(fileInfo.LastWriteTime, result.FileMetadata.ModifiedAt);

    }

    [Fact]
    public void GetMetadata_NullOrWhitespacePath_ThrowsArgumentException()
    {
        var provider = _fixture.VideoFileMetadataProvider;

        Assert.Throws<ArgumentNullException>(() => provider.GetMetadata(null!));
        Assert.Throws<ArgumentException>(() => provider.GetMetadata(string.Empty));
        Assert.Throws<ArgumentException>(() => provider.GetMetadata("   "));
    }
}
