// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CodeBrix.Templating.Parsing; //was previously: Scriban.Parsing;

/// <summary>
/// Defines the precise source location.
/// </summary>
public
struct SourceSpan
{
    /// <summary><c>SourceSpan</c>.</summary>
    public SourceSpan(string fileName, TextPosition start, TextPosition end)
    {
        FileName = fileName;
        Start = start;
        End = end;
    }
    /// <summary><c>FileName</c>.</summary>
    public string FileName { get; set; }
    /// <summary><c>IsEmpty</c>.</summary>
    public bool IsEmpty => FileName is null;
    /// <summary><c>Start</c>.</summary>
    public TextPosition Start { get; set; }
    /// <summary><c>End</c>.</summary>
    public TextPosition End { get; set; }
    /// <summary><c>Length</c>.</summary>
    public int Length => End.Offset - Start.Offset + 1;
    /// <summary><c>ToString</c>.</summary>
    public override string ToString()
    {
        return $"{FileName}({Start})-({End})";
    }
    /// <summary><c>ToStringSimple</c>.</summary>
    public string ToStringSimple()
    {
        return $"{FileName}({Start.ToStringSimple()})";
    }
}
