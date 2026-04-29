// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CodeBrix.Templating.Parsing; //was previously: Scriban.Parsing;

/// <summary>
/// An enumeration to categorize tokens.
/// </summary>
public
enum TokenType
{
    /// <summary><c>Invalid</c>.</summary>
    Invalid,
    /// <summary><c>FrontMatterMarker</c>.</summary>
    FrontMatterMarker,

    /// <summary>Token "{{"</summary>
    CodeEnter,

    /// <summary>Token "{%"</summary>
    LiquidTagEnter,

    /// <summary>Token "}}"</summary>
    CodeExit,

    /// <summary>Token "%}"</summary>
    LiquidTagExit,
    /// <summary><c>Raw</c>.</summary>
    Raw,
    /// <summary><c>Escape</c>.</summary>
    Escape,
    /// <summary><c>EscapeEnter</c>.</summary>
    EscapeEnter,
    /// <summary><c>EscapeExit</c>.</summary>
    EscapeExit,
    /// <summary><c>NewLine</c>.</summary>
    NewLine,
    /// <summary><c>Whitespace</c>.</summary>
    Whitespace,
    /// <summary><c>WhitespaceFull</c>.</summary>
    WhitespaceFull,
    /// <summary><c>Comment</c>.</summary>
    Comment,
    /// <summary><c>CommentMulti</c>.</summary>
    CommentMulti,

    /// <summary>
    /// An identifier starting by a $
    /// </summary>
    IdentifierSpecial,

    /// <summary>
    /// An identifier
    /// </summary>
    Identifier,

    /// <summary>
    /// An integer (int, long...)
    /// </summary>
    Integer,

    /// <summary>
    /// A Hexadecimal integer (int, long...)
    /// </summary>
    HexaInteger,

    /// <summary>
    /// A binary integer (int, long...)
    /// </summary>
    BinaryInteger,

    /// <summary>
    /// A floating point number
    /// </summary>
    Float,

    /// <summary>
    /// A string
    /// </summary>
    String,

    /// <summary>
    /// An interpolated string without interpolated expressions (e.g $"This is a string with no interpolated expressions")
    /// </summary>
    InterpolatedString,

    /// <summary>
    /// An interpolated string at the beginning (e.g $"This is a string with { )
    /// </summary>
    BeginInterpolatedString,

    /// <summary>
    /// An interpolated string at the middle (e.g } a continuation string { )
    /// </summary>
    ContinuationInterpolatedString,

    /// <summary>
    /// An interpolated string at the end (e.g } a ending of an interpolated string" )
    /// </summary>
    EndingInterpolatedString,

    /// <summary>
    /// An implicit string with quotes
    /// </summary>
    ImplicitString,

    /// <summary>
    /// A verbatim string
    /// </summary>
    VerbatimString,

    /// <summary>Token ";"</summary>
    SemiColon,

    /// <summary>Token "@"</summary>
        Arroba,

        /// <summary>Token "^"</summary>
    Caret,

    /// <summary>Token "^^"</summary>
    DoubleCaret,

    /// <summary>Token ":"</summary>
    Colon,

    /// <summary>Token "="</summary>
    Equal,

    /// <summary>Token "|"</summary>
    VerticalBar, // |

    /// <summary>Token "|>"</summary>
    PipeGreater, // |>

    /// <summary>Token "!"</summary>
    Exclamation, // !

    /// <summary>Token "&amp;&amp;"</summary>
    DoubleAmp, // &&

    /// <summary>Token "||"</summary>
    DoubleVerticalBar, // ||

    /// <summary>Token "&amp;"</summary>
    Amp, // &

    /// <summary>Token "?"</summary>
    Question,

    /// <summary>Token "??"</summary>
    DoubleQuestion,

    /// <summary>Token "?."</summary>
    QuestionDot,

    /// <summary>Token "?!"</summary>
    QuestionExclamation,

    /// <summary>Token "=="</summary>
    DoubleEqual,

    /// <summary>Token "!="</summary>
    ExclamationEqual,

    /// <summary>Token "&lt;"</summary>
    Less,

    /// <summary>Token ">"</summary>
    Greater,

    /// <summary>Token "&lt;="</summary>
    LessEqual,

    /// <summary>Token ">="</summary>
    GreaterEqual,

    /// <summary>Token "/"</summary>
    Divide,

    /// <summary>Token "/="</summary>
    DivideEqual,

    /// <summary>Token "//"</summary>
    DoubleDivide,

    /// <summary>Token "//="</summary>
    DoubleDivideEqual,

    /// <summary>Token "*"</summary>
    Asterisk,

    /// <summary>Token "*="</summary>
    AsteriskEqual,

    /// <summary>Token "+"</summary>
    Plus,

    /// <summary>Token "+="</summary>
    PlusEqual,

    /// <summary>Token "++"</summary>
    DoublePlus,

    /// <summary>Token "-"</summary>
    Minus,

    /// <summary>Token "-="</summary>
    MinusEqual,

    /// <summary>Token "--"</summary>
    DoubleMinus,

    /// <summary>Token "%"</summary>
    Percent,

    /// <summary>Token "%="</summary>
    PercentEqual,

    /// <summary>Token "&lt;&lt;"</summary>
    DoubleLessThan,

    /// <summary>Token ">>"</summary>
    DoubleGreaterThan,

    /// <summary>Token ","</summary>
    Comma,

    /// <summary>Token "."</summary>
    Dot,

    /// <summary>Token ".."</summary>
    DoubleDot,

    /// <summary>Token "..."</summary>
    TripleDot,

    /// <summary>Token "..&lt;"</summary>
    DoubleDotLess,

    /// <summary>Token "("</summary>
    OpenParen,

    /// <summary>Token ")"</summary>
    CloseParen,

    /// <summary>Token "{"</summary>
    OpenBrace,

    /// <summary>Token "}"</summary>
    CloseBrace,

    /// <summary>Token "["</summary>
    OpenBracket,

    /// <summary>Token "]"</summary>
    CloseBracket,

    /// <summary>Token "{"</summary>
    OpenInterpolatedBrace,

    /// <summary>Token "}"</summary>
    CloseInterpolatedBrace,

    /// <summary>
    /// Custom token
    /// </summary>
    Custom,
    /// <summary><c>Custom1</c>.</summary>
    Custom1,
    /// <summary><c>Custom2</c>.</summary>
    Custom2,
    /// <summary><c>Custom3</c>.</summary>
    Custom3,
    /// <summary><c>Custom4</c>.</summary>
    Custom4,
    /// <summary><c>Custom5</c>.</summary>
    Custom5,
    /// <summary><c>Custom6</c>.</summary>
    Custom6,
    /// <summary><c>Custom7</c>.</summary>
    Custom7,
    /// <summary><c>Custom8</c>.</summary>
    Custom8,
    /// <summary><c>Custom9</c>.</summary>
    Custom9,
    /// <summary><c>Eof</c>.</summary>
    Eof,
}
