// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Syntax;

namespace CodeBrix.Templating.Runtime; //was previously: Scriban.Runtime;

/// <summary>
/// The empty object (unique singleton, cannot be modified, does not contain any properties)
/// </summary>
[DebuggerDisplay("<empty object>")]
public
sealed class EmptyScriptObject : IScriptObject
{
    /// <summary><c>Default</c>.</summary>
    public static readonly EmptyScriptObject Default = new EmptyScriptObject();

    private EmptyScriptObject()
    {
    }
    /// <summary><c>Count</c>.</summary>
    public int Count => 0;
    /// <summary><c>GetMembers</c>.</summary>
    public IEnumerable<string> GetMembers()
    {
        yield break;
    }
    /// <summary><c>Contains</c>.</summary>
    public bool Contains(string member)
    {
        return false;
    }
    /// <summary><c>IsReadOnly</c>.</summary>
    public bool IsReadOnly
    {
        get => true;
        set { }
    }
    /// <summary><c>TryGetValue</c>.</summary>
    public bool TryGetValue(TemplateContext context, SourceSpan span, string member, out object value)
    {
        value = null;
        return false;
    }
    /// <summary><c>CanWrite</c>.</summary>
    public bool CanWrite(string member)
    {
        return false;
    }
    /// <summary><c>TrySetValue</c>.</summary>
    public bool TrySetValue(TemplateContext context, SourceSpan span, string member, object value, bool readOnly)
    {
        throw new ScriptRuntimeException(span, "Cannot set a property on the empty object");
    }
    /// <summary><c>Remove</c>.</summary>
    public bool Remove(string member)
    {
        return false;
    }
    /// <summary><c>SetReadOnly</c>.</summary>
    public void SetReadOnly(string member, bool readOnly)
    {
    }
    /// <summary><c>Clone</c>.</summary>
    public IScriptObject Clone(bool deep)
    {
        return this;
    }
    /// <summary><c>ToString</c>.</summary>
    public override string ToString()
    {
        return string.Empty;
    }
}
