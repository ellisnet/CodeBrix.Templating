// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using CodeBrix.Templating.Runtime;
using System.Collections.Generic;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptReturnStatement</c>.</summary>
[ScriptSyntax("return statement", "return <expression>?")]
public
partial class ScriptReturnStatement : ScriptStatement
{
    private ScriptExpression _expression;
    private ScriptKeyword _retKeyword;
    /// <summary><c>ScriptReturnStatement</c>.</summary>
    public ScriptReturnStatement()
    {
        _retKeyword = ScriptKeyword.Ret();
        _retKeyword.Parent = this;
    }
    /// <summary><c>RetKeyword</c>.</summary>
    public ScriptKeyword RetKeyword
    {
        get => _retKeyword;
        set => ParentToThis(ref _retKeyword, value);
    }
    /// <summary><c>Expression</c>.</summary>
    public ScriptExpression Expression
    {
        get => _expression;
        set => ParentToThisNullable(ref _expression, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        var result = Expression is null ? null : context.Evaluate(Expression);
        //ensure that deferred array interators are evaluated before we lose context
        if (result is ScriptRange range)
        {
            result = new ScriptArray(range);
        }
        context.FlowState = ScriptFlowState.Return;
        return result;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.Write(RetKeyword).ExpectSpace();
        printer.Write(Expression).ExpectEos();
    }
}
