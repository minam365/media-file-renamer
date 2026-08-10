namespace Inamsoft.MediaFileRenamer.Services.FileRenamers;

public interface IRenamingContext
{
    FileInfo File { get; }
    string OriginalName { get; }
    DateTime? ParsedTimestampFromName { get; }
}
