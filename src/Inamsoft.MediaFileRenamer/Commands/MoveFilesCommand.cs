using Spectre.Console;
using Spectre.Console.Cli;

namespace Inamsoft.MediaFileRenamer.Commands;

internal class MoveFilesCommand : Command<FileActionSettings>
{
    public override int Execute(CommandContext context, FileActionSettings settings, CancellationToken cancellationToken)
    {
        var mediaFileHelper = new MediaFileHelper();
        try
        {
            var result = mediaFileHelper.RichMoveFiles(
                settings.SourceFolderPath,
                settings.TargetFolderPath,
                settings.SourceFilePattern,
                settings.Overwrite,
                settings.Recursive);

            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]✗ Error occurred while moving files: {ex.Message}[/]");
            AnsiConsole.WriteLine();
            return -1;
        }
    }
}
