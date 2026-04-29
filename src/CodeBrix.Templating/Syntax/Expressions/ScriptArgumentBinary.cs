// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;

/// <summary>
/// A binary operation argument used with <see cref="ScriptFunctionCall"/>
/// when parsing with scientific mode.
/// </summary>
public
partial class ScriptArgumentBinary : ScriptExpression
{
    private ScriptToken _operatorToken;
    /// <summary><c>Operator</c>.</summary>
    public ScriptBinaryOperator Operator { get; set; }
    /// <summary><c>OperatorToken</c>.</summary>
    public ScriptToken OperatorToken
    {
        get => _operatorToken;
        set => ParentToThisNullable(ref _operatorToken, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        throw new InvalidOperationException("This node should not be evaluated");
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        if (OperatorToken is not null)
        {
            printer.Write(OperatorToken);
        }
        else
        {
            printer.Write(Operator.ToText());
        }
    }
}
