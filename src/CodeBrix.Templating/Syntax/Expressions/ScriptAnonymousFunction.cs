// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptAnonymousFunction</c>.</summary>
public
partial class ScriptAnonymousFunction : ScriptExpression
{
    private ScriptFunction _function;
    /// <summary><c>Function</c>.</summary>
    public ScriptFunction Function
    {
        get => _function;
        set => ParentToThisNullable(ref _function, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        return Function;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        if (Function is not null)
        {
            printer.Write(Function);
        }
    }
}
