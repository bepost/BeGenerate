using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace BeGenerate.Generators.AutoInterface;

[DebuggerDisplay("{Emit()}")]
internal sealed record ParameterData
{
    private ParameterData(IParameterSymbol symbol)
    {
        Type = symbol.Type.ToDisplayString();
        Name = symbol.Name;
    }

    private string Name { get; }
    private string Type { get; }

    public string Emit()
    {
        return $"{Type} {Name}";
    }

    public static IEnumerable<ParameterData> From(IMethodSymbol symbol)
    {
        return symbol.Parameters.Select(p => new ParameterData(p));
    }
}
