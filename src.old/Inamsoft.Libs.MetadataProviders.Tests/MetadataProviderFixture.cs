using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Inamsoft.Libs.MetadataProviders.Tests;

public class MetadataProviderFixture : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    
    public MetadataProviderFixture()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.TryAddSingleton<IPhotoFileMetadataProvider, PhotoFileMetadataProvider>();
                services.TryAddSingleton<IFileMetadataProvider, FileMetadataProvider>();
                services.TryAddSingleton<IVideoFileMetadataProvider, VideoFileMetadataProvider>();
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

    ~MetadataProviderFixture()
    {
        Dispose(false);
    }
}