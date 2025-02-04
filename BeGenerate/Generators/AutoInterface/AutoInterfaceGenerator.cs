using System.Text;
using BeGenerate.AutoInterface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BeGenerate.Generators.AutoInterface;

/// <summary> Generates interfaces from classes marked with <see cref="AutoInterfaceAttribute" />. </summary>
[Generator]
public sealed class AutoInterfaceGenerator : IIncrementalGenerator
{
    private static readonly string AutoInterfaceAttributeFullName = typeof(AutoInterfaceAttribute).FullName!;

    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
    {
        var infos = context.SyntaxProvider.ForAttributeWithMetadataName(
            AutoInterfaceAttributeFullName,
            (node, _) => node is ClassDeclarationSyntax,
            static (ctx, _) => new InterfaceData((INamedTypeSymbol) ctx.TargetSymbol));

        context.RegisterSourceOutput(
            infos,
            static (context, data) => {
                context.AddSource(data.Filename, SourceText.From(data.Emit(), Encoding.UTF8));
            });
    }
}
