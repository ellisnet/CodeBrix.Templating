// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using CodeBrix.Templating.Parsing;

namespace CodeBrix.Templating.Runtime.Accessors; //was previously: Scriban.Runtime.Accessors;
/// <summary><c>ScriptObjectAccessor</c>.</summary>
public
class ScriptObjectAccessor : IObjectAccessor
{
    /// <summary><c>Default</c>.</summary>
    public static readonly IObjectAccessor Default = new ScriptObjectAccessor();
    /// <summary><c>GetMemberCount</c>.</summary>
    public int GetMemberCount(TemplateContext context, SourceSpan span, object target)
    {
        return ((IScriptObject) target).Count;
    }
    /// <summary><c>GetMembers</c>.</summary>
    public IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span, object target)
    {
        return ((IScriptObject) target).GetMembers();
    }
    /// <summary><c>HasMember</c>.</summary>
    public bool HasMember(TemplateContext context, SourceSpan span, object target, string member)
    {
        return ((IScriptObject)target).Contains(member);
    }
    /// <summary><c>TryGetValue</c>.</summary>
    public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value)
    {
        return ((IScriptObject)target).TryGetValue(context, span, member, out value);
    }
    /// <summary><c>TrySetValue</c>.</summary>
    public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value)
    {
        return ((IScriptObject)target).TrySetValue(context, span, member, value, false);
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
