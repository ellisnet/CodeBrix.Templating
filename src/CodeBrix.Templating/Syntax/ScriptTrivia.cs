// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using CodeBrix.Templating.Parsing;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptTrivia</c>.</summary>
public
readonly partial struct ScriptTrivia
{
    /// <summary><c>Space</c>.</summary>
    public static readonly ScriptTrivia Space = new ScriptTrivia(new SourceSpan(), ScriptTriviaType.Whitespace, (ScriptStringSlice)" ");
    /// <summary><c>Comma</c>.</summary>
    public static readonly ScriptTrivia Comma = new ScriptTrivia(new SourceSpan(), ScriptTriviaType.Comma, (ScriptStringSlice)",");
    /// <summary><c>SemiColon</c>.</summary>
    public static readonly ScriptTrivia SemiColon = new ScriptTrivia(new SourceSpan(), ScriptTriviaType.SemiColon, (ScriptStringSlice)";");
    /// <summary><c>ScriptTrivia</c>.</summary>
    public ScriptTrivia(SourceSpan span, ScriptTriviaType type, ScriptStringSlice text)
    {
        Span = span;
        Type = type;
        Text = text;
    }
    /// <summary><c>Span</c>.</summary>
    public readonly SourceSpan Span;
    /// <summary><c>Type</c>.</summary>
    public readonly ScriptTriviaType Type;
    /// <summary><c>Text</c>.</summary>
    public readonly ScriptStringSlice Text;
    /// <summary><c>WithText</c>.</summary>
    public ScriptTrivia WithText(ScriptStringSlice text)
    {
        return new ScriptTrivia(Span, Type, text);
    }
    /// <summary><c>Write</c>.</summary>
    public void Write(ScriptPrinter printer)
    {
        var rawText = ToString();

        bool isRawComment = Type == ScriptTriviaType.CommentMulti && !rawText.StartsWith("##");
        if (isRawComment)
        {
            // Escape any # by \#
            rawText = rawText.Replace("#", "\\#");
            // Escape any }}
            rawText = rawText.Replace("}", "\\}");
            printer.Write("## ");
        }

        printer.Write(rawText);

        if (isRawComment)
        {
            printer.Write(" ##");
        }
    }
    /// <summary><c>ToString</c>.</summary>
    public override string ToString()
    {
        switch (Type)
        {
            case ScriptTriviaType.Empty:
                return string.Empty;
            case ScriptTriviaType.Comma:
                return ",";
            case ScriptTriviaType.SemiColon:
                return ";";
        }
        return (string)Text;
    }
}
