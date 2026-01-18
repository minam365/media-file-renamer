using System;
using Inamsoft.Libs.MetadataProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Inamsoft.Libs.MediaFileRenaming.Tests;

public class MediaFileRenamingFixture : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    
    public MediaFileRenamingFixture()
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
    
    public IServiceProvider ServiceProvider => _serviceProvider;
    
    public IFileMetadataProvider FileMetadataProvider => 
        _serviceProvider.GetRequiredService<IFileMetadataProvider>();
    
    public IPhotoFileMetadataProvider PhotoFileMetadataProvider => 
        _serviceProvider.GetRequiredService<IPhotoFileMetadataProvider>();
    
    public IVideoFileMetadataProvider VideoFileMetadataProvider => 
        _serviceProvider.GetRequiredService<IVideoFileMetadataProvider>();

    public IFileNamingService FileNamingService => 
        _serviceProvider.GetRequiredService<IFileNamingService>();

    private void ReleaseUnmanagedResources()
    {
        // TODO release unmanaged resources here
    }

    protected virtual void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            // TODO release managed resources here
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~MediaFileRenamingFixture()
    {
        Dispose(false);
    }
}