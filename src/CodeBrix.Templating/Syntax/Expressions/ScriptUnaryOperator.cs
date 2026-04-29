// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptUnaryOperator</c>.</summary>
public
enum ScriptUnaryOperator
{
    /// <summary><c>None</c>.</summary>
    None,
    /// <summary><c>Not</c>.</summary>
    Not,
    /// <summary><c>Negate</c>.</summary>
    Negate,
    /// <summary><c>Plus</c>.</summary>
    Plus,
    /// <summary><c>FunctionAlias</c>.</summary>
    FunctionAlias,
    /// <summary><c>FunctionParametersExpand</c>.</summary>
    FunctionParametersExpand,
    /// <summary><c>Increment</c>.</summary>
    Increment,
    /// <summary><c>Decrement</c>.</summary>
    Decrement,
    /// <summary><c>Custom</c>.</summary>
    Custom,
}
/// <summary><c>ScriptUnaryOperatorExtensions</c>.</summary>
public
static class ScriptUnaryOperatorExtensions
{
    /// <summary><c>ToText</c>.</summary>
    public static string ToText(this ScriptUnaryOperator op)
    {
        switch (op)
        {
            case ScriptUnaryOperator.Not:
                return "!";
            case ScriptUnaryOperator.Negate:
                return "-";
            case ScriptUnaryOperator.Plus:
                return "+";
            case ScriptUnaryOperator.FunctionAlias:
                return "@";
                case ScriptUnaryOperator.FunctionParametersExpand:
                    return "^";
            case ScriptUnaryOperator.Decrement:
                return "--";
            case ScriptUnaryOperator.Increment:
                return "++";
            default:
                throw new ArgumentOutOfRangeException(nameof(op));
        }
    }
}
