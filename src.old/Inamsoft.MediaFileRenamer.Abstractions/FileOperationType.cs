using Inamsoft.Libs.SourceGenerators.Attributes;

namespace Inamsoft.MediaFileRenamer.Abstractions;

/// <summary>
/// 
/// </summary>
public enum FileOperationType
{
    [OperationTemplate("listing files in folder {source}")]
    List,

    [OperationTemplate("copying file {fileName} from {source} to {destination}")]
    Copy,

    [OperationTemplate("moving file {fileName} from {source} to {destination}")]
    Move,

    [OperationTemplate("copying {fileCount} files from {source} to {destination}")]
    CopyFiles,

    [OperationTemplate("moving {fileCount} files from {source} to {destination}")]
    MoveFiles,

    [OperationTemplate("renaming file {fileName} then copying to {destination}")]
    RenameThenCopy,

    [OperationTemplate("renaming then copying {fileCount} files")]
    RenameThenCopyFiles,

    [OperationTemplate("renaming file {fileName} then moving to {destination}", required: "fileName, destination")]
    RenameThenMove,

    [OperationTemplate("renaming then moving {fileCount} files", required: "fileCount")]
    RenameThenMoveFiles
}

