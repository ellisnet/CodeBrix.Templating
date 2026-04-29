// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptTriviaType</c>.</summary>
public
enum ScriptTriviaType
{
    /// <summary><c>Empty</c>.</summary>
    Empty = 0,
    /// <summary><c>Whitespace</c>.</summary>
    Whitespace,
    /// <summary><c>WhitespaceFull</c>.</summary>
    WhitespaceFull,
    /// <summary><c>Comment</c>.</summary>
    Comment,
    /// <summary><c>Comma</c>.</summary>
    Comma,
    /// <summary><c>CommentMulti</c>.</summary>
    CommentMulti,
    /// <summary><c>NewLine</c>.</summary>
    NewLine,
    /// <summary><c>SemiColon</c>.</summary>
    SemiColon,
}
