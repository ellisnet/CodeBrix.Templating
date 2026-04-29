// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptVerbatim</c>.</summary>
public
abstract class ScriptVerbatim : ScriptNode, IScriptTerminal
{
    /// <summary><c>ScriptVerbatim</c>.</summary>
    protected ScriptVerbatim()
    {
        Trivias = new ScriptTrivias();
        Value = string.Empty;
    }
    /// <summary><c>ScriptVerbatim</c>.</summary>
    protected ScriptVerbatim(string value)
    {
        Trivias = new ScriptTrivias();
        Value = value;
    }
    /// <summary><c>Trivias</c>.</summary>
    public ScriptTrivias Trivias { get; set; }
    /// <summary><c>Value</c>.</summary>
    public string Value { get; set; }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        // Nothing to evaluate
        return null;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.Write(Value);
    }

}
