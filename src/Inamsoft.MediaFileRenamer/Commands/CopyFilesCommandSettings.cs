using Spectre.Console.Cli;
using System.ComponentModel;

namespace Inamsoft.MediaFileRenamer.Commands;

internal class CopyFilesCommandSettings : CommandSettings
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
    
    [CommandOption("-u|--make-unique-names")]
    [Description("If set to true, ensures that copied files have unique names in the target folder to avoid overwriting.")]
    [DefaultValue(true)]
    public bool MakeUniqueNames { get; set; } = true;
}