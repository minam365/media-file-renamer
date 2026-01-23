using Inamsoft.Libs.MetadataProviders.Abstractions;
using Inamsoft.Libs.MetadataProviders.Extensions;
using MetadataExtractor;
using Microsoft.Extensions.Logging;

namespace Inamsoft.Libs.MetadataProviders;

public abstract class BaseMetadataProvider<TMetadataProvider>
{
    public ILogger<TMetadataProvider> Logger { get; }

    protected BaseMetadataProvider(ILogger<TMetadataProvider> logger)
    {
        Logger = logger;
    }


    protected bool TryReadMetadata(string filePath, out GetMetadataResult result)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(filePath);
            result = new GetMetadataResult(directories);
            return true;
        }
        catch (Exception ex)
        {
            //Logger.LogError(ex, "Failed to read metadata tags from file: {FilePath}", filePath);
            result = new GetMetadataResult();
            return false;
        }
    }

    public readonly record struct GetMetadataResult
    {
        static readonly IReadOnlyList<MetadataTag> EmptyMetadataTags = [];
        static readonly Dictionary<string, IReadOnlyList<MetadataTag>> EmptyDirectoryToTagsMap = [];

        public GetMetadataResult()
        {
            Directories = [];
        }

        public GetMetadataResult(IReadOnlyList<MetadataExtractor.Directory>? directories) : this()
        {
            Directories = directories ?? [];
        }

        public IReadOnlyList<MetadataExtractor.Directory>? Directories { get; }

        public IReadOnlyList<MetadataTag> MetadataTags 
            => Directories is not null ? Directories.ToTagList() : EmptyMetadataTags;

        public IReadOnlyDictionary<string, IReadOnlyList<MetadataTag>> DirectoryToTagsMap 
            => Directories is not null ? Directories.ToTagDictionary() : EmptyDirectoryToTagsMap;
    }
}