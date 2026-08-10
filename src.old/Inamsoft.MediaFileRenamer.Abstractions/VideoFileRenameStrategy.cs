using System;
using System.Collections.Generic;
using System.Text;

namespace Inamsoft.MediaFileRenamer.Abstractions;

public enum VideoFileRenameStrategy
{
    None,                   // Keep original filename
    UseOriginalName,        // Explicitly preserve the original name

    Title,                  // Use the video's title/metadata title
    TitleWithYear,          // Title (Year)
    TitleWithResolution,    // Title [1080p], etc.
    TitleWithMetadata,      // Title + multiple metadata fields

    Timestamp,              // Use file creation/modification timestamp
    RecordingDate,          // Use embedded recording date (if available)

    Sequential,             // video_001, video_002, ...
    Hash,                   // Hash-based naming (MD5/SHA1/etc.)
    Guid,                   // Random GUID-based naming

    CustomPattern           // User-defined pattern, e.g. "{title}_{date}_{resolution}"
}