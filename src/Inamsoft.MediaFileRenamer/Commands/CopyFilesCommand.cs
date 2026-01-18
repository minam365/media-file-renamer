using Inamsoft.MediaFileRenamer.Services.FileSystemServices;
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
            //var result = mediaFileHelper.RichCopyFiles(request);

            var root = new DirectoryInfo(request.SourceFolderPath);

            var options = new FileScanOptions
            {
                MinFileSizeInBytes = 3072,
                SearchPattern = request.SourceFilePattern,
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                ComputeSha256 = true,
                OnError = ex => AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}")
            };
            var result = SpectreFileScan.ScanWithProgressAndTreeAsync(root, options, cancellationToken).GetAwaiter().GetResult();

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
