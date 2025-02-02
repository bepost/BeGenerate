using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace BeGenerate.Generators.AutoInterface;

internal sealed record InterfaceData
{
    public InterfaceData(INamedTypeSymbol type)
    {
        NamespaceName = type.ContainingNamespace.ToDisplayString();
        Name = type.Name;
        Methods =
        [
            ..type.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(
                    m => m is
                    {
                        DeclaredAccessibility: Accessibility.Public, IsStatic: false,
                        MethodKind: MethodKind.Ordinary
                    })
                .Select(
                    m => new MethodData
                    {
                        ReturnType = m.ReturnType.ToDisplayString(),
                        Name = m.Name,
                        Parameters =
                        [
                            ..m.Parameters.Select(
                                p => new ParameterData
                                {
                                    Type = p.Type.ToDisplayString(),
                                    Name = p.Name
                                })
                        ]
                    })
        ];
        Properties =
        [
            ..type.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic)
                .Select(
                    p => new PropertyData
                    {
                        Type = p.Type.ToDisplayString(),
                        Name = p.Name
                    })
        ];
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

        var interfaceCode = new StringBuilder();
        interfaceCode.AppendLine($"namespace {NamespaceName};");
        interfaceCode.AppendLine();
        interfaceCode.AppendLine($"public partial interface {interfaceName}");
        interfaceCode.AppendLine("{");
        interfaceCode.AppendLine(string.Join("\n", Properties.Select(p => $"    {p.Type} {p.Name} {{ get; set; }}")));
        interfaceCode.AppendLine(
            string.Join(
                "\n",
                Methods.Select(
                    m =>
                        $"    {m.ReturnType} {m.Name}({string.Join(", ", m.Parameters.Select(p => $"{p.Type} {p.Name}"))});")));
        interfaceCode.AppendLine("}");

        return interfaceCode.ToString();
    }
}
