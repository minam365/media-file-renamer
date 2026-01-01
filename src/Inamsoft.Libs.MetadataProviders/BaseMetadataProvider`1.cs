using CommunityToolkit.Diagnostics;
using Inamsoft.Libs.MetadataProviders.Abstractions;
using Microsoft.Extensions.Logging;

namespace Inamsoft.Libs.MetadataProviders;

public abstract class BaseMetadataProvider<TMetadataProvider>
{
    public ILogger<TMetadataProvider> Logger { get; }

    protected BaseMetadataProvider(ILogger<TMetadataProvider> logger)
    {
        Logger = logger;
    }


    protected IReadOnlyDictionary<string, IReadOnlyList<MetadataTag>> ReadMetadataTagsAsDirectoryDictionary(string filePath)
    {
        Dictionary<string, IReadOnlyList<MetadataTag>> metadataDict = [];

        try
        {
            var directories = MetadataExtractor.ImageMetadataReader.ReadMetadata(filePath);
            metadataDict = directories
                .SelectMany(d => d.Tags.Select(t => new MetadataTag
                {
                    Type = t.Type,
                    Name = t.Name,
                    Value = t.Description,
                    DirectoryName = d.Name
                }))
                .GroupBy(tag => tag.DirectoryName)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<MetadataTag>)g.ToList()
                );
        }
        catch (Exception)
        {

            throw;
        }

        return metadataDict;
    }


}