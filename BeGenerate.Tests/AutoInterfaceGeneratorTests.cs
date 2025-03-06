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
              public class MyClass : IMyClass
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
        generatedSource = WhitespaceRegex()
            .Replace(generatedSource, " ");
        expected = WhitespaceRegex()
            .Replace(expected, " ");
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

    [GeneratedRegex("\\s+")]
    private static partial Regex WhitespaceRegex();

    [Fact]
    public Task Driver()
    {
        var driver = BuildDriver();
        return Verifier.Verify(driver);
    }

    [Fact]
    public void EscapeKeywords()
    {
        CheckOutput("public void Test(int @class, int @event) {}", "void Test(int @class, int @event);");
    }

    [Fact]
    public void GenerateArrowGetter()
    {
        CheckOutput("public string Message => string.Empty;", "string Message { get; }");
    }

    [Fact]
    public void GenerateArrowGetterSetter()
    {
        CheckOutput(
            "private string _m; public string Message { get => _m; set => _m = value; }",
            "string Message { get; set; }");
    }

    [Fact]
    public void GenerateExplicitGetterSetter()
    {
        CheckOutput("string IMyClass.Message { get; set; }", "string Message { get; set; }");
    }

    [Fact]
    public void GenerateExplicitMethod()
    {
        CheckOutput("int IMyClass.Add(int a, int b) => a + b;", "int Add(int a, int b);");
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
    public void GenerateMethodWithAttributes()
    {
        CheckOutput("[DoesNotReturn] public void Quit() {}", "[DoesNotReturn] void Quit();");
    }

    [Fact]
    public void GenerateMethodWithDefaults()
    {
        CheckOutput(
            "public int Add(int a = 1, int b = 0, object o = default, System.Threading.CancellationToken cts = default) => a + b;",
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
            "T Add<T>(T a, T b) where T: class?, IEquatable<T>?, new();");
    }

    [Fact]
    public void GeneratePropertyWithAttributes()
    {
        CheckOutput(
            "[DoesNotReturn] public string Message { [A] get; [B] set; }",
            "[DoesNotReturn]\n    string Message { [A] get; [B] set; }");
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
            [Implements<IEquatable<IMyClass>>]
            [Implements<IComparable>]
            public class MyClass : IMyClass
            {
                /// <summary>
                ///   This is a test method.
                /// </summary>
                /// <param name="a">A</param>
                /// <param name="b">B</param>
                /// <returns>Sum of A and B</returns>
                public int Add(int a, int b) => a + b;
                /// <summary>
                ///   This is a test method.
                /// </summary>
                public string Message { get; set; }
                [ExcludeFromInterface]
                public int Test { get; set; }
                private void PrivateMethod() {}
                protected void ProtectedMethod() {}
                internal void InternalMethod() {}
                void IComparable.OtherExplicit() {}
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
