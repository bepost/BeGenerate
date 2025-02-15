using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BeGenerate.Helpers;
using Microsoft.CodeAnalysis;

namespace BeGenerate.Generators.AutoInterface;

[DebuggerDisplay("{Emit()}")]
internal sealed record ParameterData
{
    private ParameterData(IParameterSymbol symbol)
    {
        Type = symbol.Type.ToDisplayString();
        Name = symbol.Name.EscapeKeyword();
        DefaultValue = symbol.HasExplicitDefaultValue
            ? FormatDefaultValue(symbol.ExplicitDefaultValue, symbol.Type)
            : null;
    }

    public string? DefaultValue { get; }
    private string Name { get; }
    private string Type { get; }

    public string Emit()
    {
        var cb = new CodeBuilder();
        cb.Append($"{Type} {Name}");
        cb.AppendIf(DefaultValue != null, $" = {DefaultValue}");
        return cb.ToString();
    }

    public static IEnumerable<ParameterData> From(IMethodSymbol symbol)
    {
        return symbol.Parameters.Select(p => new ParameterData(p));
    }

    private static string FormatDefaultValue(object? value, ITypeSymbol type)
    {
        if (value == null)
            return "default";

        return type.SpecialType switch
        {
            SpecialType.System_String => $"\"{value}\"",
            SpecialType.System_Char => $"'{value}'",
            SpecialType.System_Boolean => value.ToString()
                                              ?.ToLowerInvariant() ??
                                          "false",
            SpecialType.System_Single => $"{value}f",
            SpecialType.System_Double => $"{value}d",
            SpecialType.System_Decimal => $"{value}m",
            SpecialType.System_Int64 => $"{value}L",
            SpecialType.System_UInt64 => $"{value}UL",
            SpecialType.System_Int32 => $"{value}",
            SpecialType.System_UInt32 => $"{value}U",
            SpecialType.System_Int16 => $"{value}",
            SpecialType.System_UInt16 => $"{value}",
            SpecialType.System_Byte => $"{value}",
            SpecialType.System_SByte => $"{value}",
            _ => value.ToString() ?? "default"
        };
    }
}
