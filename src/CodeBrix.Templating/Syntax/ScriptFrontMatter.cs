// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using CodeBrix.Templating.Parsing;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptFrontMatter</c>.</summary>
public
partial class ScriptFrontMatter : ScriptStatement
{
    private ScriptToken _startMarker = new ScriptToken(TokenType.FrontMatterMarker);
    private ScriptToken _endMarker = new ScriptToken(TokenType.FrontMatterMarker);
    private ScriptBlockStatement _statements = new ScriptBlockStatement();
    /// <summary><c>ScriptFrontMatter</c>.</summary>
    public ScriptFrontMatter()
    {
        _startMarker.Parent = this;
        _endMarker.Parent = this;
        _statements.Parent = this;
    }
    /// <summary><c>StartMarker</c>.</summary>
    public ScriptToken StartMarker
    {
        get => _startMarker;
        set => ParentToThis(ref _startMarker, value);
    }
    /// <summary><c>Statements</c>.</summary>
    public ScriptBlockStatement Statements
    {
        get => _statements;
        set => ParentToThis(ref _statements, value);
    }
    /// <summary><c>EndMarker</c>.</summary>
    public ScriptToken EndMarker
    {
        get => _endMarker;
        set => ParentToThis(ref _endMarker, value);
    }
    /// <summary><c>TextPositionAfterEndMarker</c>.</summary>
    public TextPosition TextPositionAfterEndMarker;
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        return context.Evaluate(Statements);
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.ExpectEos();
        printer.Write(StartMarker);
        printer.ExpectEos();
        printer.Write(Statements);
        printer.ExpectEos();
        printer.Write(EndMarker);
        printer.ExpectEos();
    }
}
