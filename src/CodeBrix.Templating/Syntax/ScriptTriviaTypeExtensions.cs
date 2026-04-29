// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptTriviaTypeExtensions</c>.</summary>
public
static class ScriptTriviaTypeExtensions
{
    /// <summary><c>IsSpace</c>.</summary>
    public static bool IsSpace(this ScriptTriviaType triviaType)
    {
        return triviaType switch
        {
            ScriptTriviaType.Whitespace => true,
            ScriptTriviaType.WhitespaceFull => true,
            _ => false
        };
    }
    /// <summary><c>IsNewLine</c>.</summary>
    public static bool IsNewLine(this ScriptTriviaType triviaType)
    {
        return triviaType == ScriptTriviaType.NewLine;
    }
    /// <summary><c>IsSpaceOrNewLine</c>.</summary>
    public static bool IsSpaceOrNewLine(this ScriptTriviaType triviaType)
    {
        return triviaType switch
        {
            ScriptTriviaType.Whitespace => true,
            ScriptTriviaType.WhitespaceFull => true,
            ScriptTriviaType.NewLine => true,
            _ => false
        };
    }
}
