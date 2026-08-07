using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Inamsoft.Libs.SourceGenerators.Tests;

public static class TestHelper
{
    public static async Task<GeneratorDriverRunResult> RunGeneratorAsync<TGenerator>(params string[] sources)
        where TGenerator : IIncrementalGenerator, new()
    {
        var syntaxTrees = new SyntaxTree[sources.Length];
        for (int i = 0; i < sources.Length; i++)
            syntaxTrees[i] = CSharpSyntaxTree.ParseText(sources[i]);

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
            // add your own assemblies here, e.g. the one containing OperationStep / attributes
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: syntaxTrees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new TGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        return await Task.FromResult(driver.GetRunResult());
    }
}
