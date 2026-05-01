using CommunityToolkit.Diagnostics;
using Inamsoft.Libs.MetadataProviders.Abstractions;
using Inamsoft.Libs.MetadataProviders.Extensions;
using MetadataExtractor;
using Microsoft.Extensions.Logging;

namespace Inamsoft.Libs.MetadataProviders;

public abstract class BaseMetadataProvider<TMetadataProvider, TMetadata> : IMetadataProvider<TMetadata>
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

    protected bool TryExtractMetadata(FileInfo fileInfo, out ExtractMetadataResult result)
    {
        if (fileInfo is null)
        {
            result = new ExtractMetadataResult();
            return false;
        }

        var parsed = TryExtractMetadata(fileInfo.FullName, out result);
        return parsed;
    }

    /// <inheritdoc />
    public TMetadata ReadMetadata(string filePath)
    {
        Guard.IsNotNullOrEmpty(filePath, nameof(filePath));

        FileInfo fileInfo = new(filePath);

        return ReadMetadata(fileInfo);
    }

    /// <inheritdoc />
    public TMetadata ReadMetadata(FileInfo fileInfo)
    {
        Guard.IsNotNull(fileInfo, nameof(fileInfo));

        if (Logger.IsEnabled(LogLevel.Debug))
            Logger.LogDebug("Getting metadata info from the file: {Path}", fileInfo.FullName);

        TMetadata metadata = InternalReadMetadata(fileInfo);

        if(Logger.IsEnabled(LogLevel.Debug))
            Logger.LogDebug("Metadata info successfully extracted from the file: {Path}", fileInfo.FullName);

        return metadata;

    }

    protected abstract TMetadata InternalReadMetadata(FileInfo fileInfo);

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