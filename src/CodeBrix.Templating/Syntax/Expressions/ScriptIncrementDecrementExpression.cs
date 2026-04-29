// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptIncrementDecrementExpression</c>.</summary>
[ScriptSyntax("increment/decrement expression", "<operator> <expression> or <expression> <operator>")]
public
partial class ScriptIncrementDecrementExpression : ScriptUnaryExpression
{
    /// <summary><c>Post</c>.</summary>
    public bool Post { get; set; }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        if (Right is null)
        {
            return null;
        }

        var increment = this.Operator == ScriptUnaryOperator.Increment ? 1 : -1;
        var value = Evaluate(context, this.Right.Span, ScriptUnaryOperator.Plus, context.Evaluate(this.Right));
        var incrementedValue = ScriptBinaryExpression.Evaluate(context, this.Right.Span, ScriptBinaryOperator.Add, value, increment);
        context.SetValue(Right, incrementedValue);
        return Post ? value : incrementedValue;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        if (Post)
        {
            printer.Write(Right);
            PrintOperator(printer);
        }
        else
        {
            PrintOperator(printer);
            printer.Write(Right);
        }

    }
    private void PrintOperator(ScriptPrinter printer)
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
