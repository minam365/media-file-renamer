using System;
using System.Collections.Generic;
using System.Text;
using AutoDictionary;

namespace Inamsoft.Libs.MetadataProviders.Abstractions;

[AutoDictionary()]
public partial record PhotoFileMetadata
{
    public string? CameraMake { get; init; }
    public string? CameraModel { get; init; }
    public string? Orientation { get; init; }
    public string? Latitude { get; init; }
    public string? Longitude { get; init; }
    public string? Altitude { get; init; }

    public DateTime? TakenAt { get; init; }
    public DateTime? DigitizedAt { get; init; }

    public int? Height { get; set; } = null;
    public int? Width { get; set; } = null;

    public string? XResolution { get; set; }

    public string? YResolution { get; set; }
    public string? Software { get; set; }

    public string? GpsImgDirection { get; set; }
    public string? GpsSatellites { get; set; }
    public string? GpsStatus { get; set; }

    public GpsLatitude? GpsLatitude { get; set; }
    public GpsLongitude? GpsLongitude { get; set; }

    public required FileMetadata FileMetadata { get; init; }

}
