// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Diagnostics;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;

/// <summary>
/// Slice of a string
/// </summary>
[DebuggerDisplay("{ToString()}")]
public
readonly struct ScriptStringSlice : IEquatable<ScriptStringSlice>, IComparable<ScriptStringSlice>, IComparable<string>
{
    /// <summary><c>Empty</c>.</summary>
    public static readonly ScriptStringSlice Empty = new ScriptStringSlice(string.Empty);
    /// <summary><c>ScriptStringSlice</c>.</summary>
    public ScriptStringSlice(string fullText)
    {
        FullText = fullText;
        Index = 0;
        Length = fullText?.Length ?? 0;
    }
    /// <summary><c>ScriptStringSlice</c>.</summary>
    public ScriptStringSlice(string fullText, int index, int length)
    {
        if (index < 0 || fullText is not null && index >= fullText.Length) throw new ArgumentOutOfRangeException(nameof(index));
        if (length < 0 || fullText is not null && index + length > fullText.Length) throw new ArgumentOutOfRangeException(nameof(length));
        FullText = fullText;
        Index = index;
        Length = length;
    }

    /// <summary>
    /// The text of this slice.
    /// </summary>
    public readonly string FullText;

    /// <summary>
    /// Index into the text
    /// </summary>
    public readonly int Index;

    /// <summary>
    /// Length of the slice
    /// </summary>
    public readonly int Length;
    /// <summary><c>this[int]</c>.</summary>
    public char this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Length) throw new ArgumentOutOfRangeException(nameof(index));
            return (FullText ?? string.Empty)[Index + index];
        }
    }
    /// <summary><c>Substring</c>.</summary>
    public string Substring(int index)
    {
        if (index == Length) return "";
        if ((uint)index > (uint)Length) throw new ArgumentOutOfRangeException(nameof(index));
        return FullText?.Substring(Index + index, Length - index) ?? string.Empty;
    }
    /// <summary><c>ToString</c>.</summary>
    public override string ToString()
    {
        return FullText?.Substring(Index, Length) ?? string.Empty;
    }
    /// <summary><c>Equals</c>.</summary>
    public bool Equals(ScriptStringSlice other)
    {
        if (Length != other.Length) return false;
        if (FullText is null && other.FullText is null) return true;
        if (FullText is null || other.FullText is null) return false;
        return string.CompareOrdinal(FullText, Index, other.FullText, other.Index, Length) == 0;
    }
    /// <summary><c>Equals</c>.</summary>
    public override bool Equals(object obj)
    {
        return obj is ScriptStringSlice other && Equals(other);
    }
    /// <summary><c>GetHashCode</c>.</summary>
    public override int GetHashCode()
    {
        unchecked
        {
            if (FullText is null) return 0;

            // TODO: optimize with Span for >= netstandard 2.1
            var hashCode = Length;
            for (int i = Index; i < Length; i++)
            {
                hashCode = (hashCode * 397) ^ FullText[i];
            }

            return hashCode;
        }
    }
    /// <summary><c>operator ==</c>.</summary>
    public static bool operator ==(ScriptStringSlice left, ScriptStringSlice right) => left.Equals(right);
    /// <summary><c>operator !=</c>.</summary>
    public static bool operator !=(ScriptStringSlice left, ScriptStringSlice right) => !left.Equals(right);
    /// <summary><c>operator ==</c>.</summary>
    public static bool operator ==(ScriptStringSlice left, string right) => left.CompareTo(right) == 0;
    /// <summary><c>operator !=</c>.</summary>
    public static bool operator !=(ScriptStringSlice left, string right) => left.CompareTo(right) != 0;
    /// <summary><c>operator ==</c>.</summary>
    public static bool operator ==(string left, ScriptStringSlice right) => right.CompareTo(left) == 0;
    /// <summary><c>operator !=</c>.</summary>
    public static bool operator !=(string left, ScriptStringSlice right) => right.CompareTo(left) != 0;
    /// <summary><c>CompareTo</c>.</summary>
    public int CompareTo(ScriptStringSlice other)
    {
        if (FullText is null || other.FullText is null)
        {
            if (object.ReferenceEquals(FullText, other.FullText))
            {
                return 0;
            }

            return FullText is null ? -1 : 1;
        }

        if (Length == 0 && other.Length == 0) return 0;


        var minLength = Math.Min(Length, other.Length);

        var textComparison = string.CompareOrdinal(FullText, Index, other.FullText, other.Index, minLength);
        return textComparison != 0 ? textComparison < 0 ? -1 : 1 : Length.CompareTo(other.Length);
    }
    /// <summary><c>CompareTo</c>.</summary>
    public int CompareTo(string other)
    {
        if (FullText is null || other is null)
        {
            if (object.ReferenceEquals(FullText, other))
            {
                return 0;
            }

            return FullText is null ? -1 : 1;
        }

        if (Length == 0 && other.Length == 0) return 0;


        var minLength = Math.Min(Length, other.Length);

        var textComparison = string.CompareOrdinal(FullText, Index, other, 0, minLength);
        return textComparison != 0 ?  textComparison < 0 ? -1 : 1 : Length.CompareTo(other.Length);
    }
    /// <summary><c>explicit operator ScriptStringSlice</c>.</summary>
    public static explicit operator ScriptStringSlice(string text) => new ScriptStringSlice(text);
    /// <summary><c>explicit operator string</c>.</summary>
    public static explicit operator string(ScriptStringSlice slice) => slice.ToString() ?? string.Empty;
    /// <summary><c>TrimStart</c>.</summary>
    public ScriptStringSlice TrimStart()
    {
        var text = FullText;
        if (text is null)
        {
            return Empty;
        }
        for (int i = 0; i < Length; i++)
        {
            var c = text[Index + i];
            if (!char.IsWhiteSpace(c))
            {
                return new ScriptStringSlice(text, Index + i, Length - i);
            }
        }
        return Empty;
    }
    /// <summary><c>TrimEnd</c>.</summary>
    public ScriptStringSlice TrimEnd()
    {
        var text = FullText;
        if (text is null)
        {
            return Empty;
        }
        for (int i = Length - 1; i >= 0; i--)
        {
            var c = text[Index + i];
            if (!char.IsWhiteSpace(c))
            {
                return new ScriptStringSlice(text, Index, i + 1);
            }
        }

        return Empty;
    }
    /// <summary><c>TrimEndKeepNewLine</c>.</summary>
    public ScriptStringSlice TrimEndKeepNewLine()
    {
        var text = FullText;
        if (text is null)
        {
            return Empty;
        }
        for (int i = Length - 1; i >= 0; i--)
        {
            var c = text[Index + i];
            if (!char.IsWhiteSpace(c) || c == '\n')
            {
                return new ScriptStringSlice(text, Index, i + 1);
            }
        }

        return Empty;
    }
}
/// <summary><c>ScriptStringSliceExtensions</c>.</summary>
public
static class ScriptStringSliceExtensions
{
    /// <summary><c>Slice</c>.</summary>
    public static ScriptStringSlice Slice(this string text, int index)
    {
        return new ScriptStringSlice(text, index, text.Length - index);
    }
    /// <summary><c>Slice</c>.</summary>
    public static ScriptStringSlice Slice(this string text, int index, int length)
    {
        return new ScriptStringSlice(text, index, length);
    }
}
