using System;
using System.Collections.Generic;
using System.Text;

namespace Inamsoft.MediaFileRenamer.Services.IO.Logging;

public sealed class ConsoleLogger : IUniqueFileNameLogger
{
    public void Attempt(string candidatePath, int index)
        => Console.WriteLine($"Trying {candidatePath}");

    public void Success(string finalPath, int index)
        => Console.WriteLine($"Success: {finalPath}");

    public void Failure(string candidatePath, int index, Exception ex)
        => Console.WriteLine($"Failed: {candidatePath} ({ex.Message})");
}


