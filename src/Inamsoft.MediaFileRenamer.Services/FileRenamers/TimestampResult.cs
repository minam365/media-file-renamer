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

public sealed class TimestampResult
{
    public string OriginalName { get; init; } = "";
    public DateTime? ResultingTimestamp { get; init; }
    public TimestampSource Source { get; init; }
    public List<string> Diagnostics { get; } = [];
}
