// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptEndStatement</c>.</summary>
[ScriptSyntax("end statement", "end")]
public
partial class ScriptEndStatement : ScriptStatement
{
    private ScriptKeyword _endKeyword = ScriptKeyword.End();
    /// <summary><c>ScriptEndStatement</c>.</summary>
    public ScriptEndStatement()
    {
        _endKeyword.Parent = this;
        CanSkipEvaluation = true;
        ExpectEos = true;
    }
    /// <summary><c>EndKeyword</c>.</summary>
    public ScriptKeyword EndKeyword
    {
        get => _endKeyword;
        set => ParentToThis(ref _endKeyword, value);
    }
    /// <summary><c>ExpectEos</c>.</summary>
    public bool ExpectEos { get; set; }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        return null;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.Write(EndKeyword);
        if (ExpectEos)
        {
            printer.ExpectEos();
        }
    }
}
