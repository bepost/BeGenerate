using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace BeGenerate.Helpers;

[ExcludeFromCodeCoverage]
internal static class TypeMatchExtensions
{
    public static IEnumerable<T> GetAttributeInstances<T>(this INamedTypeSymbol symbol)
        where T : Attribute
    {
        var attributes = symbol.GetAttributes()
            .Where(attr => attr.AttributeClass?.ToDisplayString() == typeof(T).FullName);

        foreach (var attributeData in attributes)
        {
            var constructorArgs = attributeData.ConstructorArguments.Select(arg => arg.Value)
                .ToArray();

            var instance = Activator.CreateInstance(typeof(T), constructorArgs);
            if (instance is not T attribute)
                continue;

            foreach (var namedArg in attributeData.NamedArguments)
            {
                var property = typeof(T).GetProperty(namedArg.Key);
                property?.SetValue(attribute, namedArg.Value.Value);
            }

            yield return attribute;
        }
    }

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
