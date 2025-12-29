using System;
using System.Collections.Generic;
using System.Text;
using Inamsoft.Libs.SourceGenerators.Attributes;

namespace Inamsoft.Libs.MetadataProviders.Abstractions;

/// <summary>
/// Represents metadata associated with a video file, including file details, video dimensions, timestamps, and track information.
/// </summary>
[GenerateConstantsFromProps()]
[GenerateDictionary()]
public partial record VideoFileMetadata
{
    public string? TrackId { get; set; }
    public string? TrackDuration { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the video file was created.
    /// </summary>
    /// <remarks>
    /// This property represents the creation timestamp of the video file metadata.
    /// It is nullable, meaning it can hold a <c>null</c> value if the creation date is not provided or unknown.
    /// </remarks>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the video file metadata was last modified.
    /// </summary>
    /// <remarks>
    /// This property represents the last modification timestamp of the video file metadata.
    /// It is nullable, indicating that it can hold a <c>null</c> value if the modification date is not set or unknown.
    /// </remarks>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the height of the video in pixels.
    /// </summary>
    /// <remarks>
    /// This property represents the vertical resolution of the video, typically measured in pixels.
    /// It is nullable, meaning it can hold a <c>null</c> value if the height is not provided or unknown.
    /// </remarks>
    public int? Height { get; set; } = null;

    /// <summary>
    /// Gets or sets the width of the video frame in pixels.
    /// </summary>
    /// <remarks>
    /// This property represents the horizontal resolution of the video.
    /// It is nullable, meaning it can hold a <c>null</c> value if the width is not specified or unavailable.
    /// </remarks>
    public int? Width { get; set; } = null;
        
    public required FileMetadata FileMetadata { get; init; }
        
}