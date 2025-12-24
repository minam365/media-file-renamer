using System;
using System.Collections.Generic;
using System.Text;
using Inamsoft.Libs.SourceGenerators.Attributes;

namespace Inamsoft.Libs.MetadataProviders.Abstractions;

[GenerateDictionary()]
public partial record VideoFileMetadata
{
    public string? TrackId { get; set; }
    public string? TrackDuration { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public int? Height { get; set; } = null;
    public int? Width { get; set; } = null;
        
    public required FileMetadata FileMetadata { get; init; }
        
}