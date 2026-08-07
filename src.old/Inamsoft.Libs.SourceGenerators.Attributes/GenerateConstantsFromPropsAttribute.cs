using System;

namespace Inamsoft.Libs.SourceGenerators.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class GenerateConstantsFromPropsAttribute : Attribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class GenerateConstantsFromEnumsAttribute : Attribute
    {
 
    }
}