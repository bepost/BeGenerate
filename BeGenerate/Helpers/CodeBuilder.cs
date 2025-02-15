using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace BeGenerate.Helpers;

internal sealed class CodeBuilder
{
    private static readonly Regex DeduplicateSpacesRegex = new(@"\s+");
    private readonly StringBuilder _sb = new();
    private int _indentLevel;
    private bool IsNewLine => _sb.Length == 0 || _sb[^1] == '\n';

    public CodeBuilder AnnotateGeneratedCode()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var name = assembly.GetName()
            .Name;
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                          ?.InformationalVersion ??
                      "1.0.0";
        Line($"""[System.CodeDom.Compiler.GeneratedCode("{name}", "{version}")]""");
        return this;
    }

    public CodeBuilder Append(string text)
    {
        var lines = text.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            var line = DeduplicateSpacesRegex.Replace(lines[i], " ");

            if (IsNewLine && line != "")
                ApplyIndent();

            _sb.Append(line);
            if (i < lines.Length - 1)
                _sb.Append("\n");
        }

        return this;
    }

    public CodeBuilder AppendIf(bool condition, string text)
    {
        if (condition)
            Append(text);
        return this;
    }

    public CodeBuilder Block(Action<CodeBuilder> action)
    {
        Line("{");
        _indentLevel++;
        action(this);
        _indentLevel--;
        Line("}");
        return this;
    }

    public CodeBuilder Block(params IEnumerable<string> lines)
    {
        return Block(
            _ => {
                foreach (var line in lines)
                    Line(line);
            });
    }

    public CodeBuilder Directive(string directive, string value)
    {
        Line($"#{directive} {value}");
        return this;
    }

    public CodeBuilder Indent(Action<CodeBuilder> action)
    {
        Line("{");
        _indentLevel++;
        action(this);
        _indentLevel--;
        Line("}");
        return this;
    }

    public CodeBuilder Join(string separator, IEnumerable<string> texts)
    {
        Append(string.Join(separator, texts));
        return this;
    }

    public CodeBuilder Line(string text = "")
    {
        Append(text + "\n");
        return this;
    }

    public CodeBuilder Parens(IEnumerable<string> items, string parens = "()", string separator = ", ")
    {
        Debug.Assert(parens.Length == 2, "Parens must be exactly 2 characters long");
        Append($"{parens[0]}{string.Join(separator, items)}{parens[1]}");
        return this;
    }

    public CodeBuilder ParensIf(
        bool condition,
        IEnumerable<string> items,
        string parens = "()",
        string separator = ", ")
    {
        if (!condition)
            return this;
        return Parens(items, parens, separator);
    }

    public override string ToString()
    {
        return _sb.ToString();
    }

    private void ApplyIndent()
    {
        _sb.Append(new string(' ', _indentLevel * 4));
    }
}
