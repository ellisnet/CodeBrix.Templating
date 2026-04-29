// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptNamedArgument</c>.</summary>
public
partial class ScriptNamedArgument : ScriptExpression
{
    private ScriptVariable _name;
    private ScriptToken _colonToken;
    private ScriptExpression _value;
    /// <summary><c>ScriptNamedArgument</c>.</summary>
    public ScriptNamedArgument()
    {
    }
    /// <summary><c>Name</c>.</summary>
    public ScriptVariable Name
    {
        get => _name;
        set => ParentToThisNullable(ref _name, value);
    }
    /// <summary><c>ColonToken</c>.</summary>
    public ScriptToken ColonToken
    {
        get => _colonToken;
        set => ParentToThisNullable(ref _colonToken, value);
    }
    /// <summary><c>Value</c>.</summary>
    public ScriptExpression Value
    {
        get => _value;
        set => ParentToThisNullable(ref _value, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        if (Value is not null) return context.Evaluate(Value);
        return true;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        if (Name is null)
        {
            return;
        }
        printer.Write(Name);

        if (Value is not null)
        {
            if (ColonToken is not null)
            {
                printer.Write(ColonToken);
            }
            printer.Write(Value);
        }
    }
}
