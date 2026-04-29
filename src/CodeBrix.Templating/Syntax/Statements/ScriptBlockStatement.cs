// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptBlockStatement</c>.</summary>
[ScriptSyntax("block statement", "<statement>...end")]
public
sealed partial class ScriptBlockStatement : ScriptStatement
{
    private ScriptList<ScriptStatement> _statements = new ScriptList<ScriptStatement>();
    /// <summary><c>ScriptBlockStatement</c>.</summary>
    public ScriptBlockStatement()
    {
        _statements.Parent = this;
    }
    /// <summary><c>Statements</c>.</summary>
    public ScriptList<ScriptStatement> Statements
    {
        get => _statements;
        set => ParentToThis(ref _statements, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        var autoIndent = context.AutoIndent;
        object result = null;
        var statements = Statements;
        string previousIndent = context.CurrentIndent;
        string currentIndent = previousIndent;
        try
        {
            for (int i = 0; i < statements.Count; i++)
            {
                var statement = statements[i];

                // Throws a cancellation
                context.CheckAbort();

                if (autoIndent && statement is ScriptEscapeStatement escape)
                {
                    if (escape.IsEntering)
                    {
                        currentIndent = escape.Indent;
                    }
                    else if (escape.IsClosing)
                    {
                        currentIndent = previousIndent;
                    }
                }
                context.CurrentIndent = currentIndent;

                if (statement.CanSkipEvaluation)
                {
                    continue;
                }

                result = context.Evaluate(statement);

                // Top-level assignment expression don't output anything
                if (!statement.CanOutput)
                {
                    result = null;
                }
                else if (result is not null && context.FlowState != ScriptFlowState.Return && context.EnableOutput)
                {
                    context.Write(Span, result);
                    result = null;
                }

                // If flow state is different, we need to exit this loop
                if (context.FlowState != ScriptFlowState.None)
                {
                    break;
                }
            }
        }
        finally
        {
            context.CurrentIndent = previousIndent;
        }

        return result;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        foreach (var scriptStatement in Statements)
        {
            printer.Write(scriptStatement);
        }
    }
    /// <summary><c>CanHaveLeadingTrivia</c>.</summary>
    public override bool CanHaveLeadingTrivia()
    {
        return false;
    }
}
