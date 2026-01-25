using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Inamsoft.Libs.SourceGenerators.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Inamsoft.Libs.SourceGenerators;


[Generator]
public sealed class AdvancedOperationMessageGenerator : GeneratorBase, IIncrementalGenerator
{
    private static readonly ImmutableHashSet<string> AllowedPlaceholders =
        ImmutableHashSet.Create("fileName", "fileCount", "source", "destination");

    // Constant definitions for GenerateDictionaryAttribute
    private const string AttributesNamespace = "Inamsoft.Libs.SourceGenerators.Attributes";

    private const string OperationTemplateAttribClassName = "OperationTemplateAttribute";
    private const string OperationTemplateAttribFullName = AttributesNamespace + "." + OperationTemplateAttribClassName;

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
                    a.AttributeClass?.ToDisplayString() == OperationTemplateAttribFullName));


        var enums = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is EnumDeclarationSyntax eds && eds.AttributeLists.Count > 0,
                static (ctx, _) => GetEnumModel(ctx))
            .Where(static m => m is not null)!;

        context.RegisterSourceOutput(markedTypes, static (spc, typeSymbol) =>
        {
            var source = GenerateCode(typeSymbol!);
            spc.AddSource($"{GetSafeTypeName(typeSymbol!)}.OperationMessages.g.cs", (string)source);
        });
        //context.RegisterSourceOutput(markedTypes, Generate);

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

        var members = GetAllIMarkedMembers(typeSymbol, OperationTemplateAttribFullName);
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

    private static EnumModel? GetEnumModel(GeneratorSyntaxContext ctx)
    {
        var enumDecl = (EnumDeclarationSyntax)ctx.Node;
        var enumSymbol = ctx.SemanticModel.GetDeclaredSymbol(enumDecl) as INamedTypeSymbol;
        if (enumSymbol is null)
            return null;

        var members = new List<MemberModel>();

        foreach (var member in enumSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            var attr = member.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == OperationTemplateAttribFullName);

            if (attr is null)
                continue;

            var template = attr.ConstructorArguments[0].Value?.ToString() ?? "";
            var placeholders = ExtractPlaceholders(template).ToImmutableArray();
            var invalid = placeholders.Where(p => !AllowedPlaceholders.Contains(p)).ToImmutableArray();

            var requiredRaw = attr.NamedArguments
                .FirstOrDefault(kv => kv.Key == "Required").Value.Value?.ToString();
            var optionalRaw = attr.NamedArguments
                .FirstOrDefault(kv => kv.Key == "Optional").Value.Value?.ToString();

            var required = SplitList(requiredRaw);
            var optional = SplitList(optionalRaw);

            members.Add(new MemberModel(member.Name, template, placeholders, invalid, required, optional));
        }

        if (members.Count == 0)
            return null;

        return new EnumModel(enumSymbol.ContainingNamespace.ToDisplayString(), enumSymbol.Name, members.ToImmutableArray());
    }

    private static IEnumerable<string> ExtractPlaceholders(string template)
    {
        int i = 0;
        while (i < template.Length)
        {
            int start = template.IndexOf('{', i);
            if (start < 0) yield break;

            int end = template.IndexOf('}', start + 1);
            if (end < 0) yield break;

            yield return template.Substring(start + 1, end - start - 1);
            i = end + 1;
        }
    }

    private static ImmutableArray<string> SplitList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ImmutableArray<string>.Empty;

        return value
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToImmutableArray();
    }

    private static void Generate(SourceProductionContext context, ImmutableArray<EnumModel> enums)
    {
        if (enums.IsDefaultOrEmpty)
            return;

        var fileOpEnum = enums.FirstOrDefault(e => e.Name == "FileOperationType");
        if (fileOpEnum is null)
            return;

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("using System;");
        sb.AppendLine("using Spectre.Console;");
        sb.AppendLine();
        sb.AppendLine("namespace FileOpsGen;");
        sb.AppendLine();
        sb.AppendLine("public static class OperationMessages");
        sb.AppendLine("{");

        // Commented warnings for invalid placeholders
        foreach (var m in fileOpEnum.Members)
        {
            foreach (var invalid in m.InvalidPlaceholders)
            {
                sb.AppendLine($"    // WARNING: Invalid placeholder '{{{invalid}}}' in {m.Name}");
            }
        }
        sb.AppendLine();

        // GetTemplate
        sb.AppendLine("    public static string GetTemplate(FileOperationType op) => op switch");
        sb.AppendLine("    {");
        foreach (var m in fileOpEnum.Members)
        {
            var escaped = m.Template.Replace("\"", "\\\"");
            sb.AppendLine($"        FileOperationType.{m.Name} => \"{escaped}\",");
        }
        sb.AppendLine("        _ => throw new ArgumentOutOfRangeException(nameof(op))");
        sb.AppendLine("    };");
        sb.AppendLine();

        // Structured logging template (no step prefix, just the template)
        sb.AppendLine("    public static string GetLogTemplate(FileOperationType op) => GetTemplate(op);");
        sb.AppendLine();

        // Metadata for analyzer & logging
        sb.AppendLine("    public static class OperationMessagesMetadata");
        sb.AppendLine("    {");
        sb.AppendLine("        public static readonly System.Collections.Generic.Dictionary<FileOperationType, string[]> Placeholders =");
        sb.AppendLine("            new System.Collections.Generic.Dictionary<FileOperationType, string[]>");
        sb.AppendLine("            {");
        foreach (var m in fileOpEnum.Members)
        {
            var list = string.Join(", ", m.Placeholders.Select(p => $"\"{p}\""));
            sb.AppendLine($"                [FileOperationType.{m.Name}] = new[] {{ {list} }},");
        }
        sb.AppendLine("            };");
        sb.AppendLine();
        sb.AppendLine("        public static readonly System.Collections.Generic.Dictionary<FileOperationType, string[]> Required =");
        sb.AppendLine("            new System.Collections.Generic.Dictionary<FileOperationType, string[]>");
        sb.AppendLine("            {");
        foreach (var m in fileOpEnum.Members)
        {
            var list = string.Join(", ", m.Required.Select(p => $"\"{p}\""));
            sb.AppendLine($"                [FileOperationType.{m.Name}] = new[] {{ {list} }},");
        }
        sb.AppendLine("            };");
        sb.AppendLine();
        sb.AppendLine("        public static readonly System.Collections.Generic.Dictionary<FileOperationType, string[]> Optional =");
        sb.AppendLine("            new System.Collections.Generic.Dictionary<FileOperationType, string[]>");
        sb.AppendLine("            {");
        foreach (var m in fileOpEnum.Members)
        {
            var list = string.Join(", ", m.Optional.Select(p => $"\"{p}\""));
            sb.AppendLine($"                [FileOperationType.{m.Name}] = new[] {{ {list} }},");
        }
        sb.AppendLine("            };");
        sb.AppendLine("    }");
        sb.AppendLine();



        // Per-operation context types + methods
        foreach (var m in fileOpEnum.Members)
        {
            GenerateContextType(sb, m);
            GenerateFormatterMethods(sb, m);
            GenerateSpectreFormatterMethods(sb, m);
            GenerateProgressHelper(sb, m);
        }

        sb.AppendLine();
        sb.AppendLine("    public static class OperationLoggingExtensions");
        sb.AppendLine("    {");
        foreach (var m in fileOpEnum.Members)
        {
            var ctxType = $"{m.Name}Context";
            sb.AppendLine($"        public static void Log{m.Name}(this Microsoft.Extensions.Logging.ILogger logger, {ctxType} ctx)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var template = OperationMessages.GetLogTemplate(FileOperationType.{m.Name});");

            // map placeholders to arguments in deterministic order
            var args = string.Join(", ", m.Placeholders.Select(p =>
                p == "fileCount" ? $"ctx.FileCount" : $"ctx.{UpperFirst(p)}"));

            sb.AppendLine($"            logger.LogInformation(template, {args});");
            sb.AppendLine("        }");
            sb.AppendLine();
        }
        sb.AppendLine("    }");

        sb.AppendLine("}");

        context.AddSource("OperationMessages.g.cs", sb.ToString());
    }

    private static void GenerateContextType(StringBuilder sb, MemberModel m)
    {
        var typeName = $"{m.Name}Context";

        sb.AppendLine($"    public sealed record {typeName}(");
        sb.AppendLine(string.Join(",\n", m.Placeholders.Select(p =>
            p switch
            {
                "fileCount" => "        int FileCount",
                _ => $"        string {UpperFirst(p)}"
            })));
        sb.AppendLine("    );");
        sb.AppendLine();
    }

    private static void GenerateFormatterMethods(StringBuilder sb, MemberModel m)
    {
        var typeName = $"{m.Name}Context";

        sb.AppendLine($"    public static string {m.Name}({typeName} ctx)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var template = GetTemplate(FileOperationType.{m.Name});");

        if (m.Placeholders.Length == 0)
        {
            sb.AppendLine("        return template;");
        }
        else
        {
            sb.Append("        return template");
            foreach (var p in m.Placeholders)
            {
                var prop = p == "fileCount" ? "FileCount" : UpperFirst(p);
                sb.Append($".Replace(\"{{{p}}}\", ctx.{prop}.ToString())");
            }
            sb.AppendLine(";");
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        // Step-aware message
        sb.AppendLine($"    public static string {m.Name}(OperationStep step, {typeName} ctx)");
        sb.AppendLine("    {");
        sb.AppendLine("        var stepText = step switch");
        sb.AppendLine("        {");
        sb.AppendLine("            OperationStep.Begin => \"Begin\",");
        sb.AppendLine("            OperationStep.Finished => \"Finished\",");
        sb.AppendLine("            OperationStep.Skipped => \"Skipped\",");
        sb.AppendLine("            OperationStep.Retrying => \"Retrying\",");
        sb.AppendLine("            OperationStep.Failed => \"Failed\",");
        sb.AppendLine("            _ => step.ToString()");
        sb.AppendLine("        };");
        sb.AppendLine($"        return stepText + \" \" + {m.Name}(ctx);");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateSpectreFormatterMethods(StringBuilder sb, MemberModel m)
    {
        var typeName = $"{m.Name}Context";

        sb.AppendLine($"    public static string {m.Name}Spectre({typeName} ctx)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var template = GetTemplate(FileOperationType.{m.Name});");

        if (m.Placeholders.Length == 0)
        {
            sb.AppendLine("        return template;");
        }
        else
        {
            sb.Append("        return template");
            foreach (var p in m.Placeholders)
            {
                var prop = p == "fileCount" ? "FileCount" : UpperFirst(p);
                var color = p switch
                {
                    "fileName" => "yellow",
                    "source" => "blue",
                    "destination" => "blue",
                    "fileCount" => "green",
                    _ => "white"
                };
                sb.Append($".Replace(\"{{{p}}}\", $\"[{color}]{{ctx.{prop}}}[/]\")");
            }
            sb.AppendLine(";");
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine($"    public static string {m.Name}Spectre(OperationStep step, {typeName} ctx)");
        sb.AppendLine("    {");
        sb.AppendLine("        var stepText = step switch");
        sb.AppendLine("        {");
        sb.AppendLine("            OperationStep.Begin => \"Begin\",");
        sb.AppendLine("            OperationStep.Finished => \"Finished\",");
        sb.AppendLine("            OperationStep.Skipped => \"Skipped\",");
        sb.AppendLine("            OperationStep.Retrying => \"Retrying\",");
        sb.AppendLine("            OperationStep.Failed => \"Failed\",");
        sb.AppendLine("            _ => step.ToString()");
        sb.AppendLine("        };");
        sb.AppendLine($"        return stepText + \" \" + {m.Name}Spectre(ctx);");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateProgressHelper(StringBuilder sb, MemberModel m)
    {
        var typeName = $"{m.Name}Context";

        sb.AppendLine($"    public static void {m.Name}ProgressTask(ProgressTask task, OperationStep step, {typeName} ctx)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var message = {m.Name}Spectre(step, ctx);");
        sb.AppendLine("        task.Description = message;");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static string UpperFirst(string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        if (s.Length == 1)
            return s.ToUpperInvariant();

        return char.ToUpperInvariant(s[0]) + s.Substring(1);
    }

    private sealed record EnumModel(string Namespace, string Name, ImmutableArray<MemberModel> Members);
    private sealed record MemberModel(
        string Name,
        string Template,
        ImmutableArray<string> Placeholders,
        ImmutableArray<string> InvalidPlaceholders,
        ImmutableArray<string> Required,
        ImmutableArray<string> Optional);
}
