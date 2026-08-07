using Inamsoft.MediaFileRenamer.Abstractions;
using Spectre.Console;

namespace Inamsoft.MediaFileRenamer.Services.IO.Logging;

public sealed class SpectreConsoleLogger : IUniqueFileNameLogger
{
    private readonly ConsoleLogLevel _level;

    public SpectreConsoleLogger(ConsoleLogLevel level = ConsoleLogLevel.Normal)
    {
        _level = level;
    }

    public void Attempt(string candidatePath, int index)
    {
        if (_level == ConsoleLogLevel.Verbose)
        {
            AnsiConsole.MarkupLine(
                $"[grey]Attempt {index}: {candidatePath.EscapeMarkup()}[/]");
        }
    }

    public void Success(string finalPath, int index)
    {
        if (_level != ConsoleLogLevel.Quiet)
        {
            AnsiConsole.MarkupLine(
                $"[green]✔ Unique filename created after {index} attempts[/] [blue]{finalPath.EscapeMarkup()}[/]");
        }
    }

    public void Failure(string candidatePath, int index, Exception exception)
    {
        if (_level == ConsoleLogLevel.Verbose)
        {
            AnsiConsole.MarkupLine(
                $"[yellow]✖ Failed attempt {index}[/] [grey]{candidatePath.EscapeMarkup()}[/] ([red]{exception.Message.EscapeMarkup()}[/])");
        }
    }
}
