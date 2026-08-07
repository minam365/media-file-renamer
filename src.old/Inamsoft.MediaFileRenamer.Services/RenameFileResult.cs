namespace Inamsoft.MediaFileRenamer.Services;

/// <summary>
/// A container used to hold information about a renamed file (target file).
/// </summary>
public record RenameFileResult
{
    /// <summary>
    /// Gets the source file info to be renamed.
    /// </summary>
    public required RenameFileRequest SourceFile { get; set; }

    /// <summary>
    /// Gets the name of the target file.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the full path of the target file.
    /// </summary>
    public required string FullName { get; init; }


}
