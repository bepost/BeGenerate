using System.Reflection;
using Microsoft.CodeAnalysis;

namespace BeGenerate.Helpers;

internal static class DebugExtensions
{
    public static void Debug(this GeneratorExecutionContext context, string message)
    {
        context.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    "DEBUG",
                    "Debug",
                    message,
                    Assembly.GetExecutingAssembly()
                        .GetName()
                        .Name ??
                    "?",
                    DiagnosticSeverity.Warning,
                    true),
                Location.None));
    }
}
