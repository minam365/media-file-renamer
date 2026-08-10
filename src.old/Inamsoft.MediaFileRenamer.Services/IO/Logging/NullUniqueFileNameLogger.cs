namespace Inamsoft.MediaFileRenamer.Services.IO.Logging;

public sealed class NullUniqueFileNameLogger : IUniqueFileNameLogger
{
    public static readonly IUniqueFileNameLogger Instance = new NullUniqueFileNameLogger();
    private NullUniqueFileNameLogger() { }

    public void Attempt(string candidatePath, int index) { }
    public void Success(string finalPath, int index) { }
    public void Failure(string candidatePath, int index, Exception exception) { }
}
