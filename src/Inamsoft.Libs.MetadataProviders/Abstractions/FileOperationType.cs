using Inamsoft.Libs.SourceGenerators.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inamsoft.Libs.MetadataProviders.Abstractions
{
    public enum FileOperationType
    {
        [OperationTemplate("listing files")]
        List
    }
}
