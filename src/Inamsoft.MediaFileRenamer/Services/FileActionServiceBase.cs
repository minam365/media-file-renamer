using Inamsoft.Libs.MediaFileRenaming;
using Inamsoft.Libs.MetadataProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inamsoft.MediaFileRenamer.Services;

internal abstract class BaseFileActionService
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
    protected BaseFileActionService()
    {
        IHost host = Host.CreateDefaultBuilder()
                        .ConfigureServices((context, services) =>
                        {
                            services.TryAddTransient<IPhotoFileMetadataProvider, PhotoFileMetadataProvider>();
                            services.TryAddTransient<IFileMetadataProvider, FileMetadataProvider>();
                            services.TryAddTransient<IVideoFileMetadataProvider, VideoFileMetadataProvider>();
                            services.TryAddTransient<IFileNamingService, FileNamingService>();
                        })
                        .Build();

        _serviceProvider = host.Services;

    }


    protected static FileInfo[] GetSourceMediaFiles(string sourceFolderPath, string sourceFilePattern, bool recursive, int minFileSizeInBytes)
    {
        var sourceDirectoryInfo = new DirectoryInfo(sourceFolderPath);
        if (!sourceDirectoryInfo.Exists)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]✗ Source folder does not exist: {sourceFolderPath}[/]");
            throw new DirectoryNotFoundException($"Source folder does not exist: {sourceFolderPath}");
        }
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        
        var mediaFiles = sourceDirectoryInfo.GetFiles(sourceFilePattern, searchOption)
                                            .Where(fi => fi.Exists && Inamsoft.Libs.MediaFileRenaming.FileNamingService.IsSupportedMediaFileExtension(fi.Extension))
                                            .Where(fi => fi.Length > minFileSizeInBytes)
                                            .OrderBy(fi => fi.DirectoryName)
                                            .ToArray();
        return mediaFiles;
    }

    protected static ProgressColumn[] CreateProgressColumns()
    {
        ProgressColumn[] columns =
        [
            new SpinnerColumn                   // Spinner
            {
                Spinner = Spinner.Known.Default
            },
            new TaskDescriptionColumn(),        // Task description
            new ProgressBarColumn               // Progress bar
            {
                CompletedStyle = new Style(Color.Green),
                FinishedStyle = new Style(Color.Green),
                RemainingStyle = new Style(Color.White)
            },
            new PercentageColumn(),             // Percentage
            new ElapsedTimeColumn(),            // Elapsed time
            new RemainingTimeColumn() // Remaining time
        ];
        return columns;
    }

}
