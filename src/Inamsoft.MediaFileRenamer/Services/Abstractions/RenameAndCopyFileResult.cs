using Inamsoft.Libs.MediaFileRenaming;

namespace Inamsoft.MediaFileRenamer.Services.Abstractions;

public record RenameAndCopyFileResult(RenameFileResult RenameFileResult, bool IsCopied)
{
}
