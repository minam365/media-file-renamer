using Inamsoft.MediaFileRenamer.Abstractions;

namespace Inamsoft.MediaFileRenamer.Services.IO.Logging;

public static class SpectreConsoleLoggerPresets
{
    public static IUniqueFileNameLogger Quiet => new SpectreConsoleLogger(ConsoleLogLevel.Quiet);
    public static IUniqueFileNameLogger Normal => new SpectreConsoleLogger(ConsoleLogLevel.Normal);
    public static IUniqueFileNameLogger Verbose => new SpectreConsoleLogger(ConsoleLogLevel.Verbose);
}
