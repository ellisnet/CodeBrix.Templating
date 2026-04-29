// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Syntax;

namespace CodeBrix.Templating.Runtime.Accessors; //was previously: Scriban.Runtime.Accessors;
/// <summary><c>StringAccessor</c>.</summary>
public
class StringAccessor : IListAccessor, IObjectAccessor
{
    /// <summary><c>Default</c>.</summary>
    public static StringAccessor Default = new StringAccessor();

    private StringAccessor()
    {
    }
    /// <summary><c>GetLength</c>.</summary>
    public int GetLength(TemplateContext context, SourceSpan span, object target)
    {
        return ((string)target).Length;
    }
    /// <summary><c>GetValue</c>.</summary>
    public object GetValue(TemplateContext context, SourceSpan span, object target, int index)
    {
        return ((string)target)[index].ToString();
    }
    /// <summary><c>SetValue</c>.</summary>
    public void SetValue(TemplateContext context, SourceSpan span, object target, int index, object value)
    {
        throw new ScriptRuntimeException(span, "Cannot replace a character in a string");
    }
    /// <summary><c>GetMemberCount</c>.</summary>
    public int GetMemberCount(TemplateContext context, SourceSpan span, object target)
    {
        // size
        return 1;
    }
    /// <summary><c>GetMembers</c>.</summary>
    public IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span, object target)
    {
        yield return "size";
    }
    /// <summary><c>HasMember</c>.</summary>
    public bool HasMember(TemplateContext context, SourceSpan span, object target, string member)
    {
        return member == "size";
    }
    /// <summary><c>TryGetValue</c>.</summary>
    public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value)
    {
        if (member == "size")
        {
            value = GetLength(context, span, target);
            return true;
        }
        value = null;
        return false;
    }
    /// <summary><c>TrySetValue</c>.</summary>
    public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value)
    {
        return false;
    }
    /// <summary><c>TryGetItem</c>.</summary>
    public bool TryGetItem(TemplateContext context, SourceSpan span, object target, object index, out object value)
    {
        throw new NotImplementedException();
    }
    /// <summary><c>TrySetItem</c>.</summary>
    public bool TrySetItem(TemplateContext context, SourceSpan span, object target, object index, object value)
    {
        throw new NotImplementedException();
    }
    /// <summary><c>HasIndexer</c>.</summary>
    public bool HasIndexer => false;
    /// <summary><c>IndexType</c>.</summary>
    public Type IndexType => null;
}
