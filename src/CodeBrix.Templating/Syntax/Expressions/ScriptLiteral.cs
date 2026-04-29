// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using CodeBrix.Templating.Parsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptLiteral</c>.</summary>
[ScriptSyntax("literal", "<value>")]
public
partial class ScriptLiteral : ScriptExpression, IScriptTerminal
{
    /// <summary><c>ScriptLiteral</c>.</summary>
    public ScriptLiteral()
    {
        Trivias = new ScriptTrivias();
    }
    /// <summary><c>ScriptLiteral</c>.</summary>
    public ScriptLiteral(object value)
    {
        Trivias = new ScriptTrivias();
        Value = value;
    }
    /// <summary><c>Trivias</c>.</summary>
    public ScriptTrivias Trivias { get; set; }
    /// <summary><c>Value</c>.</summary>
    public object Value { get; set; }
    /// <summary><c>StringQuoteType</c>.</summary>
    public ScriptLiteralStringQuoteType StringQuoteType { get; set; }
    /// <summary><c>StringTokenType</c>.</summary>
    public TokenType StringTokenType { get; set; } = TokenType.String;
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        return Value;
    }
    /// <summary><c>IsPositiveInteger</c>.</summary>
    public bool IsPositiveInteger()
    {
        if (Value is null)
        {
            return false;
        }
        var type = Value.GetType();
        if (type == typeof(int))
        {
            return ((int)Value) >= 0;
        }
        else if (type == typeof(byte))
        {
            return true;
        }
        else if (type == typeof(sbyte))
        {
            return ((sbyte)Value) >= 0;
        }
        else if (type == typeof(short))
        {
            return ((short)Value) >= 0;
        }
        else if (type == typeof(ushort))
        {
            return true;
        }
        else if (type == typeof(uint))
        {
            return true;
        }
        else if (type == typeof(long))
        {
            return (long)Value > 0;
        }
        else if (type == typeof(ulong))
        {
            return true;
        }
        return false;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        if (Value is null)
        {
            printer.Write("null");
            return;
        }

        var type = Value.GetType();
        if (type == typeof(string))
        {
            printer.Write(ToLiteral(StringQuoteType, StringTokenType, (string) Value));
        }
        else if (type == typeof(bool))
        {
            printer.Write(((bool) Value) ? "true" : "false");
        }
        else if (type == typeof(int))
        {
            printer.Write(((int) Value).ToString(CultureInfo.InvariantCulture));
        }
        else if (type == typeof(double))
        {
            printer.Write(AppendDecimalPoint(((double)Value).ToString("R", CultureInfo.InvariantCulture), true));
        }
        else if (type == typeof(float))
        {
            printer.Write(AppendDecimalPoint(((float)Value).ToString("R", CultureInfo.InvariantCulture), true));
            printer.Write("f");
        }
        else if (type == typeof(decimal))
        {
            printer.Write(AppendDecimalPoint(((decimal)Value).ToString(CultureInfo.InvariantCulture), true));
            printer.Write("m");
        }
        else if (type == typeof(byte))
        {
            printer.Write(((byte) Value).ToString(CultureInfo.InvariantCulture));
        }
        else if (type == typeof(sbyte))
        {
            printer.Write(((sbyte) Value).ToString(CultureInfo.InvariantCulture));
        }
        else if (type == typeof(short))
        {
            printer.Write(((short) Value).ToString(CultureInfo.InvariantCulture));
        }
        else if (type == typeof(ushort))
        {
            printer.Write(((ushort) Value).ToString(CultureInfo.InvariantCulture));
        }
        else if (type == typeof(uint))
        {
            printer.Write(((uint) Value).ToString(CultureInfo.InvariantCulture));
        }
        else if (type == typeof(long))
        {
            printer.Write(((long) Value).ToString(CultureInfo.InvariantCulture));
        }
        else if (type == typeof(ulong))
        {
            printer.Write(((uint) Value).ToString(CultureInfo.InvariantCulture));
        }
        else if (type == typeof(char))
        {
            var valueText = Value.ToString();
            printer.Write(ToLiteral(
                ScriptLiteralStringQuoteType.SimpleQuote,
                StringTokenType,
                valueText ?? string.Empty)
            );
        }
        else
        {
            var valueText = Value.ToString();
            if (valueText is not null)
            {
                printer.Write(valueText);
            }
        }
    }

    private static string ToLiteral(ScriptLiteralStringQuoteType quoteType, TokenType stringTokenType, string input)
    {
        char quote;
        switch (quoteType)
        {
            case ScriptLiteralStringQuoteType.DoubleQuote:
                quote = '"';
                break;
            case ScriptLiteralStringQuoteType.SimpleQuote:
                quote = '\'';
                break;
            case ScriptLiteralStringQuoteType.Verbatim:
                quote = '`';
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(quoteType));
        }

        var literal = new StringBuilder(input.Length + 2);

        if (stringTokenType == TokenType.BeginInterpolatedString || stringTokenType == TokenType.InterpolatedString)
        {
            literal.Capacity = input.Length + 3;
            literal.Append('$');
        }

        if (stringTokenType == TokenType.BeginInterpolatedString || stringTokenType == TokenType.String || stringTokenType == TokenType.InterpolatedString)
        {
            literal.Append(quote);
        }
        
        if (quoteType == ScriptLiteralStringQuoteType.Verbatim)
        {
            literal.Append(input.Replace("`", "``"));
        }
        else
        {
            foreach (var c in input)
            {
                switch (c)
                {
                    case '\\': literal.Append(@"\\"); break;
                    case '\0': literal.Append(@"\0"); break;
                    case '\a': literal.Append(@"\a"); break;
                    case '\b': literal.Append(@"\b"); break;
                    case '\f': literal.Append(@"\f"); break;
                    case '\n': literal.Append(@"\n"); break;
                    case '\r': literal.Append(@"\r"); break;
                    case '\t': literal.Append(@"\t"); break;
                    case '\v': literal.Append(@"\v"); break;
                    case '{' when stringTokenType.IsInterpolationStringToken(): literal.Append(@"\{"); break;
                    default:
                        if (c == quote)
                        {
                            literal.Append('\\').Append(c);
                        }
                        else if (char.IsControl(c))
                        {
                            literal.Append(@"\u");
                            literal.Append(((ushort)c).ToString("x4"));
                        }
                        else
                        {
                            literal.Append(c);
                        }
                        break;
                }
            }
        }

        if (stringTokenType == TokenType.EndingInterpolatedString || stringTokenType == TokenType.String || stringTokenType == TokenType.InterpolatedString)
        {
            literal.Append(quote);
        }
        return literal.ToString();
    }

    // Code from SharpYaml
    private static string AppendDecimalPoint(string text, bool hasNaN)
    {
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            // Do not append a decimal point if floating point type value
            // - is in exponential form, or
            // - already has a decimal point
            if (c == 'e' || c == 'E' || c == '.')
            {
                return text;
            }
        }
        // Special cases for floating point type supporting NaN and Infinity
        if (hasNaN && (string.Equals(text, "NaN") || text.Contains("Infinity")))
            return text;

        return text + ".0";
    }
}
/// <summary><c>ScriptLiteralStringQuoteType</c>.</summary>
public
enum ScriptLiteralStringQuoteType
{
    /// <summary><c>DoubleQuote</c>.</summary>
    DoubleQuote,
    /// <summary><c>SimpleQuote</c>.</summary>
    SimpleQuote,
    /// <summary><c>Verbatim</c>.</summary>
    Verbatim
}
