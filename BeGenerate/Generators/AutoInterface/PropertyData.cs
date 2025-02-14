namespace BeGenerate.Generators.AutoInterface;

internal sealed record PropertyData
{
    public required bool HasGetter { get; init; }
    public required bool HasSetter { get; init; }
    public required string Name { get; init; }
    public required string Type { get; init; }
}
