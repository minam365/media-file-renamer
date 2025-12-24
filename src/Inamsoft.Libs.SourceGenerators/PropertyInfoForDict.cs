using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

namespace Inamsoft.Libs.SourceGenerators
{

    public sealed record PropertyInfoForDict(string Name, ITypeSymbol Type, bool IsFlattenableNested, string AccessExpression);
}