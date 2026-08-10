namespace Inamsoft.MediaFileRenamer.Services.FileRenamers.Rules;

/// <summary>
/// 
/// </summary>
public enum VideoFileRenamingTimestampStrategy
{
    /// <summary>
    /// Keep original filename
    /// </summary>
    None,
    /// <summary>
    /// Use filename, or fallback to last modified date
    /// </summary>
    FileNameOrLastModifiedDate,
    /// <summary>
    /// Use last modified date, or fallback to filename
    /// </summary>
    LastModifiedDateOrFileName,
    

}
