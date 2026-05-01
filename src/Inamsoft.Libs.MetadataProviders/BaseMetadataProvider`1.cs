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


    protected bool TryExtractMetadata(string filePath, out ExtractMetadataResult result)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(filePath);
            result = new ExtractMetadataResult(directories);
            return true;
        }
        catch (Exception /* ex */)
        {
            //Logger.LogError(ex, "Failed to read metadata tags from file: {FilePath}", filePath);
            result = new ExtractMetadataResult();
            return false;
        }
    }

    public readonly record struct ExtractMetadataResult
    {
        static readonly IReadOnlyList<MetadataTag> EmptyMetadataTags = [];
        static readonly Dictionary<string, IReadOnlyList<MetadataTag>> EmptyDirectoryToTagsMap = [];

        public ExtractMetadataResult()
        {
            Directories = [];
        }

        public ExtractMetadataResult(IReadOnlyList<MetadataExtractor.Directory>? directories) : this()
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