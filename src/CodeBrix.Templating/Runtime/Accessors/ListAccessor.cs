// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using CodeBrix.Templating.Parsing;

namespace CodeBrix.Templating.Runtime.Accessors; //was previously: Scriban.Runtime.Accessors;
/// <summary><c>ListAccessor</c>.</summary>
public
class ListAccessor : IListAccessor, IObjectAccessor
{
    /// <summary><c>Default</c>.</summary>
    public static ListAccessor Default = new ListAccessor();

    private ListAccessor()
    {
    }
    /// <summary><c>GetLength</c>.</summary>
    public int GetLength(TemplateContext context, SourceSpan span, object target)
    {
        return ((IList) target).Count;
    }
    /// <summary><c>GetValue</c>.</summary>
    public object GetValue(TemplateContext context, SourceSpan span, object target, int index)
    {
        var list = ((IList)target);
        if (index < 0 || index >= list.Count)
        {
            return null;
        }

        return list[index];
    }
    /// <summary><c>SetValue</c>.</summary>
    public void SetValue(TemplateContext context, SourceSpan span, object target, int index, object value)
    {
        var list = ((IList) target);
        if (index < 0)
        {
            return;
        }
        // Auto-expand the array in case of accessing a range outside the current value
        for (int i = list.Count; i <= index; i++)
        {
            // TODO: If the array doesn't support null value, we shoud add a default value or throw an error?
            list.Add(null);
        }

        list[index] = value;
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
        if (target is IScriptObject)
        {
            return (((IScriptObject) target)).TryGetValue(context, span, member, out value);
        }

        value = null;
        return false;
    }
    /// <summary><c>TrySetValue</c>.</summary>
    public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value)
    {
        if (member == "size")
        {
            return false;
        }
        if (target is IScriptObject)
        {
            return (((IScriptObject)target)).TrySetValue(context, span, member, value, false);
        }
        return false;
    }
    /// <summary><c>TryGetItem</c>.</summary>
    public bool TryGetItem(TemplateContext context, SourceSpan span, object target, object index, out object value)
    {
        throw new System.NotImplementedException();
    }
    /// <summary><c>TrySetItem</c>.</summary>
    public bool TrySetItem(TemplateContext context, SourceSpan span, object target, object index, object value)
    {
        throw new System.NotImplementedException();
    }
    /// <summary><c>HasIndexer</c>.</summary>
    public bool HasIndexer => false;
    /// <summary><c>IndexType</c>.</summary>
    public Type IndexType => null;
}
