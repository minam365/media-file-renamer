namespace Inamsoft.MediaFileRenamer.Services.FileRenamers;

internal static class DiagnosticLoggingHelper
{
    public static void Log(TimestampResult result, string message)
    {
        result.Diagnostics.Add(message);
    }
}

