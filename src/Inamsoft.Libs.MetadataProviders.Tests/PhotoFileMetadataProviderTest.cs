// /Volumes/ssd-1tb/coding/github/minam365/src/Inamsoft.Libs.MetadataProviders.Tests/PhotoFileMetadataProviderTest.cs

using JetBrains.Annotations;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Directory = MetadataExtractor.Directory;

namespace Inamsoft.Libs.MetadataProviders.Tests;

[TestSubject(typeof(PhotoFileMetadataProvider))]
public class PhotoFileMetadataProviderTest: IClassFixture<MetadataProviderFixture>
{
    private readonly MetadataProviderFixture _fixture;
    private readonly ITestContextAccessor _contextAccessor;
    private IPhotoFileMetadataProvider _provider;
    private ILogger<PhotoFileMetadataProvider> _logger;

    public PhotoFileMetadataProviderTest(MetadataProviderFixture fixture, ITestContextAccessor contextAccessor)
    {
        _fixture = fixture;
        _contextAccessor = contextAccessor;
        _provider = _fixture.PhotoFileMetadataProvider;
    }

    [Fact()]
    public void GetMetadata_FileDoesNotExist_ReturnsFileMetadata()
    {
        // Arrange
        var fileName = "nonexistent.jpg";
        var metadataProvider = _fixture.PhotoFileMetadataProvider;
        var fileInfo = new FileInfo(fileName);

        // Act
        var result = metadataProvider.GetMetadata(fileName);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.FileMetadata);
        Assert.Equal(fileName, result.FileMetadata.Name);
        Assert.Equal(Path.GetFileName(fileName), result.FileMetadata.Name);
        Assert.Equal(Path.GetFileNameWithoutExtension(fileName), result.FileMetadata.NameWithoutExtension);
        Assert.Equal(Path.GetExtension(fileName), result.FileMetadata.Extension);
        Assert.Equal(fileInfo.DirectoryName, result.FileMetadata.DirectoryName);
    }

    [Fact]
    public void GetMetadata_FileExists_ReturnsPopulatedFileMetadata_iPhone13()
    {
        // Arrange
        var filePath = @"..\..\..\..\..\assets\media-files\20251119_110721068_iOS.jpg";
        var canonicalFilePath = Path.GetFullPath(filePath);
        var fileInfo = new FileInfo(filePath);

        // Act
        var result = _provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.FileMetadata);
        Assert.Equal(canonicalFilePath, result.FileMetadata.Path);
        Assert.Equal(Path.GetFileName(filePath), result.FileMetadata.Name);
        Assert.Equal(Path.GetFileNameWithoutExtension(filePath), result.FileMetadata.NameWithoutExtension);
        Assert.Equal(Path.GetExtension(filePath), result.FileMetadata.Extension);
        Assert.Equal(fileInfo.DirectoryName ?? string.Empty, result.FileMetadata.DirectoryName);
        Assert.Equal(fileInfo.CreationTime, result.FileMetadata.CreatedAt);
        Assert.Equal(fileInfo.LastWriteTime, result.FileMetadata.ModifiedAt);
        Assert.Equal(fileInfo.Length, result.FileMetadata.Length);
        Assert.Equal("Apple", result.CameraMake);
        Assert.Equal("iPhone 13", result.CameraModel);
        Assert.Equal(3024, result.Height);
        Assert.Equal(4032, result.Width);
        Assert.Equal(DateTime.Parse("2025-11-19T12:07:21"), result.TakenAt);
        Assert.NotNull(result.Latitude);
        Assert.NotNull(result.Longitude);

    }

    [Fact]
    public void GetMetadata_FileExists_ReturnsPopulatedFileMetadata_Samsung_SMG781B()
    {
        // Arrange
        var filePath = @"..\..\..\..\..\assets\media-files\20251227_133149.jpg";
        var canonicalFilePath = Path.GetFullPath(filePath);
        var fileInfo = new FileInfo(filePath);

        // Act
        var result = _provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.FileMetadata);
        Assert.Equal(canonicalFilePath, result.FileMetadata.Path);
        Assert.Equal(Path.GetFileName(filePath), result.FileMetadata.Name);
        Assert.Equal(Path.GetFileNameWithoutExtension(filePath), result.FileMetadata.NameWithoutExtension);
        Assert.Equal(Path.GetExtension(filePath), result.FileMetadata.Extension);
        Assert.Equal(fileInfo.DirectoryName ?? string.Empty, result.FileMetadata.DirectoryName);
        Assert.Equal(fileInfo.CreationTime, result.FileMetadata.CreatedAt);
        Assert.Equal(fileInfo.LastWriteTime, result.FileMetadata.ModifiedAt);
        Assert.Equal(fileInfo.Length, result.FileMetadata.Length);
        Assert.Equal("samsung", result.CameraMake);
        Assert.Equal("SM-G781B", result.CameraModel);
        Assert.Equal(3024, result.Height);
        Assert.Equal(4032, result.Width);
        Assert.Equal(DateTime.Parse("2025-12-27T13:31:49"), result.TakenAt);
        Assert.Null(result.Latitude);
        Assert.Null(result.Longitude);

    }

    [Fact]
    public void GetMetadata_FileExists_ReturnsPopulatedFileMetadata_DNG()
    {
        // Arrange
        var filePath = @"..\..\..\..\..\assets\media-files\IMG_20181113_112412.dng";
        var canonicalFilePath = Path.GetFullPath(filePath);
        var fileInfo = new FileInfo(filePath);

        // Act
        var result = _provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.FileMetadata);
        Assert.Equal(canonicalFilePath, result.FileMetadata.Path);
        Assert.Equal(Path.GetFileName(filePath), result.FileMetadata.Name);
        Assert.Equal(Path.GetFileNameWithoutExtension(filePath), result.FileMetadata.NameWithoutExtension);
        Assert.Equal(Path.GetExtension(filePath), result.FileMetadata.Extension);
        Assert.Equal(fileInfo.DirectoryName ?? string.Empty, result.FileMetadata.DirectoryName);
        Assert.Equal(fileInfo.CreationTime, result.FileMetadata.CreatedAt);
        Assert.Equal(fileInfo.LastWriteTime, result.FileMetadata.ModifiedAt);
        Assert.Equal(fileInfo.Length, result.FileMetadata.Length);
        Assert.Equal("HUAWEI", result.CameraMake);
        Assert.Equal("LYA-L29", result.CameraModel);
        Assert.Equal(3840, result.Height);
        Assert.Equal(5120, result.Width);
        Assert.Equal(DateTime.Parse("2018-11-13T11:24:11"), result.TakenAt);
        Assert.Null(result.Latitude);
        Assert.Null(result.Longitude);

    }

    [Fact]
    public void GetMetadata_FileExists_ReturnsPopulatedFileMetadata_Heic()
    {
        // Arrange
        var filePath = @"..\..\..\..\..\assets\media-files\20260101_000327.heic";
        var canonicalFilePath = Path.GetFullPath(filePath);
        var fileInfo = new FileInfo(filePath);

        // Act
        var result = _provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.FileMetadata);
        Assert.Equal(canonicalFilePath, result.FileMetadata.Path);
        Assert.Equal(Path.GetFileName(filePath), result.FileMetadata.Name);
        Assert.Equal(Path.GetFileNameWithoutExtension(filePath), result.FileMetadata.NameWithoutExtension);
        Assert.Equal(Path.GetExtension(filePath), result.FileMetadata.Extension);
        Assert.Equal(fileInfo.DirectoryName ?? string.Empty, result.FileMetadata.DirectoryName);
        Assert.Equal(fileInfo.CreationTime, result.FileMetadata.CreatedAt);
        Assert.Equal(fileInfo.LastWriteTime, result.FileMetadata.ModifiedAt);
        Assert.Equal(fileInfo.Length, result.FileMetadata.Length);
        Assert.Equal("samsung", result.CameraMake);
        Assert.Equal("SM-G781B", result.CameraModel);
        Assert.Equal(4896, result.Height);
        Assert.Equal(6528, result.Width);
        Assert.Equal(DateTime.Parse("2026-01-01T00:03:27"), result.TakenAt);
        Assert.NotNull(result.Latitude);
        Assert.NotNull(result.Longitude);

    }

    [Fact]
    public void GetMetadata_FileExists_ReturnsPopulatedFileMetadata_Rw2()
    {
        // Arrange
        var filePath = @"..\..\..\..\..\assets\media-files\P1080216.RW2";
        var canonicalFilePath = Path.GetFullPath(filePath);
        var fileInfo = new FileInfo(filePath);

        // Act
        var result = _provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.FileMetadata);
        Assert.Equal(canonicalFilePath, result.FileMetadata.Path);
        Assert.Equal(Path.GetFileName(filePath), result.FileMetadata.Name);
        Assert.Equal(Path.GetFileNameWithoutExtension(filePath), result.FileMetadata.NameWithoutExtension);
        Assert.Equal(Path.GetExtension(filePath), result.FileMetadata.Extension);
        Assert.Equal(fileInfo.DirectoryName ?? string.Empty, result.FileMetadata.DirectoryName);
        Assert.Equal(fileInfo.CreationTime, result.FileMetadata.CreatedAt);
        Assert.Equal(fileInfo.LastWriteTime, result.FileMetadata.ModifiedAt);
        Assert.Equal(fileInfo.Length, result.FileMetadata.Length);
        Assert.Equal("Panasonic", result.CameraMake);
        Assert.Equal("DMC-FZ45", result.CameraModel);
        Assert.Equal(2454, result.Height);
        Assert.Equal(4396, result.Width);
        Assert.Equal(DateTime.Parse("2016-02-07T18:26:46"), result.TakenAt);
        Assert.Null(result.Latitude);
        Assert.Null(result.Longitude);

    }
    [Fact]
    public void GetMetadata_FileExists_ReturnsPopulatedFileMetadata_NoExif()
    {
        // Arrange
        var filePath = @"..\..\..\..\..\assets\media-files\BK_WP_BakuWallpaper_Krys_1280x1050.jpg";
        var canonicalFilePath = Path.GetFullPath(filePath);
        var fileInfo = new FileInfo(filePath);

        // Act
        var result = _provider.GetMetadata(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.FileMetadata);
        Assert.Equal(canonicalFilePath, result.FileMetadata.Path);
        Assert.Equal(Path.GetFileName(filePath), result.FileMetadata.Name);
        Assert.Equal(Path.GetFileNameWithoutExtension(filePath), result.FileMetadata.NameWithoutExtension);
        Assert.Equal(Path.GetExtension(filePath), result.FileMetadata.Extension);
        Assert.Equal(fileInfo.DirectoryName ?? string.Empty, result.FileMetadata.DirectoryName);
        Assert.Equal(fileInfo.CreationTime, result.FileMetadata.CreatedAt);
        Assert.Equal(fileInfo.LastWriteTime, result.FileMetadata.ModifiedAt);
        Assert.Equal(fileInfo.Length, result.FileMetadata.Length);
        Assert.Null(result.CameraMake);
        Assert.Null(result.CameraModel);
        Assert.Equal(1050, result.Height);
        Assert.Equal(1280, result.Width);
        Assert.Equal(DateTime.Parse("2009-07-20T17:39:08"), result.TakenAt);
        Assert.Equal(DateTime.Parse("2009-07-20T17:39:08"), result.DigitizedAt);
        Assert.Null(result.Latitude);
        Assert.Null(result.Longitude);

    }


    [Fact]
    public void GetMetadata_FileNameNull_ThrowsArgumentException()
    {
        // Arrange
        string fileName = null;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => _provider.GetMetadata(fileName));
        Assert.Equal("Parameter \"filePath\" (string) must not be null or whitespace, was null. (Parameter 'filePath')", ex.Message);
    }

    [Fact]
    public void GetMetadata_InvalidPath_ThrowsArgumentException()
    {
        // Arrange
        string fileName = "  ";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _provider.GetMetadata(fileName));
        //Assert.Equal("Value cannot be null or whitespace. (Parameter 'fileName')", ex.Message);
    }
}