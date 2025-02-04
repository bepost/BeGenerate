using System.Collections.Immutable;

namespace BeGenerate.Generators.AutoInterface;

internal sealed record MethodData
{
    public required string Name { get; init; }
    public required ImmutableArray<ParameterData> Parameters { get; init; }
    public required string ReturnType { get; init; }
}
