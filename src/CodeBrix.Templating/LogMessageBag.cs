// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CodeBrix.Templating.Parsing;

namespace CodeBrix.Templating; //was previously: Scriban;

/// <summary>
/// Contains log messages.
/// </summary>
[DebuggerDisplay("Count: {Count}")]
public
class LogMessageBag : IReadOnlyList<LogMessage>
{
    private readonly List<LogMessage> _messages;
    /// <summary><c>LogMessageBag</c>.</summary>
    public LogMessageBag()
    {
        _messages = new List<LogMessage>();
    }
    /// <summary><c>Count</c>.</summary>
    public int Count => _messages.Count;
    /// <summary><c>this[int]</c>.</summary>
    public LogMessage this[int index] => _messages[index];
    /// <summary><c>HasErrors</c>.</summary>
    public bool HasErrors { get; private set; }
    /// <summary><c>Add</c>.</summary>
    public void Add(LogMessage message)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));

        if (message.Type == ParserMessageType.Error)
        {
            HasErrors = true;
        }

        _messages.Add(message);
    }
    /// <summary><c>AddRange</c>.</summary>
    public void AddRange(IEnumerable<LogMessage> messages)
    {
        if (messages is null) throw new ArgumentNullException(nameof(messages));
        foreach (var message in messages)
        {
            Add(message);
        }
    }
    /// <summary><c>GetEnumerator</c>.</summary>
    public IEnumerator<LogMessage> GetEnumerator()
    {
        return _messages.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable) _messages).GetEnumerator();
    }
    /// <summary><c>ToString</c>.</summary>
    public override string ToString()
    {
        var builder = new StringBuilder();
        foreach (var message in _messages)
        {
            builder.AppendLine(message.ToString());
        }
        return builder.ToString();
    }
}
