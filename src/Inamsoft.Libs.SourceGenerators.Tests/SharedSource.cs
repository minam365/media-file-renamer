using System;
using System.Collections.Generic;
using System.Text;

namespace Inamsoft.Libs.SourceGenerators.Tests;

public static class SharedSource
{
    public const string AttributeSource = @"
namespace FileOpsGen
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
namespace FileOpsGen
{
    public enum OperationStep { Begin, Finished, Skipped, Retrying, Failed }
}
";
}