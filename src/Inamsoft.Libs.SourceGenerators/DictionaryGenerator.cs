using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Inamsoft.Libs.SourceGenerators;

[Generator]
public sealed partial class DictionaryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {

        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource("MyExampleAttribute.g.cs", @"
                namespace HelloWorld
                {
                    // 👇 Use the attribute
                    [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
                    internal class MyExampleAttribute: global::System.Attribute {} 
                }");
        });

        var typeDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) =>
                    node is RecordDeclarationSyntax or ClassDeclarationSyntax or StructDeclarationSyntax,
                transform: static (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol((TypeDeclarationSyntax)ctx.Node) as INamedTypeSymbol
            )
            .Where(static t => t is not null);

        var candidates = typeDeclarations.Select(
            static (typeDecl, _) => typeDecl
        );

        //var withAttribute = candidates.Where(static symbol =>
        //    symbol!.GetAttributes().Any(a =>
        //        a.AttributeClass?.ToDisplayString() == "Inamsoft.Libs.SourceGenerators.Attributes.GenerateDictionaryAttribute"));

        var attr = candidates.Select(static (symbol, _) =>
            symbol!.GetAttributes());

        var withAttribute = candidates;

        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("MyExampleAttribute2.g.cs", $"{candidates}");
        });


        context.RegisterSourceOutput(withAttribute, GenerateDictionaryForType);
    }
    private static void GenerateDictionaryForType(SourceProductionContext context, INamedTypeSymbol typeSymbol)
    {
        //var attributeData = typeSymbol.GetAttributes().First(a =>
        //    a.AttributeClass?.ToDisplayString() == "Inamsoft.Libs.SourceGenerators.Attributes.GenerateDictionaryAttribute");

        var attributeData = typeSymbol.GetAttributes().First();


        var namingPolicyEnumValue = 0;
        if (attributeData.ConstructorArguments.Length == 1 &&
            attributeData.ConstructorArguments[0].Value is int intVal)
        {
            namingPolicyEnumValue = intVal;
        }

        var properties = GetRelevantProperties(typeSymbol).ToImmutableArray();

        if (properties.Length == 0)
        {
            // Diagnostic: type annotated but has no public properties
            var diag = Diagnostic.Create(
                new DiagnosticDescriptor(
                    id: "ADICT001",
                    title: "No public properties",
                    messageFormat: "Type '{0}' is marked with [GenerateDictionary] but has no public readable properties.",
                    category: "AutoDict",
                    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true),
                typeSymbol.Locations.FirstOrDefault(),
                typeSymbol.Name);

            context.ReportDiagnostic(diag);
            return;
        }

        var source = GenerateSourceForType(typeSymbol, properties, namingPolicyEnumValue);
        context.AddSource($"{typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace('.', '_')}_Dictionary.g.cs", source);
    }

    private static IEnumerable<PropertyInfoForDict> GetRelevantProperties(INamedTypeSymbol typeSymbol)
    {
        var hasGenerateDictAttribute = typeSymbol.GetAttributes()
            .Any(a => a.AttributeClass?.ToDisplayString() == "AutoDict.GenerateDictionaryAttribute");

        foreach (var member in typeSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (member.IsStatic) continue;
            if (member.IsIndexer) continue;
            if (member.DeclaredAccessibility != Accessibility.Public) continue;
            if (member.GetMethod is null) continue;

            var ignored = member.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString() == "AutoDict.DictionaryIgnoreAttribute");
            if (ignored) continue;

            var memberType = member.Type;
            var nestedHasAttribute = memberType.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString() == "AutoDict.GenerateDictionaryAttribute");

            // We treat nested flattenable if the nested type also has [GenerateDictionary].
            var isFlattenable = nestedHasAttribute;

            yield return new PropertyInfoForDict(
                Name: member.Name,
                Type: memberType,
                IsFlattenableNested: isFlattenable,
                AccessExpression: $"this.{member.Name}"
            );
        }
    }

}

