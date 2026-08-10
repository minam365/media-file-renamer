using CommunityToolkit.Diagnostics;
using Inamsoft.MediaFileRenamer.Services.MetadataProviderServices.Extensions;
using Inamsoft.MediaFileRenamer.Services.MetadataProviderServices.Models;
using MetadataExtractor;
using Microsoft.Extensions.Logging;

namespace Inamsoft.MediaFileRenamer.Services.MetadataProviderServices;

public abstract class BaseMetadataProvider<TMetadataProvider, TMetadata>(ILogger<TMetadataProvider> logger)
    : IMetadataProvider<TMetadata>
{
    protected ILogger<TMetadataProvider> Logger { get; } = logger;


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

    protected bool TryExtractMetadata(FileInfo? fileInfo, out ExtractMetadataResult result)
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

    public readonly record struct ExtractMetadataResult()
    {
        static readonly IReadOnlyList<MetadataTag> EmptyMetadataTags = [];
        static readonly Dictionary<string, IReadOnlyList<MetadataTag>> EmptyDirectoryToTagsMap = [];

        public ExtractMetadataResult(IReadOnlyList<MetadataExtractor.Directory>? directories) : this()
        {
            Directories = directories ?? [];
        }

        public IReadOnlyList<MetadataExtractor.Directory>? Directories { get; } = [];

        public IReadOnlyList<MetadataTag> MetadataTags
            => Directories is not null ? Directories.ToTagList() : EmptyMetadataTags;

        public IReadOnlyDictionary<string, IReadOnlyList<MetadataTag>> DirectoryToTagsMap
            => Directories is not null ? Directories.ToTagDictionary() : EmptyDirectoryToTagsMap;
    }
}