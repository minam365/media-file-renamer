namespace Inamsoft.Libs.MediaFileRenaming;

/// <summary>
/// A container for information about a source file to be renamed.
/// </summary>
public record RenameFileRequest
{
    public RenameFileRequest(string fullName)
    {
        FullName = fullName;
        Name = Path.GetFileName(fullName);
        NameWithoutExtension = Path.GetFileNameWithoutExtension(fullName);
        Extension = Path.GetExtension(fullName);
    }
    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the name of the file without the extension.
    /// </summary>
    public string NameWithoutExtension { get; init; }
    
    /// <summary>
    /// Gets the extension part of the file name, including the leading dot . even if it is the entire file name, or an empty string if no extension is present.
    /// </summary>
    public string Extension { get; init; }

    /// <summary>
    /// Gets the full path of the file.
    /// </summary>
    public string FullName { get; init; }

    public DateTimeOffset? LastModified { get; init; }

    /// <summary>
    /// Gets or sets the creation time of the current file.
    /// </summary>
    public DateTimeOffset? CreatedOn { get; init; }
}