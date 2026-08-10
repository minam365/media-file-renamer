namespace Inamsoft.MediaFileRenamer.Services.FileRenamers;

public readonly record struct GetFileTimestampRequest(FileInfo File)
{
    public string FileNameWithoutExtension { get; init; } = Path.GetFileNameWithoutExtension(File.Name);
}