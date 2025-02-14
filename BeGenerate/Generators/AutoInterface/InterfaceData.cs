using System.Collections.Immutable;
using System.Linq;
using System.Text;
using BeGenerate.AutoInterface;
using Microsoft.CodeAnalysis;

namespace BeGenerate.Generators.AutoInterface;

internal sealed record InterfaceData
{
    private const string AutoInterfaceAttributeName = nameof(ExcludeFromInterfaceAttribute);
    private static readonly string AutoInterfaceAttributeNamespace = typeof(ExcludeFromInterfaceAttribute).Namespace!;

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
                         } &&
                         !m.GetAttributes()
                             .Any(
                                 a => a.AttributeClass is {Name : AutoInterfaceAttributeName} ac &&
                                      ac.ContainingNamespace.ToDisplayString() == AutoInterfaceAttributeNamespace))
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
                .Where(
                    p => p is {DeclaredAccessibility: Accessibility.Public, IsStatic: false} &&
                         !p.GetAttributes()
                             .Any(
                                 a => a.AttributeClass is {Name : AutoInterfaceAttributeName} ac &&
                                      ac.ContainingNamespace.ToDisplayString() == AutoInterfaceAttributeNamespace))
                .Select(
                    p => new PropertyData
                    {
                        Type = p.Type.ToDisplayString(),
                        Name = p.Name,
                        HasSetter = p.SetMethod is not null,
                        HasGetter = p.GetMethod is not null
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
        interfaceCode.AppendLine(
            string.Join(
                "\n",
                Properties.Select(
                    p => $"    {p.Type} {p.Name} {{ {(p.HasGetter ? "get; " : "")}{(p.HasSetter ? "set; " : "")}}}")));
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
