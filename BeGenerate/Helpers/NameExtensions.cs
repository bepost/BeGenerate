using Microsoft.CodeAnalysis.CSharp;

namespace BeGenerate.Helpers;

internal static class NameExtensions
{
    public static string EscapeKeyword(this string name)
    {
        return SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ? $"@{name}" : name;
    }
}
