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
    public void GenerateGetter()
    {
        var driver = BuildDriver(
            """
            using BeGenerate.AutoInterface;

            namespace TestNamespace;

            [AutoInterface]
            public class MyClass
            {
                public string Message { get; }
            }
            """);
        var results = driver.GetRunResult();
        var source = results.Results.Single()
            .GeneratedSources.Single()
            .SourceText.ToString();
        Assert.Contains("string Message { get; }", source);
    }

    [Fact]
    public void GenerateGetterSetter()
    {
        var driver = BuildDriver(
            """
            using BeGenerate.AutoInterface;

            namespace TestNamespace;

            [AutoInterface]
            public class MyClass
            {
                public string Message { get; set; }
            }
            """);
        var results = driver.GetRunResult();
        var source = results.Results.Single()
            .GeneratedSources.Single()
            .SourceText.ToString();
        Assert.Contains("string Message { get; set; }", source);
    }

    [Fact]
    public void GenerateMethod()
    {
        var driver = BuildDriver(
            """
            using BeGenerate.AutoInterface;

            namespace TestNamespace;

            [AutoInterface]
            public class MyClass
            {
                public int Add(int a, int b) => a + b;
            }
            """);
        var results = driver.GetRunResult();
        var source = results.Results.Single()
            .GeneratedSources.Single()
            .SourceText.ToString();
        Assert.Contains("int Add(int a, int b);", source);
    }

    [Fact]
    public void GenerateSetter()
    {
        var driver = BuildDriver(
            """
            using BeGenerate.AutoInterface;

            namespace TestNamespace;

            [AutoInterface]
            public class MyClass
            {
                public string Message { set; }
            }
            """);
        var results = driver.GetRunResult();
        var source = results.Results.Single()
            .GeneratedSources.Single()
            .SourceText.ToString();
        Assert.Contains("string Message { set; }", source);
    }

    [Fact]
    public Task GenerateValidOutput()
    {
        var driver = BuildDriver(
            """
            using BeGenerate.AutoInterface;

            namespace TestNamespace;

            [AutoInterface]
            public class MyClass
            {
                public int Add(int a, int b) => a + b;
                public string Message { get; set; }
            }
            """);
        var results = driver.GetRunResult();
        return Verifier.Verify(results.Results);
    }
}
