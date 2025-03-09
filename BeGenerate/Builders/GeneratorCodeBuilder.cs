﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BeGenerate.Builders;

internal class GeneratorCodeBuilder : CodeBuilder
{
    protected GeneratorCodeBuilder(SemanticModel model)
    {
        Model = model;
    }

    protected SemanticModel Model { get; }

    protected void EmitAutoGeneratedPreamble()
    {
        Line(
            """
            // <auto-generated>
            // This code was generated by a tool.
            // </auto-generated>
            """);
        Line();
    }

    protected void EmitGeneratedCodeAttribute()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var name = assembly.GetName()
            .Name;
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                          ?.InformationalVersion ??
                      "1.0.0";
        Line(
            $"""
             [System.CodeDom.Compiler.GeneratedCode("{name}", "{version}")]
             """);
    }

    protected void EmitLocationFor(SyntaxNode? node)
    {
        if (node is null)
        {
            Line("#line default");
            return;
        }

        var filePath = node.SyntaxTree.FilePath;
        var projectDir = Path.GetDirectoryName(
            Model.Compilation.SyntaxTrees.First()
                .FilePath);
        var path = projectDir is null ? filePath : Path.GetRelativePath(projectDir, filePath);

        Line(
            $"""
             #line {node.GetLocation().GetLineSpan().StartLinePosition.Line + 1} "{path}"
             """);
    }

    protected void EmitXmlDocumentationFor(CSharpSyntaxNode node)
    {
        var leadingTrivia = node.GetLeadingTrivia();
        var xmlDocs = leadingTrivia.Select(trivia => trivia.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();

        // If found, return the string value of the XML
        if (xmlDocs is null)
            return;

        var docs = xmlDocs.ToFullString();
        var lines = docs.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim());
        var cleaned = string.Join('\n', lines);

        Line(cleaned);
    }
}
