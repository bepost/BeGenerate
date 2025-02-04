namespace BeGenerate.Generators.AutoInterface;

internal sealed record ParameterData
{
    public required string Name { get; init; }
    public required string Type { get; init; }
}
