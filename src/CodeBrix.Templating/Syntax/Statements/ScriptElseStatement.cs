// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptElseStatement</c>.</summary>
[ScriptSyntax("else statement", "else | else if <expression> ... end|else|else if")]
public
partial class ScriptElseStatement : ScriptConditionStatement
{
    private ScriptKeyword _elseKeyword;
    private ScriptBlockStatement _body;
    /// <summary><c>ScriptElseStatement</c>.</summary>
    public ScriptElseStatement()
    {
        _elseKeyword = ScriptKeyword.Else();
        _elseKeyword.Parent = this;
    }
    /// <summary><c>ElseKeyword</c>.</summary>
    public ScriptKeyword ElseKeyword
    {
        get => _elseKeyword;
        set => ParentToThis(ref  _elseKeyword, value);
    }
    /// <summary><c>Body</c>.</summary>
    public ScriptBlockStatement Body
    {
        get => _body;
        set => ParentToThisNullable(ref _body, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        return Body is null ? null : context.Evaluate(Body);
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.Write(ElseKeyword).ExpectEos();
        printer.Write(Body).ExpectEos();
    }
}
