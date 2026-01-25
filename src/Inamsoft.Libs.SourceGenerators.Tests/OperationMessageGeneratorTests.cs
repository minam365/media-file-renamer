using Inamsoft.Libs.SourceGenerators;
using Inamsoft.Libs.SourceGenerators.Tests;
using Xunit;

public class OperationMessageGeneratorTests
{
    [Fact]
    public async Task Generates_OperationMessages()
    {
        var result = await TestHelper.RunGeneratorAsync<AdvancedOperationMessageGenerator>(
            @"using Inamsoft.Libs.SourceGenerators.Attributes;
              public enum FileOperationType
              {
                  [OperationTemplate(""copying file {fileName}"", required: ""fileName"")]
                  Copy
              }",
            SharedSource.AttributeSource,
            SharedSource.OperationStepSource
        );

        //var generated = result.GeneratedTrees
        //    .First(s => s.FilePath == "OperationMessages.g.cs").GetText().ToString();

    }
}