public sealed partial class DictionaryGenerator
{
    private static string GenerateSourceForType(
        INamedTypeSymbol typeSymbol,
        ImmutableArray<PropertyInfoForDict> properties,
        int namingPolicyEnumValue)
    {
        var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : typeSymbol.ContainingNamespace.ToDisplayString();

        var typeKindKeyword = typeSymbol.TypeKind switch
        {
            TypeKind.Class => "class",
            TypeKind.Struct => "struct",
            // There is no TypeKind.Record or TypeKind.RecordStruct in Roslyn's TypeKind enum.
            // Use IsRecord property to check for records.
            _ when typeSymbol.IsRecord && typeSymbol.TypeKind == TypeKind.Struct => "record struct",
            _ when typeSymbol.IsRecord => "record",
            _ => "class"
        };

        var sb = new StringBuilder();

        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Collections.ObjectModel;");
        sb.AppendLine();

        if (ns is not null)
        {
            sb.Append("namespace ").Append(ns).AppendLine(";");
            sb.AppendLine();
        }

        sb.Append("public partial ").Append(typeKindKeyword).Append(' ')
            .Append(typeSymbol.Name);

        if (!typeSymbol.TypeParameters.IsEmpty)
        {
            sb.Append('<');
            sb.Append(string.Join(", ", typeSymbol.TypeParameters.Select(tp => tp.Name)));
            sb.Append('>');
        }

        sb.AppendLine();
        sb.AppendLine("{");

        // ToDictionary()
        sb.AppendLine("    public System.Collections.Generic.Dictionary<string, object?> ToDictionary()");
        sb.AppendLine("    {");
        sb.AppendLine("        var dict = new System.Collections.Generic.Dictionary<string, object?>();");

        EmitPropertyAssignments(sb, properties, namingPolicyEnumValue, "dict");

        sb.AppendLine("        return dict;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // ToDictionary<TValue>()
        sb.AppendLine("    public System.Collections.Generic.Dictionary<string, TValue?> ToDictionary<TValue>()");
        sb.AppendLine("    {");
        sb.AppendLine("        var dict = new System.Collections.Generic.Dictionary<string, TValue?>();");

        EmitPropertyAssignmentsGeneric(sb, properties, namingPolicyEnumValue, "dict");

        sb.AppendLine("        return dict;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // ToReadOnlyDictionary()
        sb.AppendLine("    public System.Collections.ObjectModel.ReadOnlyDictionary<string, object?> ToReadOnlyDictionary()");
        sb.AppendLine("    {");
        sb.AppendLine("        return new System.Collections.ObjectModel.ReadOnlyDictionary<string, object?>(ToDictionary());");
        sb.AppendLine("    }");

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void EmitPropertyAssignments(
        StringBuilder sb,
        ImmutableArray<PropertyInfoForDict> properties,
        int namingPolicyEnumValue,
        string dictVarName)
    {
        foreach (var p in properties)
        {
            if (p.IsFlattenableNested)
            {
                // Flatten by calling nested object's ToDictionary()
                sb.Append("        if (")
                    .Append(p.AccessExpression)
                    .AppendLine(" is not null)")
                  .Append("        {")
                    .AppendLine()
                  .Append("            foreach (var kvp in ")
                    .Append(p.AccessExpression)
                    .AppendLine(".ToDictionary())")
                  .AppendLine("            {")
                  .Append("                ")
                    .Append(dictVarName)
                    .AppendLine("[kvp.Key] = kvp.Value;")
                  .AppendLine("            }")
                  .AppendLine("        }");
            }
            else
            {
                var key = NamingHelper.ApplyNamingPolicy(p.Name, namingPolicyEnumValue);
                sb.Append("        ")
                  .Append(dictVarName)
                  .Append("[\"")
                  .Append(key)
                  .Append("\"] = ")
                  .Append(p.AccessExpression)
                  .AppendLine(";");
            }
        }
    }

    private static void EmitPropertyAssignmentsGeneric(
        StringBuilder sb,
        ImmutableArray<PropertyInfoForDict> properties,
        int namingPolicyEnumValue,
        string dictVarName)
    {
        foreach (var p in properties)
        {
            if (p.IsFlattenableNested)
            {
                sb.Append("        if (")
                    .Append(p.AccessExpression)
                    .AppendLine(" is not null)")
                  .Append("        {")
                    .AppendLine()
                  .Append("            foreach (var kvp in ")
                    .Append(p.AccessExpression)
                    .AppendLine(".ToDictionary<TValue>())")
                  .AppendLine("            {")
                  .Append("                ")
                    .Append(dictVarName)
                    .AppendLine("[kvp.Key] = kvp.Value;")
                  .AppendLine("            }")
                  .AppendLine("        }");
            }
            else
            {
                var key = NamingHelper.ApplyNamingPolicy(p.Name, namingPolicyEnumValue);
                sb.Append("        ")
                  .Append(dictVarName)
                  .Append("[\"")
                  .Append(key)
                  .Append("\"] = (TValue?)(")
                  .Append(p.AccessExpression)
                  .AppendLine(");");
            }
        }
    }
}
