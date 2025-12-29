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
    /// Indicates the orientation of the photo during capture.
    /// </summary>
    /// <remarks>
    /// This property describes how the photo is oriented, such as whether it is
    /// in landscape or portrait mode. The orientation information is typically
    /// embedded in the photo's metadata and can be used to correct or adjust
    /// the display of images when viewed on different devices or software.
    /// </remarks>
    public string? Orientation { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public string? Altitude { get; set; }

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

    public string? XResolution { get; set; }

    public string? YResolution { get; set; }
    public string? Software { get; set; }

    public string? GpsImgDirection { get; set; }
    public string? GpsSatellites { get; set; }
    public string? GpsStatus { get; set; }

    public GpsLatitude? GpsLatitude { get; set; }
    public GpsLongitude? GpsLongitude { get; set; }

    public required FileMetadata FileMetadata { get; set; }

}
