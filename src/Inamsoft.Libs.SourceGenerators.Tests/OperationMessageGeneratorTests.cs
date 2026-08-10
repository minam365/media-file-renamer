using Inamsoft.Libs.SourceGenerators;
using Inamsoft.Libs.SourceGenerators.Tests;
using Xunit;

public class OperationMessageGeneratorTests
{
    [Fact]
    public async Task Generates_OperationMessages()
    {
        var result = await TestHelper.RunGeneratorAsync<OperationMessageGenerator>(
            """
            namespace Inamsoft.MediaFileRenamer.Abstractions;

            public enum FileOperationType
            {
                [OperationTemplate("listing files")]
                List,

                [OperationTemplate("copying file {fileName} from {source} to {destination}")]
                Copy,

                [OperationTemplate("moving file {fileName} from {source} to {destination}")]
                Move,

                [OperationTemplate("copying {fileCount} files from {source} to {destination}")]
                CopyFiles,

                [OperationTemplate("moving {fileCount} files from {source} to {destination}")]
                MoveFiles,

                [OperationTemplate("renaming then copying file {fileName}")]
                RenameThenCopy,

                [OperationTemplate("renaming then copying {fileCount} files")]
                RenameThenCopyFiles,

                [OperationTemplate("renaming then moving file {fileName}")]
                RenameThenMove,

                [OperationTemplate("renaming then moving {fileCount} files")]
                RenameThenMoveFiles
            }
            """,
            SharedSource.AttributeSource,
            SharedSource.OperationStepSource
        );

        //var generated = result.GeneratedTrees
        //    .First(s => s.FilePath == "OperationMessages.g.cs").GetText().ToString();

    }
}