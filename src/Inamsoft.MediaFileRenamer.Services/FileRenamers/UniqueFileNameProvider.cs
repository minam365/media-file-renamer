using Inamsoft.MediaFileRenamer.Services.IO.Logging;

namespace Inamsoft.MediaFileRenamer.Services.FileRenamers;

public static class UniqueFileNameProvider
{

    public static string Get(string folder, string fileName)
        => Get(folder, fileName, DefaultNameStrategy);

    public static string Get(
        string folder,
        string fileName,
        Func<string, string, int, string> namingStrategy)
    {
        string baseName = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);

        for (int i = 0; i < 10_000; i++)
        {
            string candidateName = namingStrategy(baseName, extension, i);
            string candidatePath = Path.Combine(folder, candidateName);

            try
            {
                using var fs = new FileStream(candidatePath, FileMode.CreateNew);
                return candidatePath;
            }
            catch (IOException)
            {
                // File exists → try next
            }
        }

        throw new IOException("Unable to generate a unique filename.");
    }

    public static string Get(
        DirectoryInfo directory,
        string fileName,
        Func<string, string, int, string> namingStrategy,
        IUniqueFileNameLogger? logger = null)
    {
        logger ??= NullUniqueFileNameLogger.Instance;

        string baseName = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);

        for (int i = 0; i < 10_000; i++)
        {
            string candidateName = namingStrategy(baseName, extension, i);
            string candidatePath = Path.Combine(directory.FullName, candidateName);

            logger.Attempt(candidatePath, i);

            try
            {
                using var fs = new FileStream(candidatePath, FileMode.CreateNew);
                logger.Success(candidatePath, i);
                return candidatePath;
            }
            catch (IOException ex)
            {
                logger.Failure(candidatePath, i, ex);
            }
        }

        throw new IOException("Unable to generate a unique filename.");
    }

    public static string Get(
        DirectoryInfo directory,
        string fileName,
        IUniqueFileNameLogger? logger = null)
        => Get(directory, fileName, DefaultNameStrategy, logger);




    public static async Task<string> GetAsync(string folder, string fileName)
    {
        return await Task.Run(() => Get(folder, fileName));
    }

    public static async Task<string> GetAsync(DirectoryInfo directory,
                                              string fileName,
                                              Func<string, string, int, string> namingStrategy,
                                              IUniqueFileNameLogger? logger = null,
                                              CancellationToken cancellationToken = default)
    {
        return await Task.Run(
            () => Get(directory, fileName, namingStrategy, logger),
            cancellationToken);
    }

    public static Task<string> GetAsync(DirectoryInfo directory,
                                        string fileName,
                                        IUniqueFileNameLogger? logger = null,
                                        CancellationToken cancellationToken = default)
    {
        return GetAsync(directory, fileName, DefaultNameStrategy, logger, cancellationToken);
    }

    public static string DefaultNameStrategy(string baseName, string extension, int index)
        => index == 0
            ? $"{baseName}{extension}"
            : $"{baseName} ({index}){extension}";

    public static string TimestampStrategy(string baseName, string extension, int index)
        => $"{baseName}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}{extension}";

    public static string GuidStrategy(string baseName, string extension, int index)
        => $"{baseName}_{Guid.NewGuid()}{extension}";

    public static string DashNumberStrategy(string baseName, string extension, int index)
        => index == 0
            ? $"{baseName}{extension}"
            : $"{baseName}-{index}{extension}";
}