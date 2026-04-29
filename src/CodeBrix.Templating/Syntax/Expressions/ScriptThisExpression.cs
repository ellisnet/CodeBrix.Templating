// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;

/// <summary>
/// this expression returns the current <see cref="TemplateContext.CurrentGlobal"/> script object.
/// </summary>
[ScriptSyntax("this expression", "this")]
public
partial class ScriptThisExpression : ScriptExpression, IScriptVariablePath
{
    private ScriptKeyword _thisKeyword;
    /// <summary><c>ScriptThisExpression</c>.</summary>
    public ScriptThisExpression()
    {
        _thisKeyword = ScriptKeyword.This();
        _thisKeyword.Parent = this;
    }
    /// <summary><c>ThisKeyword</c>.</summary>
    public ScriptKeyword ThisKeyword
    {
        get => _thisKeyword;
        set => ParentToThis(ref _thisKeyword, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        return context.GetValue(this);
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.Write(ThisKeyword);
    }
    /// <summary><c>GetValue</c>.</summary>
    public object GetValue(TemplateContext context)
    {
        return context.CurrentGlobal;
    }
    /// <summary><c>SetValue</c>.</summary>
    public void SetValue(TemplateContext context, object valueToSet)
    {
        throw new ScriptRuntimeException(Span, "Cannot set this variable");
    }
    /// <summary><c>GetFirstPath</c>.</summary>
    public string GetFirstPath()
    {
        return "this";
    }

#if !SCRIBAN_NO_ASYNC
    /// <summary><c>GetValueAsync</c>.</summary>
    public ValueTask<object> GetValueAsync(TemplateContext context)
    {
        return new ValueTask<object>(context.CurrentGlobal);
    }
    /// <summary><c>SetValueAsync</c>.</summary>
    public ValueTask SetValueAsync(TemplateContext context, object valueToSet)
    {
        throw new ScriptRuntimeException(Span, "Cannot set this variable");
    }
#endif
}
