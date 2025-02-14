using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BeGenerate.AutoInterface;
using BeGenerate.Helpers;
using Microsoft.CodeAnalysis;

namespace BeGenerate.Generators.AutoInterface;

[DebuggerDisplay("{Emit()}")]
internal sealed record PropertyData
{
    private PropertyData(IPropertySymbol symbol)
    {
        Type = symbol.Type.ToDisplayString();
        Name = symbol.Name;
        HasSetter = symbol.SetMethod is not null;
        HasGetter = symbol.GetMethod is not null;
    }

    private bool HasGetter { get; }
    private bool HasSetter { get; }
    private string Name { get; }
    private string Type { get; }

    public string Emit()
    {
        return $"{Type} {Name} {{ {(HasGetter ? "get; " : "")}{(HasSetter ? "set; " : "")}}}";
    }

    public static IEnumerable<PropertyData> From(INamedTypeSymbol type)
    {
        return type.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(ShouldInclude)
            .Select(p => new PropertyData(p));
    }

    private static bool ShouldInclude(IPropertySymbol symbol)
    {
        return symbol is {DeclaredAccessibility: Accessibility.Public, IsStatic: false} &&
               !symbol.HasAttribute<ExcludeFromInterfaceAttribute>();
    }
}
