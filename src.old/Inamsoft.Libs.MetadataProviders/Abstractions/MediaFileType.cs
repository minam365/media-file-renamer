/* 
PSEUDOCODE / PLAN (detailed)
- Goal: Add XML documentation comments to the MediaFileMetadata record struct and MediaFileType enum.
- Steps:
  1. Prepend a detailed comment block explaining what will be changed (for reviewer).
  2. For the record struct 'MediaFileMetadata':
     - Add a <summary> describing the purpose and usage of the record.
     - Add <param> tags for each primary constructor parameter: FileMetadata, PhotoFileMetadata, VideoFileMetadata.
     - Add remarks describing behavior when both Photo and Video metadata are present and note immutability as a readonly record struct.
  3. For each property inside the record:
     - Add <summary> on MediaFileType property explaining selection precedence (Photo first, then Video, otherwise Unknown).
     - Add <remarks> if necessary to explain that Audio is not produced by current logic.
     - Add <summary> on Exists forwarding to FileMetadata.Exists.
     - Add <summary> on IsPhoto, IsVideo, IsSupportedMediaType explaining what they indicate.
  4. For the MediaFileType enum:
     - Add <summary> for the enum overall.
     - Add <summary> for each enum member (Unknown, Photo, Video, Audio) describing intended meaning.
  5. Preserve existing code structure, ensure XML doc tags are well-formed and concise.
  6. Keep comments compatible with tooling that generates IntelliSense documentation.
*/

namespace Inamsoft.Libs.MetadataProviders.Abstractions;

/// <summary>
/// Enumerates supported media file types recognized by the metadata providers.
/// </summary>
public enum MediaFileType
{
    /// <summary>
    /// The media file type is unknown or not determined.
    /// </summary>
    Unknown,

    /// <summary>
    /// The media file represents a photo (image) and photo-specific metadata is available.
    /// </summary>
    Photo,

    /// <summary>
    /// The media file represents a video and video-specific metadata is available.
    /// </summary>
    Video,

    /// <summary>
    /// The media file represents audio media. Note: this enum value exists for completeness but the
    /// current <see cref="MediaFileMetadata"/> logic does not produce <see cref="Audio"/>.
    /// </summary>
    Audio
}
