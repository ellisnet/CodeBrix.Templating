// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptNestedExpression</c>.</summary>
[ScriptSyntax("nested expression", "(<expression>)")]
public
partial class ScriptNestedExpression : ScriptExpression, IScriptVariablePath
{
    private ScriptExpression _expression;
    private ScriptToken _openParen = ScriptToken.OpenParen();
    private ScriptToken _closeParen = ScriptToken.CloseParen();
    /// <summary><c>ScriptNestedExpression</c>.</summary>
    public ScriptNestedExpression()
    {
        _openParen.Parent = this;
        _closeParen.Parent = this;
    }
    /// <summary><c>ScriptNestedExpression</c>.</summary>
    public ScriptNestedExpression(ScriptExpression expression) : this()
    {
        Expression = expression;
    }
    /// <summary><c>Wrap</c>.</summary>
    public static ScriptNestedExpression Wrap(ScriptExpression expression, bool transferTrivia = false)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var nested = new ScriptNestedExpression()
            {
                Span = expression.Span,
                Expression = expression
            };

        if (!transferTrivia) return nested;

        var firstTerminal = expression.FindFirstTerminal();
        firstTerminal?.MoveLeadingTriviasTo(nested.OpenParen);

        var lastTerminal = expression.FindLastTerminal();
        lastTerminal?.MoveTrailingTriviasTo(nested.CloseParen, true);

        return nested;
    }
    /// <summary><c>OpenParen</c>.</summary>
    public ScriptToken OpenParen
    {
        get => _openParen;
        set => ParentToThis(ref _openParen, value);
    }
    /// <summary><c>Expression</c>.</summary>
    public ScriptExpression Expression
    {
        get => _expression;
        set => ParentToThisNullable(ref _expression, value);
    }
    /// <summary><c>CloseParen</c>.</summary>
    public ScriptToken CloseParen
    {
        get => _closeParen;
        set => ParentToThis(ref _closeParen, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        if (Expression is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid nested expression. Inner expression is required.");
        }

        // A nested expression will reset the pipe arguments for the group
        context.PushPipeArguments();
        try
        {
            return context.GetValue(this);
        }
        finally
        {
            if (context.CurrentPipeArguments is not null)
            {
                context.PopPipeArguments();
            }
        }
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.Write(OpenParen);
        if (Expression is not null)
        {
            printer.Write(Expression);
        }
        printer.Write(CloseParen);
    }
    /// <summary><c>GetValue</c>.</summary>
    public object GetValue(TemplateContext context)
    {
        if (Expression is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid nested expression. Inner expression is required.");
        }
        return context.Evaluate(Expression);
    }
    /// <summary><c>SetValue</c>.</summary>
    public void SetValue(TemplateContext context, object valueToSet)
    {
        if (Expression is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid nested expression. Inner expression is required.");
        }
        context.SetValue(Expression, valueToSet);
    }
    /// <summary><c>GetFirstPath</c>.</summary>
    public string GetFirstPath()
    {
        return (Expression as IScriptVariablePath)?.GetFirstPath() ?? string.Empty;
    }
}
