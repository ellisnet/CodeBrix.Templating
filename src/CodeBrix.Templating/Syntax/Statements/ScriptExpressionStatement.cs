// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptExpressionStatement</c>.</summary>
[ScriptSyntax("expression statement", "<expression>")]
public
partial class ScriptExpressionStatement : ScriptStatement
{
    private ScriptExpression _expression;
    /// <summary><c>Expression</c>.</summary>
    public ScriptExpression Expression
    {
        get => _expression;
        set
        {
            ParentToThisNullable(ref _expression, value);
            CanOutput = value is not ScriptAssignExpression;
        }
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        if (Expression is null)
        {
            return null;
        }

        var result = context.Evaluate(Expression);
        // This code is necessary for wrap to work
        var codeDelegate = result as ScriptNode;
        if (codeDelegate is not null)
        {
            return context.Evaluate(codeDelegate);
        }
        return result;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.Write(Expression).ExpectEos();
    }
}
