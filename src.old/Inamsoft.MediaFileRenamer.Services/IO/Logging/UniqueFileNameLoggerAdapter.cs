using Microsoft.Extensions.Logging;

namespace Inamsoft.MediaFileRenamer.Services.IO.Logging;

public sealed class UniqueFileNameLoggerAdapter : IUniqueFileNameLogger
{
    private readonly ILogger _logger;

    public UniqueFileNameLoggerAdapter(ILogger logger)
    {
        _logger = logger;
    }

    public void Attempt(string candidatePath, int index)
        => _logger.LogDebug("Attempt {Index}: {Path}", index, candidatePath);

    public void Success(string finalPath, int index)
        => _logger.LogInformation("Unique filename created after {Index} attempts: {Path}", index, finalPath);

    public void Failure(string candidatePath, int index, Exception exception)
        => _logger.LogDebug(exception, "Failed attempt {Index}: {Path}", index, candidatePath);
}
