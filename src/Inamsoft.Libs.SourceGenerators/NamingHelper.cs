using System.Text;

namespace Inamsoft.Libs.SourceGenerators;

internal static class NamingHelper
{
    public static string ApplyNamingPolicy(string name, int namingPolicyEnumValue)
    {
        return namingPolicyEnumValue switch
        {
            1 => ToCamelCase(name),
            2 => ToSnakeCase(name),
            _ => name
        };
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
            return name;
        if (name.Length == 1)
            return name.ToLowerInvariant();
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var sb = new StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                    sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}
