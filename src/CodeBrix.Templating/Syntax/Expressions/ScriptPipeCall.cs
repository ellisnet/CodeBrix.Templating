// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using CodeBrix.Templating.Runtime;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptPipeCall</c>.</summary>
[ScriptSyntax("pipe expression", "<expression> | <expression>")]
public
partial class ScriptPipeCall : ScriptExpression
{
    private ScriptExpression _from;
    private ScriptToken _pipeToken;
    private ScriptExpression _to;
    /// <summary><c>From</c>.</summary>
    public ScriptExpression From
    {
        get => _from;
        set => ParentToThisNullable(ref _from, value);
    }
    /// <summary><c>PipeToken</c>.</summary>
    public ScriptToken PipeToken
    {
        get => _pipeToken;
        set => ParentToThisNullable(ref _pipeToken, value);
    }
    /// <summary><c>To</c>.</summary>
    public ScriptExpression To
    {
        get => _to;
        set => ParentToThisNullable(ref _to, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        var from = From;
        var to = To;
        if (from is null || to is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid pipe expression. Source and destination are required.");
        }

        bool newPipe = context.CurrentPipeArguments is null;
        try
        {
            // Push a new pipe arguments
            if (newPipe) context.PushPipeArguments();

            var pipeArguments = context.CurrentPipeArguments ?? throw new ScriptRuntimeException(Span, "Pipe arguments were not initialized.");
            pipeArguments.Push(from);

            var result = context.Evaluate(to);

            // If the result returns by the evaluation is a function and we haven't yet consumed the pipe argument
            // that means that we need to evaluate this function with the actual pipe arguments.
            if (result is IScriptCustomFunction && pipeArguments.Count > 0 && pipeArguments.Peek() == from)
            {
                result = ScriptFunctionCall.Call(context, to, result, true, null);
            }

            // If we have still remaining arguments, it is likely that the destination expression is not a function
            // so pipe arguments were not picked up and this is an error
            if (pipeArguments.Count > 0 && pipeArguments.Peek() == from)
            {
                throw new ScriptRuntimeException(to.Span, $"Pipe expression destination `{to}` is not a valid function ");
            }

            return result;
        }
        catch
        {
            // If we have an exception clear all the pipe froms
            newPipe = false; // Don't try to clear the pipe
            context.ClearPipeArguments();
            throw;
        }
        finally
        {
            if (newPipe)
            {
                context.PopPipeArguments();
            }
        }
    }
    /// <summary><c>CanHaveLeadingTrivia</c>.</summary>
    public override bool CanHaveLeadingTrivia()
    {
        return false;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        if (From is not null)
        {
            printer.Write(From);
        }
        if (PipeToken is not null)
        {
            printer.Write(PipeToken);
        }
        if (To is not null)
        {
            printer.Write(To);
        }
    }
}
