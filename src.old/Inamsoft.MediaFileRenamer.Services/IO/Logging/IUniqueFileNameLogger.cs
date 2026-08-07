namespace Inamsoft.MediaFileRenamer.Services.IO.Logging;

public interface IUniqueFileNameLogger
{
    void Attempt(string candidatePath, int index);
    void Success(string finalPath, int index);
    void Failure(string candidatePath, int index, Exception exception);
}
