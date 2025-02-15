using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using BeGenerate.Helpers;
using Microsoft.CodeAnalysis;

namespace BeGenerate.Generators.AutoInterface;

[DebuggerDisplay("interface {InterfaceName}")]
internal sealed record InterfaceData
{
    public InterfaceData(INamedTypeSymbol symbol)
    {
        NamespaceName = symbol.ContainingNamespace.ToDisplayString();
        Name = symbol.Name.EscapeKeyword();
        Methods = [..MethodData.From(symbol)];
        Properties = [..PropertyData.From(symbol)];
        Generics = [..symbol.TypeParameters.Select(p => new GenericTypeParameterData(p))];
    }

    public string Filename => $"{InterfaceName}.g.cs";
    private ImmutableArray<GenericTypeParameterData> Generics { get; }
    private string InterfaceName => $"I{Name}";
    private ImmutableArray<MethodData> Methods { get; }
    private string Name { get; }
    private string NamespaceName { get; }
    private ImmutableArray<PropertyData> Properties { get; }

    public string Emit()
    {
        var interfaceName = $"I{Name}";

        var code = new CodeBuilder();
        code.Directive("nullable", "enable")
            .Line($"namespace {NamespaceName};")
            .Line()
            .AnnotateGeneratedCode()
            .Append($"public partial interface {interfaceName}")
            .ParensIf(Generics.Any(), Generics.Select(g => g.Name), "<>")
            .Join("", Generics.Select(g => g.EmitConstraint()))
            .Line()
            .Block([..Properties.Select(p => p.Emit()), ..Methods.Select(m => m.Emit())]);
        return code.ToString();
    }
}
