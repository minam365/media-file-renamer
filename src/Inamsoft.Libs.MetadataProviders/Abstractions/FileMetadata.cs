using Inamsoft.Libs.SourceGenerators.Attributes;

namespace Inamsoft.Libs.MetadataProviders.Abstractions;

/// <summary>
/// Represents metadata information about a file, including its path, name, extension, directory, existence, size, and
/// timestamps.
/// </summary>
/// <remarks>This record provides convenient access to commonly used file properties and metadata. It can be used
/// to retrieve or store file details for processing, auditing, or display purposes. Thread safety is not guaranteed; if
/// multiple threads access an instance concurrently, external synchronization is required.</remarks>
/// <param name="FileInfo">The <see cref="System.IO.FileInfo"/> instance representing the file for which metadata is provided. Cannot be null.</param>
[GenerateDictionary()]
[GenerateConstantsFromProps()]
public partial record FileMetadata(FileInfo FileInfo)
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string NameWithoutExtension { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string DirectoryName { get; set; } = string.Empty;
    public bool Exists { get; set; }
    public long Length { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }

}