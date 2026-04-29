// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using CodeBrix.Templating.Functions;
using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Runtime;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptConditionalExpression</c>.</summary>
[ScriptSyntax("conditional expression", "<condition> ? <then_value> : <else_value>")]
public
partial class ScriptConditionalExpression : ScriptExpression
{
    private ScriptExpression _condition;
    private ScriptToken _questionToken;
    private ScriptExpression _thenValue;
    private ScriptToken _colonToken;
    private ScriptExpression _elseValue;
    /// <summary><c>ScriptConditionalExpression</c>.</summary>
    public ScriptConditionalExpression()
    {
        _questionToken = ScriptToken.Question();
        _questionToken.Parent = this;
        _colonToken = ScriptToken.Colon();
        _colonToken.Parent = this;
    }
    /// <summary><c>Condition</c>.</summary>
    public ScriptExpression Condition
    {
        get => _condition;
        set => ParentToThisNullable(ref _condition, value);
    }
    /// <summary><c>QuestionToken</c>.</summary>
    public ScriptToken QuestionToken
    {
        get => _questionToken;
        set => ParentToThis(ref _questionToken, value);
    }
    /// <summary><c>ThenValue</c>.</summary>
    public ScriptExpression ThenValue
    {
        get => _thenValue;
        set => ParentToThisNullable(ref _thenValue, value);
    }
    /// <summary><c>ColonToken</c>.</summary>
    public ScriptToken ColonToken
    {
        get => _colonToken;
        set => ParentToThis(ref _colonToken, value);
    }
    /// <summary><c>ElseValue</c>.</summary>
    public ScriptExpression ElseValue
    {
        get => _elseValue;
        set => ParentToThisNullable(ref _elseValue, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        if (Condition is null || ThenValue is null || ElseValue is null)
        {
            return null;
        }

        var condValue = context.Evaluate(Condition);
        var result = context.ToBool(Condition.Span, condValue);
        return context.Evaluate(result ? ThenValue : ElseValue);
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.Write(Condition);
        printer.Write(QuestionToken);
        printer.Write(ThenValue);
        printer.Write(ColonToken);
        printer.Write(ElseValue);
    }
    /// <summary><c>CanHaveLeadingTrivia</c>.</summary>
    public override bool CanHaveLeadingTrivia()
    {
        return false;
    }
}
