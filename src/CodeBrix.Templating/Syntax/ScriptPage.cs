// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using CodeBrix.Templating.Parsing;
using System.Collections.Generic;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptPage</c>.</summary>
public
partial class ScriptPage : ScriptNode
{
    private ScriptFrontMatter _frontMatter;
    private ScriptBlockStatement _body;

    /// <summary>
    /// Gets or sets the front matter. May be <c>null</c> if script is not parsed using  <see cref="ScriptMode.FrontMatterOnly"/> or <see cref="ScriptMode.FrontMatterAndContent"/>. See remarks.
    /// </summary>
    /// <remarks>
    /// Note that this code block is not executed when evaluating this page. It has to be evaluated separately (usually before evaluating the page).
    /// </remarks>
    public ScriptFrontMatter FrontMatter
    {
        get => _frontMatter;
        set => ParentToThisNullable(ref _frontMatter, value);
    }
    /// <summary><c>Body</c>.</summary>
    public ScriptBlockStatement Body
    {
        get => _body;
        set => ParentToThisNullable(ref _body, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        context.FlowState = ScriptFlowState.None;
        try
        {
            return Body is null ? null : context.Evaluate(Body);
        }
        finally
        {
            context.FlowState = ScriptFlowState.None;
        }
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        if (Body is not null)
        {
            printer.Write(Body);
        }
    }
}
