using System;
using System.Collections.Generic;
using System.Text;
using AutoDictionary;
using Inamsoft.Libs.SourceGenerators.Attributes;

namespace Inamsoft.Libs.MetadataProviders.Abstractions;

/// <summary>
/// Represents metadata associated with a photo file. This metadata provides
/// details about the photo, such as camera information, geographic coordinates,
/// resolution, timestamps, and other relevant attributes.
/// </summary>
/// <remarks>
/// This class serves as a data structure for managing photo-specific metadata
/// while integrating with source generator attributes to produce constants and
/// dictionary mappings from its properties.
/// </remarks>
/// <example>
/// The metadata includes details like camera make and model, photo resolutions,
/// GPS coordinates, timestamps for when the photo was taken and digitized, and
/// software information.
/// </example>
[GenerateConstantsFromProps()]
[GenerateDictionary()]
public partial record PhotoFileMetadata
{
    /// <summary>
    /// Represents the make of the camera that captured the photo.
    /// </summary>
    /// <remarks>
    /// This property typically holds the manufacturer information of the camera
    /// (e.g., Canon, Nikon, Sony). It is part of the metadata available within
    /// photo files and can be used to identify the camera's brand.
    /// </remarks>
    public string? CameraMake { get; set; }

    /// <summary>
    /// Represents the model of the camera that captured the photo.
    /// </summary>
    /// <remarks>
    /// This property typically holds the specific model information of the camera
    /// (e.g., EOS 5D Mark IV, D850, Alpha 7 III). It is part of the metadata
    /// embedded within photo files and can be used to identify the exact camera model.
    /// </remarks>
    public string? CameraModel { get; set; }

    /// <summary>
    /// Gets or sets the latitude coordinate as a string representation.
    /// </summary>
    public string? Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude coordinate as a string representation.
    /// </summary>
    public string? Longitude { get; set; }

    /// <summary>
    /// Represents the date and time when the photo was taken.
    /// </summary>
    /// <remarks>
    /// This property stores the original timestamp of the photo's capture, as recorded
    /// in the metadata. It is typically used to determine when the photo was created
    /// or shot and can help to sequence or organize images chronologically.
    /// </remarks>
    public DateTime? TakenAt { get; set; }

    /// <summary>
    /// Represents the date and time when the photo was digitized.
    /// </summary>
    /// <remarks>
    /// This property captures the timestamp when the photo was converted into a digital format.
    /// It is part of the photo's metadata and provides information about the digitization process.
    /// </remarks>
    public DateTime? DigitizedAt { get; set; }

    /// <summary>
    /// Represents the height of the photo in pixels.
    /// </summary>
    /// <remarks>
    /// This property holds the vertical dimension of the photo as an integer value.
    /// It is an essential attribute for defining the resolution of the photo and
    /// can be used in various image processing or display scenarios.
    /// </remarks>
    public int? Height { get; set; } = null;

    /// <summary>
    /// Represents the width of the photo in pixels.
    /// </summary>
    /// <remarks>
    /// This property specifies the horizontal dimension of the photo, typically measured
    /// in pixels. It is part of the photo's metadata and can be used to determine its
    /// resolution or aspect ratio in conjunction with the height property.
    /// </remarks>
    public int? Width { get; set; } = null;

    public string CameraInfo
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(CameraMake))
            {
                parts.Add(CameraMake);
            }
            if (!string.IsNullOrEmpty(CameraModel))
            {
                parts.Add(CameraModel);
            }
            return string.Join(" ", parts).Trim();
        }
    }

    public string ImageDimensions
    {
        get
        {
            if (Width.HasValue && Height.HasValue)
            {
                return $"{Width.Value}x{Height.Value}";
            }
            return string.Empty;
        }
    }

    public string GpsCoordinates
    {
        get
        {
            if (!string.IsNullOrEmpty(Latitude) && !string.IsNullOrEmpty(Longitude))
            {
                return $"{Latitude} {Longitude}";
            }
            return string.Empty;
        }
    }

    public string SuggestedFileName
    {
        get
        {
            var sb = new StringBuilder();

            DateTime timestamp = (TakenAt.HasValue ? TakenAt : DigitizedAt.HasValue ? DigitizedAt.Value : null) ?? FileMetadata.ModifiedAt;
            sb.Append(timestamp.ToString("yyyyMMdd_HHmmss"));

            if (!string.IsNullOrEmpty(CameraInfo))
            {
                sb.Append($"_(Cam={CameraInfo})");
            }
            if (!string.IsNullOrEmpty(ImageDimensions))
            {
                sb.Append($"_(Dim={ImageDimensions})");
            }
            if (!string.IsNullOrEmpty(GpsCoordinates))
            {
                sb.Append($"_(Gps={GpsCoordinates})");
            }

            sb.Append($"_{FileMetadata.Name}");

            return sb.ToString();
        }
    }

    //public GpsLatitude? GpsLatitude { get; set; }
    //public GpsLongitude? GpsLongitude { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public required FileMetadata FileMetadata { get; set; }

}
