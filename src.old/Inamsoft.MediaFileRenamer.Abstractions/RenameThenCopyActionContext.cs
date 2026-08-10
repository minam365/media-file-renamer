using Inamsoft.Libs.SourceGenerators.Attributes;

namespace Inamsoft.MediaFileRenamer.Abstractions;

[GenerateConstantsFromProps]
public record RenameThenCopyActionContext(string RenameFileResult, bool IsCopied)
{
}
