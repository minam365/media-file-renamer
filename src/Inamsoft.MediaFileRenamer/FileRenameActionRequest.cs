namespace Inamsoft.MediaFileRenamer;

public record FileRenameActionRequest(
    string SourceFolderPath,
    string TargetFolderPath)
{
    public string SourceFilePattern { get; init; } = "*.jpg";
    
    public bool Recursive { get; init; } = false;
    
    public bool OverwriteExistingFiles { get; init; } = false;
    
    public string? FilePrefix { get; init; } = string.Empty;
}