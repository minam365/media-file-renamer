namespace Inamsoft.Libs.MediaFileRenaming;

public record RenameFileSettings
{
    /// <summary>
    /// The target folder path where the renamed files will be placed.
    /// </summary>
    public required string TargetFolderPath { get; set; }

    /// <summary>
    /// Ensures that the generated file names are unique within the target folder.
    /// </summary>
    public bool EnsureUniqueFileNames { get; set; } = true;

    /// <summary>
    /// The optional prefix to include in the generated file names.
    /// </summary>
    public string? TargetFileNamePrefix { get; set; } = default;
}
