using Spectre.Console.Cli;
using System.ComponentModel;

namespace Inamsoft.MediaFileRenamer.Commands;

internal class FileActionSettings : CommandSettings
{
    [CommandArgument(0, "<source-folder-path>")]
    [Description("The path of the source folder containing files to copy.")]
    public required string SourceFolderPath { get; init; } = string.Empty;

    [CommandArgument(1, "<target-folder-path>")]
    [Description("The path of the target folder where files will be copied.")]
    public required string TargetFolderPath { get; init; } = string.Empty;

    [CommandOption("-p|--source-file-pattern")]
    [Description("The file pattern to match source files (e.g., '*.*', '*.jpg', '*.mov', '*.mp4').")]
    [DefaultValue("*.*")]
    public string SourceFilePattern { get; set; } = "*.*";

    [CommandOption("-o|--overwrite")]
    [Description("If set to false, ensures that copied/moved files have unique names in the target folder to avoid overwriting.")]
    [DefaultValue(false)]
    public bool Overwrite { get; set; } = false;

    [CommandOption("-r|--recursive")]
    [Description("If set to true, processes files in all subdirectories of the source folder recursively.")]
    [DefaultValue(false)]
    public bool Recursive { get; set; } = false;
    
    [CommandOption("-f|--file-prefix")]
    [Description("A prefix to be added to the beginning of the target file name.")]
    [DefaultValue("")]
    public string? FilePrefix { get; set; } = string.Empty;

    [CommandOption("-m|--min-file-size-in-bytes")]
    [Description("Minimum file size in bytes to consider for processing.")]
    [DefaultValue(2048)]
    public int MinFileSizeInBytes { get; set; } = 2048;

}
