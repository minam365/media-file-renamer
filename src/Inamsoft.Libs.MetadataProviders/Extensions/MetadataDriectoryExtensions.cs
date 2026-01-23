using Inamsoft.Libs.MetadataProviders.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inamsoft.Libs.MetadataProviders.Extensions;

public static class MetadataDriectoryExtensions
{
    /// <summary>
    /// Converts a collection of metadata directories into a read-only dictionary mapping directory names to their
    /// associated metadata tags.
    /// </summary>
    /// <remarks>The returned dictionary provides a convenient way to access tags grouped by their directory
    /// names. The order of directories and tags is preserved as in the input collection. This method does not modify
    /// the input collection.</remarks>
    /// <param name="directories">The collection of metadata directories to convert. Each directory should contain one or more tags to be included
    /// in the resulting dictionary.</param>
    /// <returns>A read-only dictionary where each key is a directory name and each value is a read-only list of metadata tags
    /// found in that directory. If a directory contains no tags, its value will be an empty list.</returns>
    public static IReadOnlyDictionary<string, IReadOnlyList<MetadataTag>> ToTagDictionary(this IEnumerable<MetadataExtractor.Directory> directories)
    {
        var dict = new Dictionary<string, IReadOnlyList<MetadataTag>>();
        foreach (var directory in directories)
        {
            var tags = new List<MetadataTag>();
            foreach (var tag in directory.Tags)
            {
                tags.Add(new MetadataTag
                {
                    Type = tag.Type,
                    Name = tag.Name,
                    Value = tag.Description,
                    DirectoryName = tag.DirectoryName
                });
            }
            dict[directory.Name] = tags;
        }
        return dict;
    }

    /// <summary>
    /// Creates a read-only list of metadata tags extracted from the specified collection of directories.
    /// </summary>
    /// <remarks>Each <see cref="MetadataTag"/> in the returned list represents a tag from one of the input
    /// directories, including its type, name, value, and directory name. The order of tags in the list reflects the
    /// order in which they appear in the input directories.</remarks>
    /// <param name="directories">The collection of metadata directories from which to extract tags. Cannot be null.</param>
    /// <returns>An <see cref="IReadOnlyList{MetadataTag}"/> containing all tags found in the provided directories. The list will
    /// be empty if no tags are present.</returns>
    public static IReadOnlyList<MetadataTag> ToTagList(this IEnumerable<MetadataExtractor.Directory> directories)
    {
        var tags = new List<MetadataTag>();
        foreach (var directory in directories)
        {
            foreach (var tag in directory.Tags)
            {
                tags.Add(new MetadataTag
                {
                    Type = tag.Type,
                    Name = tag.Name,
                    Value = tag.Description,
                    DirectoryName = tag.DirectoryName
                });
            }
        }
        return tags;
    }

}
