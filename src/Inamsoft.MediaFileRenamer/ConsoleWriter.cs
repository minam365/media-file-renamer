using Inamsoft.Libs.MetadataProviders.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using static Inamsoft.MediaFileRenamer.MediaFileHelper;

namespace Inamsoft.MediaFileRenamer;

internal static class ConsoleWriter
{

    private static readonly IReadOnlyDictionary<FileOperationType, string> OperationDescriptions =
        new Dictionary<FileOperationType, string>
        {
            [FileOperationType.List] = "Listing files",
            [FileOperationType.Copy] = "Copying file",
            [FileOperationType.CopyFiles]= "Copying files",
            [FileOperationType.Move] = "Moving file",
            [FileOperationType.MoveFiles]= "Moving files",
            [FileOperationType.RenameThenCopy] = "Renaming then copying file",
            [FileOperationType.RenameThenCopyFiles] = "Renaming then copying files",
            [FileOperationType.RenameThenMove] = "Renaming then moving file",
            [FileOperationType.RenameThenMoveFiles] = "Renaming then moving files"
        };

    public static string GetOperationInfo(FileOperationType operationType, string stepName)
    {
        if (string.IsNullOrWhiteSpace(stepName))
            throw new ArgumentException("Step name must be provided.", nameof(stepName));

        var description = OperationDescriptions.TryGetValue(operationType, out var text)
            ? text
            : $"Performing file operation ({operationType})";

        return $"{stepName}: {description}";
    }

}
