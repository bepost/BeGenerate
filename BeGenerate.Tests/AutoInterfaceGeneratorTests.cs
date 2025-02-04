using System.Linq;
using System.Threading.Tasks;
using BeGenerate.AutoInterface;
using BeGenerate.Generators.AutoInterface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyXunit;
using Xunit;

namespace BeGenerate.Tests;

public sealed class AutoInterfaceGeneratorTests
{
    private static GeneratorDriver BuildDriver(params string[] sources)
    {
        var compilation = CSharpCompilation.Create(
            "compilation",
            sources.Select(s => CSharpSyntaxTree.ParseText(s)),
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(AutoInterfaceAttribute).Assembly.Location)
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new AutoInterfaceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        return driver.RunGenerators(compilation);
    }

    [Fact]
    public Task Driver()
    {
        var driver = BuildDriver();
        return Verifier.Verify(driver);
    }

    [Fact]
    public Task RunResult()
    {
        var driver = BuildDriver(
            """
            using BeGenerate.AutoInterface;

            namespace TestNamespace;

            [AutoInterface]
            public class MyClass
            {
                public string GetMessage() => "Hello";
                public int Add(int a, int b) => a + b;
            }
            """);
        var results = driver.GetRunResult();
        return Verifier.Verify(results.Results);
    }
}
