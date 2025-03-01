namespace BeGenerate;

internal sealed record SourceFile
{
    public required string Code { get; init; }
    public required string Filename { get; init; }
}
