using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BeGenerate.Generators;

[Generator]
public sealed class AutoInterfaceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not AutoInterfaceSyntaxReceiver receiver)
            return;

        // Find all classes marked with [AutoInterface]
        var compilation = context.Compilation;

        // TODO Typed
        var autoInterfaceAttribute = compilation.GetTypeByMetadataName("BeGenerate.AutoInterfaceAttribute");

        foreach (var classDeclaration in receiver.CandidateClasses)
        {
            var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var classSymbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

            if (classSymbol == null ||
                !classSymbol.GetAttributes()
                    .Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, autoInterfaceAttribute)))
                continue;

            var interfaceCode = GenerateInterfaceCode(classSymbol);
            context.AddSource($"{classSymbol.Name}.g.cs", SourceText.From(interfaceCode, Encoding.UTF8));
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver to collect candidate classes
        context.RegisterForSyntaxNotifications(() => new AutoInterfaceSyntaxReceiver());
    }

    private string GenerateInterfaceCode(INamedTypeSymbol classSymbol)
    {
        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        var interfaceName = $"I{classSymbol.Name}";

        var methods = classSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(
                m => m is
                {
                    DeclaredAccessibility: Accessibility.Public, IsStatic: false, MethodKind: MethodKind.Ordinary
                })
            .Select(
                m =>
                    $"        {m.ReturnType.ToDisplayString()} {m.Name}({string.Join(", ", m.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"))});");

        var properties = classSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic)
            .Select(p => $"        {p.Type.ToDisplayString()} {p.Name} {{ get; set; }}");

        var interfaceCode = new StringBuilder();
        interfaceCode.AppendLine($"namespace {namespaceName}.Generated");
        interfaceCode.AppendLine("{");
        interfaceCode.AppendLine($"    public interface {interfaceName}");
        interfaceCode.AppendLine("    {");
        interfaceCode.AppendLine(string.Join("\n", properties));
        interfaceCode.AppendLine(string.Join("\n", methods));
        interfaceCode.AppendLine("    }");
        interfaceCode.AppendLine("}");

        return interfaceCode.ToString();
    }

    private sealed class AutoInterfaceSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = [];

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax {AttributeLists.Count: > 0} classDecl)
                CandidateClasses.Add(classDecl);
        }
    }
}
