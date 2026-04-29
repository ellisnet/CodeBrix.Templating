// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using CodeBrix.Templating.Helpers;
using CodeBrix.Templating.Runtime;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptArrayInitializerExpression</c>.</summary>
[ScriptSyntax("array initializer", "[item1, item2,...]")]
public
partial class ScriptArrayInitializerExpression : ScriptExpression
{
    private ScriptList<ScriptExpression> _values = new ScriptList<ScriptExpression>();
    private ScriptToken _openBracketToken = ScriptToken.OpenBracket();
    private ScriptToken _closeBracketToken = ScriptToken.CloseBracket();
    /// <summary><c>ScriptArrayInitializerExpression</c>.</summary>
    public ScriptArrayInitializerExpression()
    {
        _openBracketToken.Parent = this;
        _values.Parent = this;
        _closeBracketToken.Parent = this;
    }
    /// <summary><c>OpenBracketToken</c>.</summary>
    public ScriptToken OpenBracketToken
    {
        get => _openBracketToken;
        set => ParentToThis(ref _openBracketToken, value);
    }
    /// <summary><c>Values</c>.</summary>
    public ScriptList<ScriptExpression> Values
    {
        get => _values;
        set => ParentToThis(ref _values, value);
    }
    /// <summary><c>CloseBracketToken</c>.</summary>
    public ScriptToken CloseBracketToken
    {
        get => _closeBracketToken;
        set => ParentToThis(ref _closeBracketToken, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        var scriptArray = new ScriptArray();
        foreach (var value in Values)
        {
            var valueEval = context.Evaluate(value);
            scriptArray.Add(valueEval);
        }
        return scriptArray;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.Write(OpenBracketToken);
        printer.WriteListWithCommas(Values);
        printer.Write(CloseBracketToken);
    }
}
