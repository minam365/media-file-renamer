using System;

namespace Inamsoft.Libs.SourceGenerators.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class GenerateDictionaryAttribute : Attribute
    {
        public GenerateDictionaryAttribute() { }

        public GenerateDictionaryAttribute(DictionaryNamingPolicy namingPolicy)
        {
            NamingPolicy = namingPolicy;
        }

        public DictionaryNamingPolicy NamingPolicy { get; }
    }
}
