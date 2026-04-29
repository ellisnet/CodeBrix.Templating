// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using CodeBrix.Templating.Runtime;
using System.Collections.Generic;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptImportStatement</c>.</summary>
[ScriptSyntax("import statement", "import <expression>")]
public
partial class ScriptImportStatement : ScriptStatement
{
    private ScriptKeyword _importKeyword;
    private ScriptExpression _expression;
    /// <summary><c>ScriptImportStatement</c>.</summary>
    public ScriptImportStatement()
    {
        _importKeyword = ScriptKeyword.Import();
        _importKeyword.Parent = this;
    }
    /// <summary><c>ImportKeyword</c>.</summary>
    public ScriptKeyword ImportKeyword
    {
        get => _importKeyword;
        set => ParentToThis(ref  _importKeyword, value);
    }
    /// <summary><c>Expression</c>.</summary>
    public ScriptExpression Expression
    {
        get => _expression;
        set => ParentToThisNullable(ref _expression, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        if (Expression is null)
        {
            return null;
        }

        var value = context.Evaluate(Expression);
        if (value is null)
        {
            return null;
        }

        context.Import(Expression.Span, value);
        return null;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.Write(ImportKeyword).ExpectSpace();
        printer.Write(Expression);
        printer.ExpectEos();
    }
}
