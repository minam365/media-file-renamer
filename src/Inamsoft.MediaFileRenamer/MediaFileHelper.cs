using Inamsoft.Libs.MediaFileRenaming;
using Inamsoft.Libs.MetadataProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace Inamsoft.MediaFileRenamer;


internal class MediaFileHelper
{
    private readonly IServiceProvider _serviceProvider;

    public IServiceProvider ServiceProvider => _serviceProvider;

    public IFileMetadataProvider FileMetadataProvider =>
        _serviceProvider.GetRequiredService<IFileMetadataProvider>();

    public IPhotoFileMetadataProvider PhotoFileMetadataProvider =>
        _serviceProvider.GetRequiredService<IPhotoFileMetadataProvider>();

    public IVideoFileMetadataProvider VideoFileMetadataProvider =>
        _serviceProvider.GetRequiredService<IVideoFileMetadataProvider>();

    public IFileNamingService FileNamingService =>
        _serviceProvider.GetRequiredService<IFileNamingService>();
    public MediaFileHelper()
    {
        IHost host = Host.CreateDefaultBuilder()
                        .ConfigureServices((context, services) =>
                        {
                            services.TryAddSingleton<IPhotoFileMetadataProvider, PhotoFileMetadataProvider>();
                            services.TryAddSingleton<IFileMetadataProvider, FileMetadataProvider>();
                            services.TryAddSingleton<IVideoFileMetadataProvider, VideoFileMetadataProvider>();
                            services.TryAddSingleton<IFileNamingService, FileNamingService>();
                        })
                        .Build();

        _serviceProvider = host.Services;

    }

