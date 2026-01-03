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
            var result = mediaFileHelper.RichCopyFiles(
                settings.SourceFolderPath,
                settings.TargetFolderPath,
                settings.SourceFilePattern,
                settings.MakeUniqueNames);
            AnsiConsole.MarkupLineInterpolated($"[green]✔ Completed moving files. Succeeded: {result.SucceededCount}, Failed: {result.FailedCount}[/]");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]✗ Error occurred while moving files: {ex.Message}[/]");
            return -1;
        }
    }
}
