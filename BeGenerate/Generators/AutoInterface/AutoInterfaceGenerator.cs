using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BeGenerate.Generators.AutoInterface;

[Generator]
public sealed class AutoInterfaceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var infos = context.SyntaxProvider.ForAttributeWithMetadataName(
            typeof(AutoInterfaceAttribute).FullName!,
            (node, _) => node is ClassDeclarationSyntax,
            static (ctx, _) => new InterfaceData((INamedTypeSymbol) ctx.TargetSymbol));

        context.RegisterSourceOutput(
            infos,
            static (context, data) => {
                context.AddSource(data.Filename, SourceText.From(data.Emit(), Encoding.UTF8));
            });
    }
}
