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
/// Represents metadata for a media file, combining general file information with optional photo- or video-specific metadata.
/// </summary>
/// <remarks>
/// Use this type to access common file metadata together with additional metadata when the file is a photo or a video.
/// The <see cref="MediaFileType"/> is determined by the presence of <see cref="PhotoFileMetadata"/> or
/// <see cref="VideoFileMetadata"/>: photo metadata takes precedence when both are provided.
/// This type is implemented as a readonly record struct to prefer value semantics while remaining immutable.
/// </remarks>
/// <param name="FileMetadata">The general file metadata associated with the media file. Must not be null.</param>
/// <param name="PhotoFileMetadata">Photo-specific metadata for the file, or <see langword="null"/> if the file is not a photo.</param>
/// <param name="VideoFileMetadata">Video-specific metadata for the file, or <see langword="null"/> if the file is not a video.</param>
public readonly record struct MediaFileMetadata(FileMetadata FileMetadata, PhotoFileMetadata? PhotoFileMetadata = null, VideoFileMetadata? VideoFileMetadata = null)
{
    /// <summary>
    /// Gets the type of media file represented by this instance.
    /// </summary>
    /// <remarks>
    /// The selection logic gives priority to photo metadata: if <see cref="PhotoFileMetadata"/> is present,
    /// <see cref="MediaFileType.Photo"/> is returned. Otherwise, if <see cref="VideoFileMetadata"/> is present,
    /// <see cref="MediaFileType.Video"/> is returned. If neither is present, <see cref="MediaFileType.Unknown"/>
    /// is returned. The enum also defines <see cref="MediaFileType.Audio"/>, but this record does not set it.
    /// </remarks>
    public MediaFileType MediaFileType
    {
        get
        {
            if (PhotoFileMetadata is not null)
                return MediaFileType.Photo;
            if (VideoFileMetadata is not null)
                return MediaFileType.Video;
            return MediaFileType.Unknown;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the underlying file exists on disk.
    /// </summary>
    /// <remarks>
    /// This property forwards to <see cref="FileMetadata.Exists"/> and therefore reflects the value computed
    /// when the <see cref="FileMetadata"/> instance was created.
    /// </remarks>
    public bool Exists => FileMetadata.Exists;

    /// <summary>
    /// Gets a value indicating whether this media file represents a photo.
    /// </summary>
    /// <remarks>
    /// Equivalent to checking <c>MediaFileType == MediaFileType.Photo</c>.
    /// </remarks>
    public bool IsPhoto => MediaFileType == MediaFileType.Photo;

    /// <summary>
    /// Gets a value indicating whether this media file represents a video.
    /// </summary>
    /// <remarks>
    /// Equivalent to checking <c>MediaFileType == MediaFileType.Video</c>.
    /// </remarks>
    public bool IsVideo => MediaFileType == MediaFileType.Video;

    /// <summary>
    /// Gets a value indicating whether the media file type is supported (photo or video).
    /// </summary>
    /// <remarks>
    /// Returns <see langword="true"/> when the instance represents either a photo or a video; otherwise <see langword="false"/>.
    /// </remarks>
    public bool IsSupportedMediaType => IsPhoto || IsVideo;
}