    public FileActionResult RichCopyFiles(string sourceFolderPath, string targetFolderPath, string sourceFilePattern = "*.jpg", bool overwrite = false, bool recursive = false)
    {

        int succeededCount = 0;
        int failedCount = 0;
        var sourceDirectoryInfo = new DirectoryInfo(sourceFolderPath);
        if (!sourceDirectoryInfo.Exists)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]✗ Source folder does not exist: {sourceFolderPath}[/]");
            throw new DirectoryNotFoundException($"Source folder does not exist: {sourceFolderPath}");
        }
        SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var mediaFiles = sourceDirectoryInfo.GetFiles(sourceFilePattern, searchOption);

        AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(new ProgressColumn[]
            {
                new SpinnerColumn               // Spinner
                {
                    Spinner = Spinner.Known.Default
                },
                new TaskDescriptionColumn(),    // Task description
                new ProgressBarColumn           // Progress bar
                {
                    CompletedStyle = new Style(Color.Green),
                    FinishedStyle = new Style(Color.Lime),
                    RemainingStyle = new Style(Color.Grey)
                },
                new PercentageColumn(),         // Percentage
                new ElapsedTimeColumn(),        // Elapsed time
                new RemainingTimeColumn(),      // Remaining time
            })
            .Start((Action<ProgressContext>)(ctx =>
            {
                var copyTask = ctx.AddTask("Copying files...", maxValue: mediaFiles.Length);

                while (!copyTask.IsFinished)
                {
                    var fileCounter = 1;
                    foreach (var mediaFile in mediaFiles)
                    {
                        try
                        {
                            AnsiConsole.WriteLine();
                            AnsiConsole.MarkupLineInterpolated($"[dim yellow]→ Copying {fileCounter} of {mediaFiles.Length}:[/] {mediaFile.Name} [dim yellow]to[/] {targetFolderPath}");

                            string targetFilePath = overwrite
                                ? FileNamingService.GetTargetFilePath(mediaFile.FullName, targetFolderPath)
                                : FileNamingService.MakeUniqueTargetFilePath(mediaFile.FullName, targetFolderPath);
                            var targetDirectory = Path.GetDirectoryName(targetFilePath);
                            if (!Directory.Exists(targetDirectory))
                            {
                                Directory.CreateDirectory(targetDirectory!);
                            }

                            File.Copy(mediaFile.FullName, targetFilePath, overwrite);
                            succeededCount++;

                            AnsiConsole.MarkupLineInterpolated($"[bold green]✓ Copied  {fileCounter} of {mediaFiles.Length}:[/] {mediaFile.Name} [green]to[/] {targetFilePath}");
                        }
                        catch (Exception ex)
                        {
                            //AnsiConsole.WriteException(ex);
                            AnsiConsole.MarkupLineInterpolated($"[red]✗ Failed to copy {fileCounter} of {mediaFiles.Length}:[/] {mediaFile.Name} [yellow]to[/] {targetFolderPath} [red]Error:[/] {ex.Message}");
                            failedCount++;
                        }
                        copyTask.Increment(1);
                        fileCounter++;
                    }
                }
            }));

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLineInterpolated($"[yellow]Completed copying files[/]. [green]✓ Succeeded:[/] {succeededCount}, [red]✗ Failed:[/] {failedCount}");
        AnsiConsole.WriteLine();

        return new FileActionResult(succeededCount, failedCount);
    }

    public FileActionResult RichMoveFiles(string sourceFolderPath, string targetFolderPath, string sourceFilePattern = "*.jpg", bool overwrite = false, bool recursive = false)
    {
        int succeededCount = 0;
        int failedCount = 0;
        var sourceDirectoryInfo = new DirectoryInfo(sourceFolderPath);
        if (!sourceDirectoryInfo.Exists)
        {
            throw new DirectoryNotFoundException($"Source folder does not exist: {sourceFolderPath}");
        }

        SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var mediaFiles = sourceDirectoryInfo.GetFiles(sourceFilePattern, searchOption);

        AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(new ProgressColumn[]
            {
                        new SpinnerColumn               // Spinner
                        {
                            Spinner = Spinner.Known.Default
                        },
                        new TaskDescriptionColumn(),    // Task description
                        new ProgressBarColumn           // Progress bar
                        {
                            CompletedStyle = new Style(Color.Green),
                            FinishedStyle = new Style(Color.Lime),
                            RemainingStyle = new Style(Color.Grey)
                        },
                        new PercentageColumn(),         // Percentage
                        new ElapsedTimeColumn(),        // Elapsed time
                        new RemainingTimeColumn(),      // Remaining time
            })
            .Start(ctx =>
            {
                var copyTask = ctx.AddTask("Moving files...", maxValue: mediaFiles.Length);

                while (!copyTask.IsFinished)
                {
                    var fileCounter = 1;

                    foreach (var mediaFile in mediaFiles)
                    {
                        try
                        {
                            AnsiConsole.WriteLine();
                            AnsiConsole.MarkupLineInterpolated($"[dim yellow]→ Moving {fileCounter} of {mediaFiles.Length}:[/] {mediaFile.Name} [dim yellow]to[/] {targetFolderPath}");

                            string targetFilePath = overwrite
                                ? FileNamingService.GetTargetFilePath(mediaFile.FullName, targetFolderPath)
                                : FileNamingService.MakeUniqueTargetFilePath(mediaFile.FullName, targetFolderPath);
                            var targetDirectory = Path.GetDirectoryName(targetFilePath);
                            if (!Directory.Exists(targetDirectory))
                            {
                                Directory.CreateDirectory(targetDirectory!);
                            }
                            File.Move(mediaFile.FullName, targetFilePath, overwrite);
                            succeededCount++;
                            AnsiConsole.MarkupLineInterpolated($"[bold green]✓ Moved  {fileCounter} of {mediaFiles.Length}:[/] {mediaFile.Name} [green]to[/] {targetFilePath}");
                        }
                        catch (Exception ex)
                        {
                            //AnsiConsole.WriteException(ex);
                            AnsiConsole.MarkupLineInterpolated($"[red]✗ Failed to move {fileCounter} of {mediaFiles.Length}:[/] {mediaFile.Name} [yellow]to[/] {targetFolderPath}. [red]Error:[/] {ex.Message}");
                            failedCount++;
                        }
                        copyTask.Increment(1);
                        fileCounter++;
                    }
                }
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLineInterpolated($"[yellow]Completed moving files[/]. [green]✓ Succeeded:[/] {succeededCount}, [red]✗ Failed:[/] {failedCount}");
        AnsiConsole.WriteLine();

        return new FileActionResult(succeededCount, failedCount);
    }



    public FileActionResult CopyFiles(string sourceFolderPath, string targetFolderPath, string sourceFilePattern = "*.jpg", bool makeUniqueNames = true)
    {
        int succeededCount = 0;
        int failedCount = 0;
        var sourceDirectoryInfo = new DirectoryInfo(sourceFolderPath);
        if (!sourceDirectoryInfo.Exists)
        {
            throw new DirectoryNotFoundException($"Source folder does not exist: {sourceFolderPath}");
        }
        var mediaFiles = sourceDirectoryInfo.GetFiles(sourceFilePattern, SearchOption.TopDirectoryOnly);
        foreach (var mediaFile in mediaFiles)
        {
            try
            {
                string targetFilePath = makeUniqueNames
                    ? FileNamingService.MakeUniqueTargetFilePath(mediaFile.FullName, targetFolderPath)
                    : FileNamingService.GetTargetFilePath(mediaFile.FullName, targetFolderPath);
                var targetDirectory = Path.GetDirectoryName(targetFilePath);
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory!);
                }
                File.Copy(mediaFile.FullName, targetFilePath);
                succeededCount++;
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                failedCount++;
            }
        }
        return new FileActionResult(succeededCount, failedCount);
    }

    public FileActionResult MoveFiles(string sourceFolderPath, string targetFolderPath, string sourceFilePattern = "*.jpg", bool makeUniqueNames = true)
    {
        int succeededCount = 0;
        int failedCount = 0;
        var sourceDirectoryInfo = new DirectoryInfo(sourceFolderPath);
        if (!sourceDirectoryInfo.Exists)
        {
            throw new DirectoryNotFoundException($"Source folder does not exist: {sourceFolderPath}");
        }
        var mediaFiles = sourceDirectoryInfo.GetFiles(sourceFilePattern, SearchOption.TopDirectoryOnly);
        foreach (var mediaFile in mediaFiles)
        {
            try
            {
                string targetFilePath = makeUniqueNames
                    ? FileNamingService.MakeUniqueTargetFilePath(mediaFile.FullName, targetFolderPath)
                    : FileNamingService.GetTargetFilePath(mediaFile.FullName, targetFolderPath);
                var targetDirectory = Path.GetDirectoryName(targetFilePath);
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory!);
                }
                File.Move(mediaFile.FullName, targetFilePath);
                succeededCount++;
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                failedCount++;
            }
        }
        return new FileActionResult(succeededCount, failedCount);
    }

}

internal readonly record struct FileActionResult(int SucceededCount, int FailedCount);
