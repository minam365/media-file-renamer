namespace Inamsoft.MediaFileRenamer.Services.FileRenamers;

public readonly record struct FileRenamerActionResult(string OriginalFileName, string NewFileName);