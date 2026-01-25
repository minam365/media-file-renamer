using System;

namespace Inamsoft.Libs.SourceGenerators.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class OperationTemplateAttribute : Attribute
    {
        public string Template { get; }
        public string Required { get; }
        public string Optional { get; }

        public OperationTemplateAttribute(string template, string required = null, string optional = null)
        {
            Template = template;
            Required = required;
            Optional = optional;
        }
    }

}
