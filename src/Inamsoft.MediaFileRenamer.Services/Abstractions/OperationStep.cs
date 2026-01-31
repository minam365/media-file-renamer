namespace Inamsoft.MediaFileRenamer.Services.Abstractions;

public enum OperationStep
{
    Begin,
    Finished,
    Skipped,
    Retrying,
    Failed
}
