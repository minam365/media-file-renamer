namespace Inamsoft.MediaFileRenamer.Services.FileRenamers;

public readonly record struct GetFileTimestampResponse(DateTime Timestamp, TimestampSource Source, string StatusMessage);
