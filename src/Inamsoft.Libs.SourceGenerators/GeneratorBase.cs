using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Inamsoft.Libs.SourceGenerators;

public abstract class GeneratorBase
{
    protected static string GetSafeTypeName(INamedTypeSymbol typeSymbol)
    {
        // Turn things like Namespace.Outer+Inner<T> into a file-safe name
        var name = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var builder = new StringBuilder(name.Length);

        foreach (var ch in name)
        {
            builder.Append(char.IsLetterOrDigit(ch) ? ch : '_');
        }

        return builder.ToString();
    }

    protected static string? GetOutputNamespace(INamedTypeSymbol typeSymbol)
    {
        var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : typeSymbol.ContainingNamespace.ToDisplayString();
        
        return ns;
    }

    protected static string GetSymbolTypeName(INamedTypeSymbol typeSymbol) =>
        typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
    
    protected static string GetSymbolName(ISymbol symbol) => 
        symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

    protected static string GetSymbolTypeName(ISymbol symbol) =>
        symbol.Name;

    protected static string GetSymbolTypeKind(INamedTypeSymbol typeSymbol)
    {
        var typeKindKeyword = typeSymbol.TypeKind switch
        {
            TypeKind.Class when typeSymbol is { IsRecord: false } => "class",
            TypeKind.Class when typeSymbol is { IsRecord: true } => "record",
            TypeKind.Struct => "struct",
            // There is no TypeKind.Record or TypeKind.RecordStruct in Roslyn's TypeKind enum.
            // Use IsRecord property to check for records.
            _ when typeSymbol is { IsRecord: true, TypeKind: TypeKind.Struct } => "record struct",
            _ when typeSymbol.IsRecord => "record",
            _ => "class"
        };
        return typeKindKeyword;
    }
    
    protected static ImmutableArray<ISymbol> GetAllIMarkedMembers(INamedTypeSymbol typeSymbol,
        string markerAttribFullName)
    {
        List<ISymbol> members = new List<ISymbol>(20);

        var currentSymbol = typeSymbol;
        do
        {
            var props = currentSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p =>
                    p.DeclaredAccessibility == Accessibility.Public &&
                    p.GetMethod is not null &&
                    !HasMarkerAttribute(p, markerAttribFullName) &&
                    !p.IsStatic);

            var fields = currentSymbol.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(f =>
                    f.DeclaredAccessibility == Accessibility.Public &&
                    f is { IsStatic: false, IsConst: false } &&
                    !HasMarkerAttribute(f, markerAttribFullName));
            
            members.AddRange(props);
            members.AddRange(fields);
            
            currentSymbol = currentSymbol.BaseType;

        } while (currentSymbol?.BaseType is not null);
        
        members.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.Ordinal));

        return [..members];
    }
    
    private static bool HasMarkerAttribute(ISymbol symbol, string markerAttribFullName)
    {
        return symbol.GetAttributes()
            .Any(a => a.AttributeClass?.ToDisplayString() == markerAttribFullName);
    }
}