using Inamsoft.Libs.SourceGenerators.Attributes;

namespace Inamsoft.MediaFileRenamer.Services.Abstractions;

[GenerateConstantsFromProps]
public record RenameThenCopyActionContext(string RenameFileResult, bool IsCopied)
{
}
