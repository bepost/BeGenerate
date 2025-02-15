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
        Name = symbol.Name.EscapeKeyword();
        HasSetter = symbol.SetMethod is not null;
        HasGetter = symbol.GetMethod is not null;
    }

    private bool HasGetter { get; }
    private bool HasSetter { get; }
    private string Name { get; }
    private string Type { get; }

    public string Emit()
    {
        var cb = new CodeBuilder();
        cb.Append($"{Type} {Name} ")
            .Append("{ ")
            .AppendIf(HasGetter, "get; ")
            .AppendIf(HasSetter, "set; ")
            .Append("}");
        return cb.ToString();
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
