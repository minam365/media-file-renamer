using Spectre.Console;
using Spectre.Console.Cli;

namespace Inamsoft.MediaFileRenamer.Commands;

internal class CopyFilesCommand : Command<FileActionSettings>
{
    public override int Execute(CommandContext context, FileActionSettings settings, CancellationToken cancellationToken)
    {
        var mediaFileHelper = new MediaFileHelper();
        try
        {
            FileRenameActionRequest request = new(settings.SourceFolderPath, settings.TargetFolderPath)
            {
                SourceFilePattern = settings.SourceFilePattern,
                Recursive = settings.Recursive,
                OverwriteExistingFiles = settings.Overwrite,
                FilePrefix = settings.FilePrefix
            };
            var result = mediaFileHelper.RichCopyFiles(request);

            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]✗ Error occurred while copying files: {ex.Message}[/]");
            AnsiConsole.WriteLine();
            return -1;
        }
    }
}
