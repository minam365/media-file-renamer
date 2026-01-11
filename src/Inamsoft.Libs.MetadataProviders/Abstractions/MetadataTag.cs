namespace Inamsoft.Libs.MetadataProviders.Abstractions;

/// <summary>
/// 
/// </summary>
/// <param name="Type"></param>
/// <param name="Name"></param>
/// <param name="Value"></param>
/// <param name="DirectoryName"></param>
public readonly record struct MetadataTag(int Type, string Name, string? Value, string DirectoryName)
{
    /// <summary>
    /// Gets a value indicating whether the current instance contains a non-empty value.
    /// </summary>
    public bool HasValue => !string.IsNullOrEmpty(Value);
}
