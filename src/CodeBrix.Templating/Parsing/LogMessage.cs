// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Text;

namespace CodeBrix.Templating.Parsing; //was previously: Scriban.Parsing;
/// <summary><c>LogMessage</c>.</summary>
public
class LogMessage
{
    /// <summary><c>LogMessage</c>.</summary>
    public LogMessage(ParserMessageType type, SourceSpan span, string message)
    {
        Type = type;
        Span = span;
        Message = message;
    }
    /// <summary><c>Type</c>.</summary>
    public ParserMessageType Type { get; set; }
    /// <summary><c>Span</c>.</summary>
    public SourceSpan Span { get; set; }
    /// <summary><c>Message</c>.</summary>
    public string Message { get; set; }
    /// <summary><c>ToString</c>.</summary>
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(Span.ToStringSimple());
        builder.Append(" : ");
        builder.Append(Type.ToString().ToLowerInvariant());
        builder.Append(" : ");
        if (Message is not null)
        {
            builder.Append(Message);
        }
        return builder.ToString();
    }
}
/// <summary><c>ParserMessageType</c>.</summary>
public
enum ParserMessageType
{
    /// <summary><c>Error</c>.</summary>
    Error,
    /// <summary><c>Warning</c>.</summary>
    Warning,
}
