// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptEscapeStatement</c>.</summary>
[ScriptSyntax("{{ or }}", "{{ or }}")]
public
partial class ScriptEscapeStatement : ScriptStatement, IScriptTerminal
{
    /// <summary><c>ScriptEscapeStatement</c>.</summary>
    public ScriptEscapeStatement()
    {
        Trivias = new ScriptTrivias();
        Indent = string.Empty;
        CanSkipEvaluation = true;
    }
    /// <summary><c>Trivias</c>.</summary>
    public ScriptTrivias Trivias { get; set; }
    /// <summary><c>WhitespaceMode</c>.</summary>
    public ScriptWhitespaceMode WhitespaceMode { get; set; }
    /// <summary><c>Indent</c>.</summary>
    public string Indent { get; set; }
    /// <summary><c>IsEntering</c>.</summary>
    public bool IsEntering { get; set; }
    /// <summary><c>IsClosing</c>.</summary>
    public bool IsClosing => !IsEntering;
    /// <summary><c>EscapeCount</c>.</summary>
    public int EscapeCount { get; set; }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        return null;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        if (IsEntering)
        {
            printer.WriteEnterCode(EscapeCount);
            WriteWhitespaceMode(printer);
        }
        else
        {
            WriteWhitespaceMode(printer);
            printer.WriteExitCode(EscapeCount);
        }
    }

    private void WriteWhitespaceMode(ScriptPrinter printer)
    {
        switch (WhitespaceMode)
        {
            case ScriptWhitespaceMode.None:
                break;
            case ScriptWhitespaceMode.Greedy:
                printer.Write("-");
                break;
            case ScriptWhitespaceMode.NonGreedy:
                printer.Write("~");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

}
