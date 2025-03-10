﻿using System.Diagnostics;
using System.Linq;
using System.Threading;
using BeGenerate.AutoInterface;
using BeGenerate.Builders;
using BeGenerate.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BeGenerate.Generators.AutoInterface;

internal sealed class AutoInterfaceCodeBuilder : GeneratorCodeBuilder
{
    private readonly AutoInterfaceAttribute _attribute;
    private readonly CancellationToken _cancellationToken;
    private readonly string _fullName;
    private readonly string _name;

    private AutoInterfaceCodeBuilder(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken) :
        base(context.SemanticModel)
    {
        _cancellationToken = cancellationToken;

        _attribute = context.TargetSymbol.GetAttributeInstances<AutoInterfaceAttribute>()
            .First();

        _name = _attribute.Name ?? $"I{((INamedTypeSymbol) context.TargetSymbol).Name}";
        _fullName = $"{((INamedTypeSymbol) context.TargetSymbol).ContainingNamespace?.ToDisplayString()}.{_name}";
    }

    public static SourceFile Build(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var builder = new AutoInterfaceCodeBuilder(context, cancellationToken);

        var code = builder.Emit((ClassDeclarationSyntax) context.TargetNode, context.TargetSymbol);

        return new SourceFile
        {
            Filename = $"{builder._fullName}.g.cs",
            Code = code
        };
    }

    private string Emit(ClassDeclarationSyntax node, ISymbol symbol)
    {
        EmitAutoGeneratedPreamble();
        EmitNullability(node);
        EmitUsings(node);
        EmitNamespaceDeclaration(symbol);

        EmitInterface(node, symbol);
        return ToString();
    }

    private void EmitInterface(ClassDeclarationSyntax node, ISymbol symbol)
    {
        EmitXmlDocumentationFor(node);
        EmitLocationFor(node);
        // TODO: Add attributes
        Line("[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
        EmitGeneratedCodeAttribute();
        switch (_attribute.Accessibility)
        {
            case InterfaceAccessibility.Public:
                Append("public ");
                break;
            case InterfaceAccessibility.Internal:
                Append("internal ");
                break;
            case InterfaceAccessibility.None:
                break;
            default:
                Debug.Fail($"Unknown accessibility {_attribute.Accessibility}");
                break;
        }

        Append("partial interface ", _name, node.TypeParameterList);

        var inherits = symbol.GetAttributes()
            .Where(
                a => a.AttributeClass is not null &&
                     a.AttributeClass.IsGenericType &&
                     a.AttributeClass.Name == nameof(ImplementsAttribute<object>))
            .SelectMany(a => a.AttributeClass!.TypeArguments)
            .ToArray();

        if (inherits.Any())
        {
            Append(": ");
            Join(", ", inherits.Select(x => x.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
        }

        node.ConstraintClauses.ForEach(c => Append(" ", c));

        Line();
        Block(
            () => {
                EmitInterfaceProperties(node);
                EmitInterfaceMethods(node);
            });
        EmitLocationFor(null);
    }

    private void EmitInterfaceMethods(ClassDeclarationSyntax node)
    {
        node.Members.OfType<MethodDeclarationSyntax>()
            .Select(x => (Node: x, Symbol: Model.GetDeclaredSymbol(x)!))
            .Where(x => !x.Symbol.HasAttribute<ExcludeFromInterfaceAttribute>())
            .Where(
                x => x.Symbol.DeclaredAccessibility == Accessibility.Public ||
                     x.Node.ExplicitInterfaceSpecifier?.Name.ToString() == _name)
            .ForEach(x => EmitMethod(x.Node));
    }

    private void EmitInterfaceProperties(ClassDeclarationSyntax node)
    {
        node.Members.OfType<PropertyDeclarationSyntax>()
            .Select(x => (Node: x, Symbol: Model.GetDeclaredSymbol(x)!))
            .Where(x => !x.Symbol.HasAttribute<ExcludeFromInterfaceAttribute>())
            .Where(
                x => x.Symbol.DeclaredAccessibility == Accessibility.Public ||
                     x.Node.ExplicitInterfaceSpecifier?.Name.ToString() == _name)
            .ForEach(x => EmitProperty(x.Node, x.Symbol));
    }

    private void EmitMethod(MethodDeclarationSyntax node)
    {
        EmitXmlDocumentationFor(node);
        Line(node.AttributeLists);
        Append(node.ReturnType, " ", node.Identifier.Text);
        Append(node.TypeParameterList);
        Append("(");
        Join(", ", node.ParameterList.Parameters);
        Append(")");
        node.ConstraintClauses.ForEach(x => Append(" ", x));
        Line(";");
        Line();
    }

    private void EmitNamespaceDeclaration(ISymbol symbol)
    {
        if (symbol.ContainingNamespace is not null)
            Line("namespace ", symbol.ContainingNamespace.ToDisplayString(), ";");
        Line();
    }

    private void EmitNullability(ClassDeclarationSyntax node)
    {
        Line(
            Model.GetNullableContext(node.SpanStart) switch
            {
                NullableContext.Disabled => "#nullable disable",
                _ => "#nullable enable"
            });
        Line();
    }

    private void EmitProperty(PropertyDeclarationSyntax node, IPropertySymbol symbol)
    {
        EmitXmlDocumentationFor(node);
        Line(node.AttributeLists);
        Append(node.Type, " ", node.Identifier.Text);
        Append(" {");

        if (symbol.GetMethod is not null && !symbol.GetMethod.HasAttribute<ExcludeFromInterfaceAttribute>())
        {
            node.AccessorList?.Accessors.Where(a => a.IsKind(SyntaxKind.GetAccessorDeclaration))
                .Select(a => a.AttributeLists)
                .Where(a => a.Any())
                .ForEach(a => Append(" ", a));
            Append(" get;");
        }

        if (symbol.SetMethod is not null && !symbol.SetMethod.HasAttribute<ExcludeFromInterfaceAttribute>())
        {
            node.AccessorList?.Accessors.Where(a => a.IsKind(SyntaxKind.SetAccessorDeclaration))
                .Select(a => a.AttributeLists)
                .Where(a => a.Any())
                .ForEach(a => Append(" ", a));
            Append(" set;");
        }

        Line(" }");
        Line();
    }

    private void EmitUsings(ClassDeclarationSyntax node)
    {
        node.Ancestors()
            .OfType<NamespaceDeclarationSyntax>()
            .SelectMany(s => s.Usings)
            .Concat((node.SyntaxTree.GetRoot(_cancellationToken) as CompilationUnitSyntax)?.Usings ?? [])
            .Distinct()
            .Select(x => x.ToString())
            .Order()
            .ForEach(Line);
        Line();
    }
}
