// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptReadOnlyStatement</c>.</summary>
[ScriptSyntax("readonly statement", "readonly <variable>")]
public
partial class ScriptReadOnlyStatement : ScriptStatement
{
    private ScriptVariable _variable;
    private ScriptKeyword _readOnlyKeyword = ScriptKeyword.ReadOnly();
    /// <summary><c>ScriptReadOnlyStatement</c>.</summary>
    public ScriptReadOnlyStatement()
    {
        _readOnlyKeyword.Parent = this;
    }
    /// <summary><c>ReadOnlyKeyword</c>.</summary>
    public ScriptKeyword ReadOnlyKeyword
    {
        get => _readOnlyKeyword;
        set => ParentToThis(ref _readOnlyKeyword, value);
    }
    /// <summary><c>Variable</c>.</summary>
    public ScriptVariable Variable
    {
        get => _variable;
        set => ParentToThisNullable(ref _variable, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        if (Variable is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid readonly statement. Variable is required.");
        }
        context.SetReadOnly(Variable);
        return null;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.Write(ReadOnlyKeyword).ExpectSpace();
        if (Variable is not null)
        {
            printer.Write(Variable);
        }
        printer.ExpectEos();
    }
}
