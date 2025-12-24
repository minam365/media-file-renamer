using Inamsoft.Libs.SourceGenerators.Attributes;

namespace Inamsoft.Libs.MetadataProviders.Abstractions;

[GenerateDictionary()]
public partial record FileMetadata
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string FileNameWithoutExtension { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string DirectoryName { get; set; } = string.Empty;
    public string DirectoryPath { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }

}
