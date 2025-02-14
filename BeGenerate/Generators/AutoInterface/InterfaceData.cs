using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using BeGenerate.Helpers;
using Microsoft.CodeAnalysis;

namespace BeGenerate.Generators.AutoInterface;

[DebuggerDisplay("interface {InterfaceName}")]
internal sealed record InterfaceData
{
    public InterfaceData(INamedTypeSymbol type)
    {
        NamespaceName = type.ContainingNamespace.ToDisplayString();
        Name = type.Name;
        Methods = [..MethodData.From(type)];
        Properties = [..PropertyData.From(type)];
    }

    public string Filename => $"{InterfaceName}.g.cs";
    private string InterfaceName => $"I{Name}";
    private ImmutableArray<MethodData> Methods { get; }
    private string Name { get; }
    private string NamespaceName { get; }
    private ImmutableArray<PropertyData> Properties { get; }

    public string Emit()
    {
        var interfaceName = $"I{Name}";

        var code = new CodeBuilder();
        code.Line($"namespace {NamespaceName};")
            .Line()
            .Line($"public partial interface {interfaceName}")
            .Block([..Properties.Select(p => p.Emit()), ..Methods.Select(m => m.Emit())]);
        return code.ToString();
    }
}
