using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace BeGenerate.Helpers;

[ExcludeFromCodeCoverage]
internal static class TypeMatchExtensions
{
    public static bool HasAttribute<T>(this ISymbol? symbol)
    {
        if (symbol is null)
            return false;

        return symbol.GetAttributes()
            .Any(a => a.Is<T>());
    }

    public static bool Is<T>(this AttributeData data)
    {
        return data.AttributeClass is not null &&
               data.AttributeClass.Name == typeof(T).Name &&
               data.AttributeClass.ContainingNamespace.ToDisplayString() == typeof(T).Namespace!;
    }
}
