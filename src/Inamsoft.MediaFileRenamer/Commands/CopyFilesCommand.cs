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
            var result = mediaFileHelper.RichCopyFiles(
                settings.SourceFolderPath,
                settings.TargetFolderPath,
                settings.SourceFilePattern,
                settings.MakeUniqueNames);
            AnsiConsole.MarkupLineInterpolated($"[green]✔ Completed copying files. Succeeded: {result.SucceededCount}, Failed: {result.FailedCount}[/]");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]✗ Error occurred while copying files: {ex.Message}[/]");
            return -1;
        }
    }
}
