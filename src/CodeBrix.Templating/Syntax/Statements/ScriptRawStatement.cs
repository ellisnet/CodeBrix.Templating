// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptRawStatement</c>.</summary>
[ScriptSyntax("raw statement", "<raw_text>")]
public
partial class ScriptRawStatement : ScriptStatement
{
    /// <summary><c>Text</c>.</summary>
    public ScriptStringSlice Text { get; set; }
    /// <summary><c>IsEscape</c>.</summary>
    public bool IsEscape { get; set; }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        if (Text.Length == 0 || string.IsNullOrEmpty(Text.FullText)) return null;

        if (Text.Length > 0)
        {
            // If we are in the context of output, output directly to TemplateContext.Output
            if (context.EnableOutput)
            {
                context.Write(Text);
            }
            else
            {
                return Text.ToString();
            }
        }
        return null;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        if (Text.Length > 0 && !string.IsNullOrEmpty(Text.FullText))
        {
            printer.Write(Text);
        }
    }
}
