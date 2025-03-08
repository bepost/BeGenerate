using System.IO;
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
    private static GeneratorDriver BuildDriver(string source)
    {
        var systemRuntimePath = Path.Combine(
            Path.GetDirectoryName(typeof(object).Assembly.Location)!,
            "System.Runtime.dll");

        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            "compilation",
            [syntaxTree],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(systemRuntimePath),
                MetadataReference.CreateFromFile(typeof(AutoInterfaceAttribute).Assembly.Location)
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var driver = CSharpGeneratorDriver.Create(new AutoInterfaceGenerator());
        var generatorResult = driver.RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out _);

        var compilationResult = updatedCompilation.Emit(new MemoryStream());
        if (compilationResult.Success)
            return generatorResult;

        var errors = compilationResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => $"{d.Id}: {d.GetMessage()} (at {d.Location})")
            .ToList();

        Assert.Fail($"Compilation failed with {errors.Count} error(s):\n" + string.Join("\n", errors));
        return generatorResult;
    }

    private static void CheckOutput(string code, string expected)
    {
        var driver = BuildDriver(
            $$"""
              using System;
              using BeGenerate.AutoInterface;
              using System.Diagnostics.CodeAnalysis;

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
        var driver = BuildDriver("");
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
    public Task GenerateCustomNameOutput()
    {
        var driver = BuildDriver(
            """
            using BeGenerate.AutoInterface;

            namespace TestNamespace;

            [AutoInterface(Name = "Test")]
            public class MyClass : Test;
            """);
        var results = driver.GetRunResult();
        return Verifier.Verify(results.Results)
            .AddScrubber(VersionScrubber);
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
    public Task GenerateInternalOutput()
    {
        var driver = BuildDriver(
            """
            using BeGenerate.AutoInterface;

            namespace TestNamespace;

            [AutoInterface(Accessibility = InterfaceAccessibility.Internal)]
            public class MyClass : IMyClass;
            """);
        var results = driver.GetRunResult();
        return Verifier.Verify(results.Results)
            .AddScrubber(VersionScrubber);
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
    public Task GenerateNoneAccessorOutput()
    {
        var driver = BuildDriver(
            """
            using BeGenerate.AutoInterface;

            namespace TestNamespace;

            [AutoInterface(Accessibility = InterfaceAccessibility.None)]
            public class MyClass : IMyClass;
            """);
        var results = driver.GetRunResult();
        return Verifier.Verify(results.Results)
            .AddScrubber(VersionScrubber);
    }

    [Fact]
    public void GeneratePropertyWithAttributes()
    {
        CheckOutput(
            """[Obsolete] public string Message { [Obsolete("get")] get; [Obsolete("set")] set; }""",
            """[Obsolete] string Message { [Obsolete("get")] get; [Obsolete("set")] set; }""");
    }

    [Fact]
    public void GenerateSetter()
    {
        CheckOutput("public string Message { set {} }", "string Message { set; }");
    }

    [Fact]
    public Task GenerateValidOutput()
    {
        var driver = BuildDriver(
            """
            using System;
            using BeGenerate.AutoInterface;

            namespace TestNamespace;

            interface IOther
            {
                void OtherExplicit();
            }

            /// <summary>
            ///   This is a test interface.
            /// </summary>
            [AutoInterface(Accessibility = InterfaceAccessibility.Public)]
            [Implements<IEquatable<IMyClass>>]
            [Implements<IComparable>]
            public class MyClass : IMyClass, IOther
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
                void IOther.OtherExplicit() {}
                bool IEquatable<IMyClass>.Equals(IMyClass? x)=>false;
                int IComparable.CompareTo(object? x)=>0;
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
            using System;

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
