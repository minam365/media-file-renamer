// /Volumes/ssd-1tb/coding/github/minam365/src/Inamsoft.Libs.MetadataProviders.Tests/PhotoFileMetadataProviderTest.cs

using Inamsoft.Libs.MetadataProviders;
using Inamsoft.Libs.MetadataProviders.Abstractions;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Directory = MetadataExtractor.Directory;

namespace Inamsoft.Libs.MetadataProviders.Tests;

[TestSubject(typeof(PhotoFileMetadataProvider))]
public class PhotoFileMetadataProviderTest: IClassFixture<MetadataProviderFixture>
{
    private readonly MetadataProviderFixture _fixture;
    private readonly ITestContextAccessor _contextAccessor;
    private PhotoFileMetadataProvider _provider;
    private ILogger<PhotoFileMetadataProvider> _logger;

    public PhotoFileMetadataProviderTest(MetadataProviderFixture fixture, ITestContextAccessor contextAccessor)
    {
        _fixture = fixture;
        _contextAccessor = contextAccessor;
    }

    [Fact()]
    public void GetMetadata_FileDoesNotExist_ReturnsFileMetadata()
    {
        // Arrange
        var fileName = "nonexistent.jpg";
        var metadataProvider = _fixture.PhotoFileMetadataProvider;

        // Act
        var result = metadataProvider.GetMetadata(fileName);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.FileMetadata);
        Assert.Equal(fileName, result.FileMetadata.FilePath);
        Assert.Equal(Path.GetFileName(fileName), result.FileMetadata.FileName);
        Assert.Equal(Path.GetFileNameWithoutExtension(fileName), result.FileMetadata.FileNameWithoutExtension);
        Assert.Equal(Path.GetExtension(fileName), result.FileMetadata.FileExtension);
        Assert.Equal(string.Empty, result.FileMetadata.DirectoryPath);
        Assert.Equal(string.Empty, result.FileMetadata.DirectoryName);
    }

    [Fact]
    public void GetMetadata_FileExists_ReturnsPopulatedFileMetadata()
    {
        // Arrange
        var fileName = Path.GetTempFileName();
        var fileInfo = new FileInfo(fileName);

        // Act
        var result = _provider.GetMetadata(fileName);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.FileMetadata);
        Assert.Equal(fileName, result.FileMetadata.FilePath);
        Assert.Equal(Path.GetFileName(fileName), result.FileMetadata.FileName);
        Assert.Equal(Path.GetFileNameWithoutExtension(fileName), result.FileMetadata.FileNameWithoutExtension);
        Assert.Equal(Path.GetExtension(fileName), result.FileMetadata.FileExtension);
        Assert.Equal(fileInfo.DirectoryName, result.FileMetadata.DirectoryPath);
        Assert.Equal(fileInfo.Directory?.Name ?? string.Empty, result.FileMetadata.DirectoryName);
        Assert.Equal(fileInfo.CreationTime, result.FileMetadata.CreatedAt);
        Assert.Equal(fileInfo.LastWriteTime, result.FileMetadata.ModifiedAt);

        // Cleanup
        File.Delete(fileName);
    }

    [Fact]
    public void GetMetadata_ValidExifData_PopulatesPhotoMetadata()
    {
        // Arrange
        var fileName = Path.GetTempFileName();
        var directories = new List<Directory>();
        var exifSubIfdDirectory = new ExifSubIfdDirectory();
        exifSubIfdDirectory.Set(ExifDirectoryBase.TagDateTimeOriginal, DateTime.Parse("2023-01-01T12:34:56"));
        directories.Add(exifSubIfdDirectory);

        var exifIfd0Directory = new ExifIfd0Directory();
        directories.Add(exifIfd0Directory);

        ImageMetadataReader.ReadMetadata(Arg.Any<string>()).Returns(directories);

        // Act
        var result = _provider.GetMetadata(fileName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("2023-01-01T12:34:56", result.TakenAt?.ToString("yyyy-MM-ddTHH:mm:ss"));

        // Cleanup
        File.Delete(fileName);
    }

    [Fact]
    public void GetMetadata_NoExifData_ReturnsDefaultPhotoMetadata()
    {
        // Arrange
        var fileName = Path.GetTempFileName();
        ImageMetadataReader.ReadMetadata(Arg.Any<string>()).Returns(new List<Directory>());

        // Act
        var result = _provider.GetMetadata(fileName);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.TakenAt);

        // Cleanup
        File.Delete(fileName);
    }

    [Fact]
    public void GetMetadata_FileNameNull_ThrowsArgumentException()
    {
        // Arrange
        string fileName = null;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => _provider.GetMetadata(fileName));
        Assert.Equal("Value cannot be null. (Parameter 'fileName')", ex.Message);
    }

    [Fact]
    public void GetMetadata_InvalidPath_ThrowsArgumentException()
    {
        // Arrange
        string fileName = "  ";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _provider.GetMetadata(fileName));
        Assert.Equal("Value cannot be null or whitespace. (Parameter 'fileName')", ex.Message);
    }
}