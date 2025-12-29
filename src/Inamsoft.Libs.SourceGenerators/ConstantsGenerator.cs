using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Inamsoft.Libs.SourceGenerators;

[Generator]
public class ConstantsGenerator : GeneratorBase, IIncrementalGenerator
{
    // Constant definitions for GenerateDictionaryAttribute
    private const string GenerateConstantsFromPropsAAttribNamespace = "Inamsoft.Libs.SourceGenerators.Attributes";
    
    private const string GenerateConstantsFromPropsAttribClassName = "GenerateConstantsFromPropsAttribute";
    private const string GenerateConstantsFromPropsAttribFullName = GenerateConstantsFromPropsAAttribNamespace + "." + GenerateConstantsFromPropsAttribClassName;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {

            
        // 2. Find candidate type declarations with attributes
        var typeDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) =>
                    node is TypeDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, _) =>
                {
                    var typeDecl = (TypeDeclarationSyntax)ctx.Node;
                    var symbol = ctx.SemanticModel.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;
                    return symbol;
                })
            .Where(static symbol => symbol is not null);

        // 3. Filter to types marked with [AutoDictionary]
        var markedTypes = typeDeclarations
            .Where(static symbol =>
                symbol!.GetAttributes().Any(a =>
                    a.AttributeClass?.ToDisplayString() == GenerateConstantsFromPropsAttribFullName));

        // 4. Generate the source per marked type
        context.RegisterSourceOutput(markedTypes, static (spc, typeSymbol) =>
        {
            var source = GenerateCode(typeSymbol!);
            //var source = GenerateExtensionsFor(typeSymbol!);
            spc.AddSource($"{GetSafeTypeName(typeSymbol!)}.PropsAsConstants.g.cs", (string)source);
        });
    }

    private static string GenerateCode(INamedTypeSymbol typeSymbol)
    {
        var outputNamespace = GetOutputNamespace(typeSymbol);
        var typeName = GetSymbolTypeName(typeSymbol);
        
        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine($"using System;");
        sb.AppendLine();
        
        if (outputNamespace is not null)
        {
            sb.Append("namespace ").Append(outputNamespace).AppendLine(";");
        }
        sb.AppendLine();
        sb.AppendLine($"public static class {typeName}Constants");
        sb.AppendLine("{");

        var members = GetAllIMarkedMembers(typeSymbol, GenerateConstantsFromPropsAttribFullName);
        foreach (var member in members)
        {
            sb.Append("\tpublic const string ");
            sb.Append(member.Name);
            sb.Append(" = ");
            sb.Append("\"").Append(member.Name).Append("\"");
            sb.AppendLine(";");
        }
        sb.AppendLine("}");
        
        return sb.ToString();
    }
}