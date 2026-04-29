// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using CodeBrix.Templating.Runtime;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptWithStatement</c>.</summary>
[ScriptSyntax("with statement", "with <variable> ... end")]
public
partial class ScriptWithStatement : ScriptStatement
{
    private ScriptKeyword _withKeyword;
    private ScriptExpression _name;
    private ScriptBlockStatement _body;
    /// <summary><c>ScriptWithStatement</c>.</summary>
    public ScriptWithStatement()
    {
        _withKeyword = ScriptKeyword.With();
        _withKeyword.Parent = this;
    }
    /// <summary><c>WithKeyword</c>.</summary>
    public ScriptKeyword WithKeyword
    {
        get => _withKeyword;
        set => ParentToThis(ref _withKeyword, value);
    }
    /// <summary><c>Name</c>.</summary>
    public ScriptExpression Name
    {
        get => _name;
        set => ParentToThisNullable(ref _name, value);
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
        if (Name is null)
        {
            return null;
        }

        var target = context.GetValue(Name);
        if (target is not IScriptObject scriptObject)
        {
            var targetName = target?.GetType().Name ?? "null";
            throw new ScriptRuntimeException(Name.Span, $"Invalid target property `{Name}` used for [with] statement. Must be a ScriptObject instead of `{targetName}`");
        }

        context.PushGlobal(scriptObject);
        try
        {
            var result = Body is null ? null : context.Evaluate(Body);
            return result;
        }
        finally
        {
            context.PopGlobal();
        }
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.Write(WithKeyword).ExpectSpace();
        printer.Write(Name);
        printer.ExpectEos();
        printer.Write(Body).ExpectEos();
    }
}
