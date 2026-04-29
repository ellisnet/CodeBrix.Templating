// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using CodeBrix.Templating.Parsing;

namespace CodeBrix.Templating.Runtime.Accessors; //was previously: Scriban.Runtime.Accessors;
/// <summary><c>NullAccessor</c>.</summary>
public
class NullAccessor : IObjectAccessor
{
    /// <summary><c>Default</c>.</summary>
    public static readonly NullAccessor Default = new NullAccessor();
    /// <summary><c>GetMemberCount</c>.</summary>
    public int GetMemberCount(TemplateContext context, SourceSpan span, object target)
    {
        return 0;
    }
    /// <summary><c>GetMembers</c>.</summary>
    public IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span, object target)
    {
        yield break;
    }
    /// <summary><c>HasMember</c>.</summary>
    public bool HasMember(TemplateContext context, SourceSpan span, object target, string member)
    {
        return false;
    }
    /// <summary><c>TryGetValue</c>.</summary>
    public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value)
    {
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
        value = null;
        return false;
    }
    /// <summary><c>TrySetItem</c>.</summary>
    public bool TrySetItem(TemplateContext context, SourceSpan span, object target, object index, object value)
    {
        return false;
    }
    /// <summary><c>HasIndexer</c>.</summary>
    public bool HasIndexer => false;
    /// <summary><c>IndexType</c>.</summary>
    public Type IndexType => null;
}
