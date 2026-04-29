// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CodeBrix.Templating.Parsing; //was previously: Scriban.Parsing;
/// <summary><c>TextPosition</c>.</summary>
public
struct TextPosition : IEquatable<TextPosition>
{
    /// <summary><c>Eof</c>.</summary>
    public static readonly TextPosition Eof = new TextPosition(-1, -1, -1);
    /// <summary><c>TextPosition</c>.</summary>
    public TextPosition(int offset, int line, int column)
    {
        Offset = offset;
        Column = column;
        Line = line;
    }
    /// <summary><c>Offset</c>.</summary>
    public int Offset { get; set; }
    /// <summary><c>Column</c>.</summary>
    public int Column { get; set; }
    /// <summary><c>Line</c>.</summary>
    public int Line { get; set; }
    /// <summary><c>NextColumn</c>.</summary>
    public TextPosition NextColumn(int offset = 1)
    {
        return new TextPosition(Offset + offset, Line, Column + offset);
    }
    /// <summary><c>NextLine</c>.</summary>
    public TextPosition NextLine(int offset = 1)
    {
        return new TextPosition(Offset + offset, Line + offset, 0);
    }
    /// <summary><c>ToString</c>.</summary>
    public override string ToString()
    {
        return $"({Offset}:{Line},{Column})";
    }
    /// <summary><c>ToStringSimple</c>.</summary>
    public string ToStringSimple()
    {
        return $"{Line+1},{Column+1}";
    }
    /// <summary><c>Equals</c>.</summary>
    public bool Equals(TextPosition other)
    {
        return Offset == other.Offset && Column == other.Column && Line == other.Line;
    }
    /// <summary><c>Equals</c>.</summary>
    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        return obj is TextPosition && Equals((TextPosition) obj);
    }
    /// <summary><c>GetHashCode</c>.</summary>
    public override int GetHashCode()
    {
        return Offset;
    }
    /// <summary><c>operator ==</c>.</summary>
    public static bool operator ==(TextPosition left, TextPosition right)
    {
        return left.Equals(right);
    }
    /// <summary><c>operator !=</c>.</summary>
    public static bool operator !=(TextPosition left, TextPosition right)
    {
        return !left.Equals(right);
    }
}
