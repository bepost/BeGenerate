using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
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

        var code = new StringBuilder();
        code.AppendLine($"namespace {NamespaceName};");
        code.AppendLine();
        code.AppendLine($"public partial interface {interfaceName}");
        code.AppendLine("{");
        foreach (var property in Properties)
            code.AppendLine($"    {property.Emit()}");
        foreach (var method in Methods)
            code.AppendLine($"    {method.Emit()}");
        code.AppendLine("}");

        return code.ToString();
    }
}
