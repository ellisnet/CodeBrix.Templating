// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using CodeBrix.Templating.Helpers;
using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Runtime;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptUnaryExpression</c>.</summary>
[ScriptSyntax("unary expression", "<operator> <expression>")]
public
partial class ScriptUnaryExpression : ScriptExpression
{
    private ScriptToken _operatorToken;
    private ScriptExpression _right;
    /// <summary><c>Operator</c>.</summary>
    public ScriptUnaryOperator Operator { get; set; }
    /// <summary><c>OperatorToken</c>.</summary>
    public ScriptToken OperatorToken
    {
        get => _operatorToken;
        set => ParentToThisNullable(ref _operatorToken, value);
    }
    /// <summary><c>OperatorAsText</c>.</summary>
    public string OperatorAsText => OperatorToken?.Value ?? Operator.ToText();
    /// <summary><c>Right</c>.</summary>
    public ScriptExpression Right
    {
        get => _right;
        set => ParentToThisNullable(ref _right, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        if (Operator == ScriptUnaryOperator.FunctionAlias)
        {
            return context.Evaluate(Right, true);
        }

        if (Right is null)
        {
            return Evaluate(context, Span, Operator, null);
        }

        var value = context.Evaluate(Right);

        return Evaluate(context, Right.Span, Operator, value);
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
        printer.Write(Right);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public static object Evaluate(TemplateContext context, SourceSpan span, ScriptUnaryOperator op, object value)
    {
        if (value is IScriptCustomUnaryOperation customUnary)
        {
            if (customUnary.TryEvaluate(context, span, op, value, out var result))
            {
                return result;
            }
        }
        else
        {
            switch (op)
            {
                case ScriptUnaryOperator.Not:
                {
                    if (context.UseScientific)
                    {
                        if (!(value is bool))
                        {
                            throw new ScriptRuntimeException(span, $"Expecting a boolean instead of {context.GetTypeName(value)} value: {value}");
                        }

                        return !(bool)value;
                    }
                    else
                    {
                        return !context.ToBool(span, value);
                    }
                }
                case ScriptUnaryOperator.Negate:
                case ScriptUnaryOperator.Plus:
                {
                    bool negate = op == ScriptUnaryOperator.Negate;

                    if (value is not null)
                    {
                        if (value is int)
                        {
                            return negate ? -((int)value) : value;
                        }
                        else if (value is double)
                        {
                            return negate ? -((double)value) : value;
                        }
                        else if (value is float)
                        {
                            return negate ? -((float)value) : value;
                        }
                        else if (value is long)
                        {
                            return negate ? -((long)value) : value;
                        }
                        else if (value is decimal)
                        {
                            return negate ? -((decimal)value) : value;
                        }
                        else if (value is BigInteger)
                        {
                            return negate ? -((BigInteger)value) : value;
                        }
                        else
                        {
                            throw new ScriptRuntimeException(span, $"Unexpected value `{value} / Type: {context.GetTypeName(value)}`. Cannot negate(-)/positive(+) a non-numeric value");
                        }
                    }
                }
                break;

                case ScriptUnaryOperator.FunctionParametersExpand:
                    return value;
            }
        }

        throw new ScriptRuntimeException(span, $"Operator `{op.ToText()}` is not supported");
    }
    /// <summary><c>Wrap</c>.</summary>
    public static ScriptUnaryExpression Wrap(ScriptUnaryOperator unaryOperator, ScriptToken unaryToken, ScriptExpression expression, bool transferTrivia)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var unary = new ScriptUnaryExpression()
        {
            Span = expression.Span,
            Operator = unaryOperator,
            OperatorToken = unaryToken,
            Right = expression,
        };

        if (!transferTrivia) return unary;

        var firstTerminal = expression.FindFirstTerminal();
        firstTerminal?.MoveLeadingTriviasTo(unary.OperatorToken);

        return unary;
    }
}
