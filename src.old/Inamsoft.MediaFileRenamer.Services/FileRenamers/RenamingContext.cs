namespace Inamsoft.MediaFileRenamer.Services.FileRenamers;

public sealed class RenamingContext : IRenamingContext
{
    public FileInfo File { get; }
    public string OriginalName { get; }
    public DateTime? ParsedTimestampFromName { get; }

    public RenamingContext(FileInfo file, Func<string, DateTime?> timestampParser)
    {
        File = file;
        OriginalName = Path.GetFileNameWithoutExtension(file.Name);
        ParsedTimestampFromName = timestampParser(OriginalName);
    }
}
