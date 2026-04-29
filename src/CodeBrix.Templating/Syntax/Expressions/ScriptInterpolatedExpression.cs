// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptInterpolatedExpression</c>.</summary>
[ScriptSyntax("interpolated string expression", "$\"string{<expression>}string\"")]
public
partial class ScriptInterpolatedExpression : ScriptExpression
{
    private ScriptExpression _expression;
    private ScriptToken _openBrace;
    private ScriptToken _closeBrace;
    /// <summary><c>ScriptInterpolatedExpression</c>.</summary>
    public ScriptInterpolatedExpression()
    {
        _openBrace = ScriptToken.OpenInterpBrace();
        _openBrace.Parent = this;
        _closeBrace = ScriptToken.CloseInterpBrace();
        _closeBrace.Parent = this;
    }
    /// <summary><c>ScriptInterpolatedExpression</c>.</summary>
    public ScriptInterpolatedExpression(ScriptExpression expression) : this()
    {
        Expression = expression;
    }
    /// <summary><c>OpenBrace</c>.</summary>
    public ScriptToken OpenBrace
    {
        get => _openBrace;
        set => ParentToThis(ref _openBrace, value);
    }
    /// <summary><c>Expression</c>.</summary>
    public ScriptExpression Expression
    {
        get => _expression;
        set => ParentToThisNullable(ref _expression, value);
    }
    /// <summary><c>CloseBrace</c>.</summary>
    public ScriptToken CloseBrace
    {
        get => _closeBrace;
        set => ParentToThis(ref _closeBrace, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        if (Expression is null)
        {
            return null;
        }

        // A nested expression will reset the pipe arguments for the group
        context.PushPipeArguments();
        try
        {
            return context.Evaluate(Expression);
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
        printer.Write(OpenBrace);
        printer.Write(Expression);
        printer.Write(CloseBrace);
    }
}
