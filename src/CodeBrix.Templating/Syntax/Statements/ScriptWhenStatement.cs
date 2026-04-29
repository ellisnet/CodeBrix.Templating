// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Text;
using CodeBrix.Templating.Functions;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptWhenStatement</c>.</summary>
[ScriptSyntax("when statement", "when <expression> ... end|when|else")]
public
partial class ScriptWhenStatement : ScriptConditionStatement
{
    private ScriptKeyword _whenKeyword;
    private ScriptList<ScriptExpression> _values;
    private ScriptBlockStatement _body;
    private ScriptConditionStatement _next;
    /// <summary><c>ScriptWhenStatement</c>.</summary>
    public ScriptWhenStatement()
    {
        _whenKeyword = ScriptKeyword.When();
        _whenKeyword.Parent = this;
        _values = new ScriptList<ScriptExpression>();
        _values.Parent = this;
    }
    /// <summary><c>WhenKeyword</c>.</summary>
    public ScriptKeyword WhenKeyword
    {
        get => _whenKeyword;
        set => ParentToThis(ref _whenKeyword, value);
    }

    /// <summary>
    /// Get or sets the value used to check against When clause.
    /// </summary>
    public ScriptList<ScriptExpression> Values
    {
        get => _values;
        set => ParentToThis(ref _values, value);
    }
    /// <summary><c>Body</c>.</summary>
    public ScriptBlockStatement Body
    {
        get => _body;
        set => ParentToThisNullable(ref _body, value);
    }
    /// <summary><c>Next</c>.</summary>
    public ScriptConditionStatement Next
    {
        get => _next;
        set => ParentToThisNullable(ref _next, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        var caseValue = context.PeekCase();
        foreach (var value in Values)
        {
            var whenValue = context.Evaluate(value);
            var result = ScriptBinaryExpression.Evaluate(context, Span, ScriptBinaryOperator.CompareEqual, caseValue, whenValue);
            if (result is bool booleanResult && booleanResult)
            {
                return Body is null ? null : context.Evaluate(Body);
            }

        }
        return Next is null ? null : context.Evaluate(Next);
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.Write(WhenKeyword).ExpectSpace();
        printer.WriteListWithCommas(Values);
        printer.ExpectEos();
        printer.Write(Body).ExpectEos();
        printer.Write(Next);
    }
}
