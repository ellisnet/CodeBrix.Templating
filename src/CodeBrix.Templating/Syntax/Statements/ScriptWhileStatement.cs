// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptWhileStatement</c>.</summary>
[ScriptSyntax("while statement", "while <expression> ... end")]
public
partial class ScriptWhileStatement : ScriptLoopStatementBase
{
    private ScriptKeyword _whileKeyword;
    private ScriptExpression _condition;
    private ScriptBlockStatement _body;
    /// <summary><c>ScriptWhileStatement</c>.</summary>
    public ScriptWhileStatement()
    {
        _whileKeyword = ScriptKeyword.While();
        _whileKeyword.Parent = this;
    }
    /// <summary><c>WhileKeyword</c>.</summary>
    public ScriptKeyword WhileKeyword
    {
        get => _whileKeyword;
        set => ParentToThis(ref _whileKeyword, value);
    }
    /// <summary><c>Condition</c>.</summary>
    public ScriptExpression Condition
    {
        get => _condition;
        set => ParentToThisNullable(ref _condition, value);
    }
    /// <summary><c>Body</c>.</summary>
    public ScriptBlockStatement Body
    {
        get => _body;
        set => ParentToThisNullable(ref _body, value);
    }
    /// <summary><c>LoopItem</c>.</summary>
    protected override object LoopItem(TemplateContext context, LoopState state)
    {
        return Body is null ? null : context.Evaluate(Body);
    }
    /// <summary><c>EvaluateImpl</c>.</summary>
    protected override object EvaluateImpl(TemplateContext context)
    {
        var index = 0;
        object result = null;
        BeforeLoop(context);

        var loopState = CreateLoopState();
        context.SetLoopVariable(ScriptVariable.WhileObject, loopState);

        while (context.StepLoop(this))
        {
            if (Condition is null)
            {
                break;
            }

            var conditionResult = context.ToBool(Condition.Span, context.Evaluate(Condition));
            if (!conditionResult)
            {
                break;
            }

            loopState.Index = index++;
            result = LoopItem(context, loopState);

            if (!ContinueLoop(context))
            {
                break;
            }
        };
        AfterLoop(context);
        return result;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.Write(WhileKeyword).ExpectSpace();
        printer.Write(Condition);
        printer.ExpectEos();
        printer.Write(Body).ExpectEos();
    }
}
