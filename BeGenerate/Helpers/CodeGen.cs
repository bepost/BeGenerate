using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BeGenerate.Helpers;

internal sealed class CodeBuilder
{
    private readonly StringBuilder _complete = new();
    private readonly StringBuilder _current = new();
    private int _indent;
    private bool _lastWasEmpty = true;

    public void Append(object? source)
    {
        if (source is null)
            return;

        Append(source.ToString());
    }

    public void Append(params IEnumerable<object?> sources)
    {
        foreach (var source in sources)
            Append(source);
    }

    public void Append(string? text)
    {
        if (text == null)
            return;
        foreach (var c in text)
            Append(c);
    }

    public void Append(char c)
    {
        switch (c)
        {
            case '\n' when _lastWasEmpty && _current.Length == 0:
                break;
            case '\n' when _current.Length == 0:
                _lastWasEmpty = true;
                _complete.Append('\n');
                break;
            case '\n':
                _lastWasEmpty = false;
                _complete.Append(
                    _current.ToString()
                        .TrimEnd());
                _complete.Append('\n');
                _current.Clear();
                break;
            case '\t' when _current.Length == 0:
                if (_current.Length == 0)
                    _current.Append(' ', _indent * 4);
                _current.Append("    ");
                break;
            default:
                if (_current.Length == 0)
                    _current.Append(' ', _indent * 4);
                _current.Append(c);
                break;
        }
    }

    public void Block(Action action)
    {
        Line("{");
        _indent++;
        action();
        _indent--;
        Debug.Assert(_indent >= 0);
        Line("}");
    }

    public void Indent(Action action)
    {
        _indent++;
        action();
        _indent--;
        Debug.Assert(_indent >= 0);
    }

    public void Join(string separator, params IEnumerable<object?> sources)
    {
        var first = true;

        foreach (var source in sources)
        {
            if (!first)
                Append(separator);
            first = false;
            Append(source);
        }
    }

    public void Line(string? line = "")
    {
        if (line == null)
            return;

        Append(line);
        Append('\n');
    }

    public void Line(object? source)
    {
        if (source is null)
            return;

        Line(source.ToString());
    }

    public void Line(params IEnumerable<object?> sources)
    {
        var anything = false;
        foreach (var source in sources)
        {
            if (source is null)
                continue;
            anything = true;
            Append(source.ToString());
        }

        if (anything)
            Append('\n');
    }

    public override string ToString()
    {
        if (_current.Length > 0 || !_lastWasEmpty)
        {
            return _complete +
                   _current.ToString()
                       .TrimEnd();
        }

        return _complete.ToString();
    }
}
