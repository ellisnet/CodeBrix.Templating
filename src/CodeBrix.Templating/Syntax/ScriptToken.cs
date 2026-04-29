// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using CodeBrix.Templating.Parsing;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;

/// <summary>
/// A verbatim node (use for custom parsing).
/// </summary>
public
partial class ScriptToken : ScriptVerbatim
{
    /// <summary><c>SemiColon</c>.</summary>
    public static ScriptToken SemiColon() => new ScriptToken(TokenType.SemiColon);
    /// <summary><c>Arroba</c>.</summary>
    public static ScriptToken Arroba() => new ScriptToken(TokenType.Arroba);
    /// <summary><c>Caret</c>.</summary>
    public static ScriptToken Caret() => new ScriptToken(TokenType.Caret);
    /// <summary><c>DoubleCaret</c>.</summary>
    public static ScriptToken DoubleCaret() => new ScriptToken(TokenType.DoubleCaret);
    /// <summary><c>Colon</c>.</summary>
    public static ScriptToken Colon() => new ScriptToken(TokenType.Colon);
    /// <summary><c>Equal</c>.</summary>
    public static ScriptToken Equal() => new ScriptToken(TokenType.Equal);
    /// <summary><c>Pipe</c>.</summary>
    public static ScriptToken Pipe() => new ScriptToken(TokenType.VerticalBar);
    /// <summary><c>PipeGreater</c>.</summary>
    public static ScriptToken PipeGreater() => new ScriptToken(TokenType.PipeGreater);
    /// <summary><c>Exclamation</c>.</summary>
    public static ScriptToken Exclamation() => new ScriptToken(TokenType.Exclamation);
    /// <summary><c>DoubleAmp</c>.</summary>
    public static ScriptToken DoubleAmp() => new ScriptToken(TokenType.DoubleAmp);
    /// <summary><c>DoublePipe</c>.</summary>
    public static ScriptToken DoublePipe() => new ScriptToken(TokenType.DoubleVerticalBar);
    /// <summary><c>Amp</c>.</summary>
    public static ScriptToken Amp() => new ScriptToken(TokenType.Amp);
    /// <summary><c>Question</c>.</summary>
    public static ScriptToken Question() => new ScriptToken(TokenType.Question);
    /// <summary><c>DoubleQuestion</c>.</summary>
    public static ScriptToken DoubleQuestion() => new ScriptToken(TokenType.DoubleQuestion);
    /// <summary><c>QuestionExclamation</c>.</summary>
    public static ScriptToken QuestionExclamation() => new ScriptToken(TokenType.QuestionExclamation);
    /// <summary><c>CompareEqual</c>.</summary>
    public static ScriptToken CompareEqual() => new ScriptToken(TokenType.DoubleEqual);
    /// <summary><c>CompareNotEqual</c>.</summary>
    public static ScriptToken CompareNotEqual() => new ScriptToken(TokenType.ExclamationEqual);
    /// <summary><c>CompareLess</c>.</summary>
    public static ScriptToken CompareLess() => new ScriptToken(TokenType.Less);
    /// <summary><c>CompareGreater</c>.</summary>
    public static ScriptToken CompareGreater() => new ScriptToken(TokenType.Greater);
    /// <summary><c>CompareLessOrEqual</c>.</summary>
    public static ScriptToken CompareLessOrEqual() => new ScriptToken(TokenType.LessEqual);
    /// <summary><c>CompareGreaterOrEqual</c>.</summary>
    public static ScriptToken CompareGreaterOrEqual() => new ScriptToken(TokenType.GreaterEqual);
    /// <summary><c>Divide</c>.</summary>
    public static ScriptToken Divide() => new ScriptToken(TokenType.Divide);
    /// <summary><c>DivideEqual</c>.</summary>
    public static ScriptToken DivideEqual() => new ScriptToken(TokenType.DivideEqual);
    /// <summary><c>DoubleDivide</c>.</summary>
    public static ScriptToken DoubleDivide() => new ScriptToken(TokenType.DoubleDivide);
    /// <summary><c>DoubleDivideEqual</c>.</summary>
    public static ScriptToken DoubleDivideEqual() => new ScriptToken(TokenType.DoubleDivideEqual);
    /// <summary><c>Star</c>.</summary>
    public static ScriptToken Star() => new ScriptToken(TokenType.Asterisk);
    /// <summary><c>StarEqual</c>.</summary>
    public static ScriptToken StarEqual() => new ScriptToken(TokenType.AsteriskEqual);
    /// <summary><c>Plus</c>.</summary>
    public static ScriptToken Plus() => new ScriptToken(TokenType.Plus);
    /// <summary><c>PlusEqual</c>.</summary>
    public static ScriptToken PlusEqual() => new ScriptToken(TokenType.PlusEqual);
    /// <summary><c>Minus</c>.</summary>
    public static ScriptToken Minus() => new ScriptToken(TokenType.Minus);
    /// <summary><c>MinusEqual</c>.</summary>
    public static ScriptToken MinusEqual() => new ScriptToken(TokenType.MinusEqual);
    /// <summary><c>Modulus</c>.</summary>
    public static ScriptToken Modulus() => new ScriptToken(TokenType.Percent);
    /// <summary><c>ModulusEqual</c>.</summary>
    public static ScriptToken ModulusEqual() => new ScriptToken(TokenType.PercentEqual);
    /// <summary><c>DoubleLess</c>.</summary>
    public static ScriptToken DoubleLess() => new ScriptToken(TokenType.DoubleLessThan);
    /// <summary><c>DoubleGreater</c>.</summary>
    public static ScriptToken DoubleGreater() => new ScriptToken(TokenType.DoubleGreaterThan);
    /// <summary><c>Comma</c>.</summary>
    public static ScriptToken Comma() => new ScriptToken(TokenType.Comma);
    /// <summary><c>Dot</c>.</summary>
    public static ScriptToken Dot() => new ScriptToken(TokenType.Dot);
    /// <summary><c>DoubleDot</c>.</summary>
    public static ScriptToken DoubleDot() => new ScriptToken(TokenType.DoubleDot);
    /// <summary><c>TripleDot</c>.</summary>
    public static ScriptToken TripleDot() => new ScriptToken(TokenType.TripleDot);
    /// <summary><c>DoubleDotLess</c>.</summary>
    public static ScriptToken DoubleDotLess() => new ScriptToken(TokenType.DoubleDotLess);
    /// <summary><c>OpenParen</c>.</summary>
    public static ScriptToken OpenParen() => new ScriptToken(TokenType.OpenParen);
    /// <summary><c>CloseParen</c>.</summary>
    public static ScriptToken CloseParen() => new ScriptToken(TokenType.CloseParen);
    /// <summary><c>OpenBrace</c>.</summary>
    public static ScriptToken OpenBrace() => new ScriptToken(TokenType.OpenBrace);
    /// <summary><c>CloseBrace</c>.</summary>
    public static ScriptToken CloseBrace() => new ScriptToken(TokenType.CloseBrace);
    /// <summary><c>OpenBracket</c>.</summary>
    public static ScriptToken OpenBracket() => new ScriptToken(TokenType.OpenBracket);
    /// <summary><c>CloseBracket</c>.</summary>
    public static ScriptToken CloseBracket() => new ScriptToken(TokenType.CloseBracket);
    /// <summary><c>OpenInterpBrace</c>.</summary>
    public static ScriptToken OpenInterpBrace() => new ScriptToken(TokenType.OpenInterpolatedBrace);
    /// <summary><c>CloseInterpBrace</c>.</summary>
    public static ScriptToken CloseInterpBrace() => new ScriptToken(TokenType.CloseInterpolatedBrace);
    /// <summary><c>ScriptToken</c>.</summary>
    public ScriptToken()
    {
    }
    /// <summary><c>ScriptToken</c>.</summary>
    public ScriptToken(TokenType type)
    {
        TokenType = type;
        Value = type.ToText() ?? string.Empty;
    }
    /// <summary><c>TokenType</c>.</summary>
    public TokenType TokenType { get; set; }
}
