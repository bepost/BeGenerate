using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using BeGenerate.AutoInterface;
using BeGenerate.Helpers;
using Microsoft.CodeAnalysis;

namespace BeGenerate.Generators.AutoInterface;

[DebuggerDisplay("{Emit()}")]
internal sealed record MethodData
{
    private MethodData(IMethodSymbol symbol)
    {
        ReturnType = symbol.ReturnType.ToDisplayString();
        Name = symbol.Name.EscapeKeyword();
        Parameters = [..ParameterData.From(symbol)];
        Generics = [..symbol.TypeParameters.Select(p => new GenericTypeParameterData(p))];
    }

    private ImmutableArray<GenericTypeParameterData> Generics { get; }
    private string Name { get; }
    private ImmutableArray<ParameterData> Parameters { get; }
    private string ReturnType { get; }

    public string Emit()
    {
        var cb = new CodeBuilder();
        cb.Append($"{ReturnType} {Name}")
            .ParensIf(Generics.Any(), Generics.Select(g => g.Name), "<>")
            .Parens(Parameters.Select(p => p.Emit()))
            .Join("", Generics.Select(p => p.EmitConstraint()))
            .Append(";");
        return cb.ToString();
    }

    public static IEnumerable<MethodData> From(INamedTypeSymbol type)
    {
        return type.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(ShouldInclude)
            .Select(m => new MethodData(m));
    }

    private static bool ShouldInclude(IMethodSymbol symbol)
    {
        return symbol is
               {
                   DeclaredAccessibility: Accessibility.Public, IsStatic: false,
                   MethodKind: MethodKind.Ordinary
               } &&
               !symbol.HasAttribute<ExcludeFromInterfaceAttribute>();
    }
}
