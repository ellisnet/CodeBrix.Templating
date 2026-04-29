// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using CodeBrix.Templating.Helpers;
using CodeBrix.Templating.Parsing;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptRuntimeException</c>.</summary>
public
class ScriptRuntimeException : Exception
{
    /// <summary><c>ScriptRuntimeException</c>.</summary>
    public ScriptRuntimeException(SourceSpan span, string message) : base(message)
    {
        Span = span;
    }
    /// <summary><c>ScriptRuntimeException</c>.</summary>
    public ScriptRuntimeException(SourceSpan span, string message, Exception innerException) : base(message, innerException)
    {
        Span = span;
    }
    /// <summary><c>Span</c>.</summary>
    public SourceSpan Span { get; }
    /// <summary><c>Message</c>.</summary>
    public override string Message
    {
        get
        {
            return new LogMessage(ParserMessageType.Error, Span, base.Message).ToString();
        }
    }
    /// <summary><c>EnableDisplayInnerException</c>.</summary>
    public static bool EnableDisplayInnerException
    {
        get;
        set;
    }

    /// <summary>
    /// Provides the exception message without the source span prefix.
    /// </summary>
    public string OriginalMessage
    {
        get
        {
            return base.Message;
        }
    }
    /// <summary><c>ToString</c>.</summary>
    public override string ToString()
    { 
        if (ScriptRuntimeException.EnableDisplayInnerException && InnerException is not null)
        {
            return base.ToString();
        }

        return Message;
    }
}
/// <summary><c>ScriptAbortException</c>.</summary>
public
class ScriptAbortException : ScriptRuntimeException
{
    /// <summary><c>ScriptAbortException</c>.</summary>
    public ScriptAbortException(SourceSpan span, CancellationToken cancellationToken) : this(span, "The operation was cancelled", cancellationToken)
    {
        CancellationToken = cancellationToken;
    }
    /// <summary><c>ScriptAbortException</c>.</summary>
    public ScriptAbortException(SourceSpan span, string message, CancellationToken cancellationToken) : base(span, message)
    {
        CancellationToken = cancellationToken;
    }
    /// <summary><c>CancellationToken</c>.</summary>
    public CancellationToken CancellationToken { get; }
}
/// <summary><c>ScriptParserRuntimeException</c>.</summary>
public
class ScriptParserRuntimeException : ScriptRuntimeException
{
    /// <summary><c>ScriptParserRuntimeException</c>.</summary>
    public ScriptParserRuntimeException(SourceSpan span, string message, LogMessageBag parserMessages) : this(span, message, parserMessages, null)
    {
    }
    /// <summary><c>ScriptParserRuntimeException</c>.</summary>
    public ScriptParserRuntimeException(SourceSpan span, string message, LogMessageBag parserMessages, Exception innerException) : base(span, message, innerException)
    {
        if (parserMessages is null) throw new ArgumentNullException(nameof(parserMessages));
        ParserMessages = parserMessages;
    }
    /// <summary><c>ParserMessages</c>.</summary>
    public LogMessageBag ParserMessages { get; }
    /// <summary><c>Message</c>.</summary>
    public override string Message
    {
        get
        {
            return $"{base.Message} Parser messages:\n {ParserMessages}";
        }
    }

}
