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
                MinFileSizeInBytes = settings.MinFileSizeInBytes,
                SearchPattern = request.SourceFilePattern,
                Recursive = settings.Recursive,
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                ComputeSha256 = false,
                OnError = ex => AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}"),
                OnDirectoryEntered = dir => AnsiConsole.MarkupLine($"[blue]Entering directory:[/] {dir.FullName}"),
                OnFileFound = fileScanResult =>
                {
                    AnsiConsole.MarkupLine($"[green]Renaming and copying file:[/] {fileScanResult.Icon} {fileScanResult.File.FullName}");
                },
                ProcessFile = fileScanResult =>
                {
                    // Here you can implement the logic to rename and copy the file
                    var relativePath = Path.GetRelativePath(root.FullName, fileScanResult.File.FullName);
                    var targetFilePath = Path.Combine(request.TargetFolderPath, fileScanResult.File.Name);
                    // Ensure the target directory exists
                    var targetDirectory = Path.GetDirectoryName(targetFilePath);
                    if (targetDirectory != null && !Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }
                    // Copy the file
                    fileScanResult.File.CopyTo(targetFilePath, request.OverwriteExistingFiles);
                }
            };
            //var result = SpectreFileScan.ScanWithProgressAndTreeAsync(root, options, cancellationToken).GetAwaiter().GetResult();
            CleanFileScanner.RunDashboardAsync(root, options, cancellationToken).GetAwaiter().GetResult();

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
