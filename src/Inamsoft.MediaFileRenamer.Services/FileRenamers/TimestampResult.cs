using System;
using System.Collections.Generic;
using System.Text;

namespace Inamsoft.MediaFileRenamer.Services.FileRenamers;

public enum TimestampSource
{
    None,
    FileName,
    PhotoMetadata,
    VideoMetadata,
    AvchdSidecar,
    XmpSidecar,
    ThmSidecar,
    LrvSidecar,
    SrtSidecar,
    FileSystemModifiedDate,
    PngMetadata,
    WmvMetadata,
    PsdMetadata
}

public readonly record struct TimestampResult()
{
    public required string OriginalName { get; init; }
    public required DateTime ResultingTimestamp { get; init; }
    public TimestampSource Source { get; init; } = TimestampSource.None;
    public required GetFileTimestampResponse Response { get; init; }
}
