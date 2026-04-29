// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using CodeBrix.Templating.Runtime;
using System.Collections.Generic;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptIsEmptyExpression</c>.</summary>
[ScriptSyntax("empty expression", "<expression>.empty?")]
public
partial class ScriptIsEmptyExpression: ScriptMemberExpression, IScriptVariablePath
{
    private ScriptToken _questionToken = ScriptToken.Question();
    /// <summary><c>ScriptIsEmptyExpression</c>.</summary>
    public ScriptIsEmptyExpression()
    {
        _questionToken.Parent = this;
    }
    /// <summary><c>QuestionToken</c>.</summary>
    public ScriptToken QuestionToken
    {
        get => _questionToken;
        set => ParentToThis(ref _questionToken, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        return context.GetValue(this);
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        base.PrintTo(printer);
        printer.Write(QuestionToken);
    }
    /// <summary><c>CanHaveLeadingTrivia</c>.</summary>
    public override bool CanHaveLeadingTrivia()
    {
        return false;
    }
    /// <summary><c>GetValue</c>.</summary>
    public override object GetValue(TemplateContext context)
    {
        var targetObject = GetTargetObject(context, false);
        return context.IsEmpty(Span, targetObject);
    }
    /// <summary><c>SetValue</c>.</summary>
    public override void SetValue(TemplateContext context, object valueToSet)
    {
        throw new ScriptRuntimeException(Span, $"The `.empty?` property cannot be set");
    }
    /// <summary><c>GetFirstPath</c>.</summary>
    public override string GetFirstPath()
    {
        return (Target as IScriptVariablePath)?.GetFirstPath() ?? string.Empty;
    }

    private object GetTargetObject(TemplateContext context, bool isSet)
    {
        var target = Target;
        if (target is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid `.empty?` expression. Target is required.");
        }

        var targetObject = context.GetValue(target);

        if (targetObject is null)
        {
            if (isSet || !context.EnableRelaxedMemberAccess)
            {
                throw new ScriptRuntimeException(this.Span, $"Object `{this.Target}` is null. Cannot access property `empty?`");
            }
        }
        return targetObject;
    }
}
