namespace Inamsoft.MediaFileRenamer.Services.FileRenamers;

public readonly record struct TargetFolderContext(string BaseFolder, string SubFolderFormat = "{0:yyyy}\\{0:MM}. {0:MMM}");
