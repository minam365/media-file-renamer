using Inamsoft.MediaFileRenamer.Services.IO.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inamsoft.MediaFileRenamer.Services.IO;

public static class DirectoryInfoExtensions
{
    public static string GetUniqueFilePath(
        this DirectoryInfo directory,
        string fileName,
        Func<string, string, int, string>? namingStrategy = null,
        IUniqueFileNameLogger? logger = null)
    {
        namingStrategy ??= UniqueFileNameProvider.DefaultNameStrategy;
        return UniqueFileNameProvider.Get(directory, fileName, namingStrategy, logger);
    }

    public static Task<string> GetUniqueFilePathAsync(this DirectoryInfo directory,
                                                      string fileName,
                                                      Func<string, string, int, string>? namingStrategy = null,
                                                      IUniqueFileNameLogger? logger = null,
                                                      CancellationToken cancellationToken = default)
    {
        namingStrategy ??= UniqueFileNameProvider.DefaultNameStrategy;
        return UniqueFileNameProvider.GetAsync(directory, fileName, namingStrategy, logger, cancellationToken);
    }

}
