using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BeGenerate.AutoInterface;
using BeGenerate.Generators.AutoInterface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using VerifyXunit;
using Xunit;

namespace BeGenerate.Tests;

public sealed partial class AutoInterfaceGeneratorTests
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

    private void CheckOutput(string code, string expected)
    {
        var driver = BuildDriver(
            $$"""
              using System;
              using BeGenerate.AutoInterface;

              namespace TestNamespace;

              [AutoInterface]
              public class MyClass
              {
                  {{code}}  
              }
              """);
        var results = driver.GetRunResult();
        var source = results.Results.Single()
            .GeneratedSources.Single()
            .SourceText.ToString();
        var generatedSource = TrimOutside()
            .Replace(source, "");
        generatedSource.ShouldContain(expected);
    }

    [GeneratedRegex(@"^[^{]*\{\s*|\s*\}[^}]*$", RegexOptions.Singleline)]
    private static partial Regex TrimOutside();

    private static void VersionScrubber(StringBuilder content)
    {
        var version = typeof(AutoInterfaceGenerator).Assembly
                          .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                          ?.InformationalVersion ??
                      "1.0.0";
        content.Replace(version, "***");
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
        CheckOutput("public string Message { get; }", "string Message { get; }");
    }

    [Fact]
    public void GenerateGetterSetter()
    {
        CheckOutput("public string Message { get; set; }", "string Message { get; set; }");
    }

    [Fact]
    public void GenerateMethod()
    {
        CheckOutput("public int Add(int a, int b) => a + b;", "int Add(int a, int b);");
    }

    [Fact]
    public void GenerateMethodWithDefaults()
    {
        CheckOutput(
            "public int Add(int a = 1, int b = default, object o = default, System.Threading.CancellationToken cts = default) => a + b;",
            "int Add(int a = 1, int b = 0, object o = default, System.Threading.CancellationToken cts = default);");
    }

    [Fact]
    public void GenerateMethodWithGeneric()
    {
        CheckOutput("public T Add<T>(T a, T b) { throw new Exception(); }", "T Add<T>(T a, T b);");
    }

    [Fact]
    public void GenerateMethodWithGenericAndConstraint()
    {
        CheckOutput(
            "public T Add<T>(T a, T b) where T: class?, IEquatable<T>?, new() { throw new Exception(); }",
            "T Add<T>(T a, T b) where T: class?, System.IEquatable<T>?, new();");
    }

    [Fact]
    public void GenerateSetter()
    {
        CheckOutput("public string Message { set; }", "string Message { set; }");
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
        return Verifier.Verify(results.Results)
            .AddScrubber(VersionScrubber);
    }

    [Fact]
    public Task GenerateValidOutputGeneric()
    {
        var driver = BuildDriver(
            """
            using BeGenerate.AutoInterface;

            namespace TestNamespace;

            [AutoInterface]
            public class MyClass<T> where T: class?, IEquatable<T>?, new()
            {
                public void Test(T a, T b) {}
                public string Message { get; set; }
            }
            """);
        var results = driver.GetRunResult();

        return Verifier.Verify(results.Results)
            .AddScrubber(VersionScrubber);
    }
}
