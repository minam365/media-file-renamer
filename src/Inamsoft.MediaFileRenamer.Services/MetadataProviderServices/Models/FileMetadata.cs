using Inamsoft.Libs.SourceGenerators.Attributes;

namespace Inamsoft.MediaFileRenamer.Services.MetadataProviderServices.Models;

/// <summary>
/// Represents metadata information about a file, including its path, name, extension, directory, existence, size, and
/// timestamps.
/// </summary>
/// <remarks>
/// This record provides convenient access to commonly used file properties and metadata. It can be used to retrieve or
/// store file details for processing, auditing, or display purposes. Thread safety is not guaranteed; if multiple
/// threads access an instance concurrently, external synchronization is required.
/// </remarks>
/// <param name="FileInfo">
/// The <see cref="System.IO.FileInfo"/> instance representing the file for which metadata is provided. Cannot be null.
/// </param>
[GenerateDictionary()]
[GenerateConstantsFromProps()]
public partial record FileMetadata()
{
    public FileMetadata(FileInfo fileInfo) : this()
    {
        FileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
        Path = fileInfo.FullName;
        Name = fileInfo.Name;
        NameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fileInfo.FullName);
        Extension = fileInfo.Extension;
        DirectoryName = fileInfo.DirectoryName ?? string.Empty;
        Exists = fileInfo.Exists;

        if(fileInfo.Exists)
        {
            DateCreated = fileInfo.CreationTime;
            DateCreatedUtc = fileInfo.CreationTimeUtc;
            DateModified = fileInfo.LastWriteTime;
            DateModifiedUtc = fileInfo.LastWriteTimeUtc;
            Length = fileInfo.Length;
        }
        else
        {
            DateCreated = default;
            DateModified = default;
            DateCreatedUtc = default;
            DateModifiedUtc = default;
            Length = default;
        }
    }

    public FileInfo FileInfo { get; init; } = default!;
    public string Path { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string NameWithoutExtension { get; init; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string DirectoryName { get; set; } = string.Empty;
    public bool Exists { get; set; }
    public long Length { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateCreatedUtc { get; set; }
    public DateTime DateModified { get; set; }
    public DateTime DateModifiedUtc { get; set; }

}