using Inamsoft.Libs.MetadataProviders.Abstractions;
using Inamsoft.MediaFileRenamer.Abstractions;
using static Inamsoft.MediaFileRenamer.MediaFileHelper;

namespace Inamsoft.MediaFileRenamer.Logging;

public static class OperationMessageFactory
{
    private static readonly IReadOnlyDictionary<FileOperationType, string> Templates =
        new Dictionary<FileOperationType, string>
        {
            [FileOperationType.List] = "listing files",
            [FileOperationType.Copy] = "copying file {fileName}",
            [FileOperationType.Move] = "moving file {fileName}",
            [FileOperationType.CopyFiles] = "copying {fileCount} files",
            [FileOperationType.MoveFiles] = "moving {fileCount} files",
            [FileOperationType.RenameThenCopy] = "renaming then copying file {fileName}",
            [FileOperationType.RenameThenCopyFiles] = "renaming then copying {fileCount} files",
            [FileOperationType.RenameThenMove] = "renaming then moving file {fileName}",
            [FileOperationType.RenameThenMoveFiles] = "renaming then moving {fileCount} files"
        };

    private static readonly IReadOnlyDictionary<FileOperationType, string> DetailedTemplates =
    new Dictionary<FileOperationType, string>
    {
        [FileOperationType.Copy] = "copying file {fileName} from {source} to {destination}",
        [FileOperationType.Move] = "moving file {fileName} from {source} to {destination}",
        [FileOperationType.CopyFiles] = "copying {fileCount} files from {source} to {destination}",
        [FileOperationType.MoveFiles] = "moving {fileCount} files from {source} to {destination}",
        // etc.
    };

    public static string GetMessage(FileOperationType type, OperationStep step)
    {
        var stepText = step switch
        {
            OperationStep.Begin => "Begin",
            OperationStep.Finished => "Finished",
            OperationStep.Skipped => "Skipped",
            OperationStep.Retrying => "Retrying",
            OperationStep.Failed => "Failed",
            _ => step.ToString()
        };

        var template = Templates.TryGetValue(type, out var t)
            ? t
            : $"performing file operation ({type})";

        return $"{stepText} {template}";
    }

    public static string GetDetailedMessage(FileOperationType type, OperationStep step)
    {
        var stepText = step switch
        {
            OperationStep.Begin => "Begin",
            OperationStep.Finished => "Finished",
            _ => step.ToString()
        };

        var template = DetailedTemplates.TryGetValue(type, out var t)
            ? t
            : Templates[type];

        return $"{stepText} {template}";
    }

    public static string GetSpectreMessage(FileOperationType type, OperationStep step)
    {
        var msg = GetMessage(type, step);

        return msg
            .Replace("{fileName}", "[yellow]{fileName}[/]")
            .Replace("{fileCount}", "[green]{fileCount}[/]")
            .Replace("{source}", "[blue]{source}[/]")
            .Replace("{destination}", "[blue]{destination}[/]");
    }
}