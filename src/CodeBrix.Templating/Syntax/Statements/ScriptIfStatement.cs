// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptIfStatement</c>.</summary>
[ScriptSyntax("if statement", "if <expression> ... end|else|else if")]
public
partial class ScriptIfStatement : ScriptConditionStatement
{
    private ScriptExpression _condition;
    private ScriptBlockStatement _then;
    private ScriptConditionStatement _else;
    private ScriptKeyword _ifKeyword;
    private ScriptKeyword _elseKeyword;
    /// <summary><c>ScriptIfStatement</c>.</summary>
    public ScriptIfStatement()
    {
        _ifKeyword = ScriptKeyword.If();
        _ifKeyword.Parent = this;
    }

    /// <summary>
    /// Only valid for `else if`
    /// </summary>
    public ScriptKeyword ElseKeyword
    {
        get => _elseKeyword;
        set => ParentToThisNullable(ref _elseKeyword, value);
    }
    /// <summary><c>IfKeyword</c>.</summary>
    public ScriptKeyword IfKeyword
    {
        get => _ifKeyword;
        set => ParentToThis(ref _ifKeyword, value);
    }

    /// <summary>
    /// Get or sets the condition of this if statement.
    /// </summary>
    public ScriptExpression Condition
    {
        get => _condition;
        set => ParentToThisNullable(ref _condition, value);
    }
    /// <summary><c>Then</c>.</summary>
    public ScriptBlockStatement Then
    {
        get => _then;
        set => ParentToThisNullable(ref _then, value);
    }
    /// <summary><c>Else</c>.</summary>
    public ScriptConditionStatement Else
    {
        get => _else;
        set => ParentToThisNullable(ref _else, value);
    }
    /// <summary><c>IsElseIf</c>.</summary>
    public bool IsElseIf => ElseKeyword is not null;
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        if (Condition is null)
        {
            return null;
        }

        var conditionValue = context.ToBool(Condition.Span, context.Evaluate(Condition));
        return conditionValue
            ? Then is null ? null : context.Evaluate(Then)
            : Else is null ? null : context.Evaluate(Else);
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        if (IsElseIf)
        {
            printer.Write(ElseKeyword).ExpectSpace();
        }
        printer.Write(IfKeyword).ExpectSpace();
        printer.Write(Condition);
        printer.ExpectEos();
        printer.Write(Then);
        printer.Write(Else);
    }
}
