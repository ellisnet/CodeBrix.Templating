// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.Templating.Syntax;

namespace CodeBrix.Templating.Runtime; //was previously: Scriban.Runtime;

/// <summary>
/// Interface used to text output when evaluating a template used by <see cref="TemplateContext.Output"/> and <see cref="TemplateContext.PushOutput()"/>
/// </summary>
public
interface IScriptOutput
{
    /// <summary><c>Write</c>.</summary>
    void Write(string text, int offset, int count);
    /// <summary><c>WriteAsync</c>.</summary>
    ValueTask WriteAsync(string text, int offset, int count, CancellationToken cancellationToken);
}

/// <summary>
/// Extensions for <see cref="IScriptOutput"/>
/// </summary>
public
static partial class ScriptOutputExtensions
{
    /// <summary><c>Write</c>.</summary>
    public static void Write(this IScriptOutput scriptOutput, string text)
    {
        if (text is null) throw new ArgumentNullException(nameof(text));
        scriptOutput.Write(text, 0, text.Length);
    }
    /// <summary><c>Write</c>.</summary>
    public static void Write(this IScriptOutput scriptOutput, ScriptStringSlice text)
    {
        if (text.FullText is null) throw new ArgumentNullException(nameof(text));
        if (text.Length == 0) return;
        scriptOutput.Write(text.FullText, text.Index, text.Length);
    }
}
