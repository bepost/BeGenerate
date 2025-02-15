using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace BeGenerate.Generators.AutoInterface;

[DebuggerDisplay("{Name} {EmitConstraint()}")]
internal sealed record GenericTypeParameterData
{
    public GenericTypeParameterData(ITypeParameterSymbol symbol)
    {
        Name = symbol.Name;

        var constraints = ImmutableArray.CreateBuilder<string>();

        if (symbol.HasUnmanagedTypeConstraint)
            constraints.Add("unmanaged");
        if (symbol.HasNotNullConstraint)
            constraints.Add("notnull");
        if (symbol.HasValueTypeConstraint)
            constraints.Add("struct");

        if (symbol.HasReferenceTypeConstraint)
        {
            constraints.Add(
                symbol.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated ? "class?" : "class");
        }

        constraints.AddRange(symbol.ConstraintTypes.Select(t => t.ToDisplayString()));
        if (symbol.HasConstructorConstraint)
            constraints.Add("new()");

        Constraints = constraints.ToImmutable();
    }

    public string Name { get; }
    private ImmutableArray<string> Constraints { get; }

    public string EmitConstraint()
    {
        return Constraints.Any() ? $" where {Name}: {string.Join(", ", Constraints)}" : string.Empty;
    }
}
