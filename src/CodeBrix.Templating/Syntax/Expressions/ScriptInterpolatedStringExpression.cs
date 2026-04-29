// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptInterpolatedStringExpression</c>.</summary>
[ScriptSyntax("interpolated expression", "{<expression>}")]
public
partial class ScriptInterpolatedStringExpression : ScriptExpression
{
    private ScriptList<ScriptExpression> _stringParts;
    /// <summary><c>ScriptInterpolatedStringExpression</c>.</summary>
    public ScriptInterpolatedStringExpression()
    {
        _stringParts = new ScriptList<ScriptExpression>();
        _stringParts.Parent = this;
    }
    /// <summary><c>Parts</c>.</summary>
    public ScriptList<ScriptExpression> Parts
    {
        get => _stringParts;
        set => ParentToThis(ref _stringParts, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        // A nested expression will reset the pipe arguments for the group
        context.PushPipeArguments();
        try
        {
            var builder = new System.Text.StringBuilder(); // TODO: use thread local
            foreach (var scriptExpression in Parts)
            {
                var value = context.Evaluate(scriptExpression);
                if (value is not null)
                {
                    builder.Append(value);
                }
            }
            return builder.ToString();
        }
        finally
        {
            if (context.CurrentPipeArguments is not null)
            {
                context.PopPipeArguments();
            }
        }
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        foreach (var scriptExpression in Parts)
        {
            printer.Write(scriptExpression);
        }
    }
}
