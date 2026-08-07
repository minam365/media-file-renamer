using System;
using System.Collections.Generic;
using System.Text;

namespace Inamsoft.Libs.SourceGenerators.Tests;

public static class SharedSource
{
    public const string AttributeSource = @"
namespace Inamsoft.Libs.SourceGenerators.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class OperationTemplateAttribute : System.Attribute
    {
        public string Template { get; }
        public string? Required { get; }
        public string? Optional { get; }
        public OperationTemplateAttribute(string template, string? required = null, string? optional = null)
        {
            Template = template;
            Required = required;
            Optional = optional;
        }
    }
}
";

    public const string OperationStepSource = @"
using Inamsoft.Libs.SourceGenerators.Attributes;

namespace Inamsoft.MediaFileRenamer.Abstractions;

/// <summary>
/// 
/// </summary>
public enum FileOperationType
{
    [OperationTemplate(""listing files"")]
    List,

    [OperationTemplate(""copying file {fileName} from {source} to {destination}"")]
    Copy,

    [OperationTemplate(""moving file {fileName} from {source} to {destination}"")]
    Move,

    [OperationTemplate(""copying {fileCount} files from {source} to {destination}"")]
    CopyFiles,

    [OperationTemplate(""moving {fileCount} files from {source} to {destination}"")]
    MoveFiles,

    [OperationTemplate(""renaming file {fileName} then copying to {destination}"")]
    RenameThenCopy,

    [OperationTemplate(""renaming then copying {fileCount} files"")]
    RenameThenCopyFiles,

    [OperationTemplate(""renaming file {fileName} then moving to {destination}"", required: ""fileName, destination"")]
    RenameThenMove,

    [OperationTemplate(""renaming then moving {fileCount} files"", required: ""fileCount"")]
    RenameThenMoveFiles
}
";
}