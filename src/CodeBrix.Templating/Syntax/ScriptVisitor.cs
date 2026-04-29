// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptVisitor</c>.</summary>
public
abstract partial class ScriptVisitor
{
    /// <summary><c>Visit</c>.</summary>
    public virtual void Visit(ScriptNode node)
    {
        if (node is null)
            return;

        node.Accept(this);
    }
    /// <summary><c>Visit</c>.</summary>
    public virtual void Visit(ScriptList list)
    {
        if (list is null) return;
        var count = list.ChildrenCount;
        for (int i = 0; i < count; i++)
        {
            var child = list[i];
            Visit(child);
        }
    }
    /// <summary><c>DefaultVisit</c>.</summary>
    protected virtual void DefaultVisit(ScriptNode node)
    {
        if (node is null)
            return;

        var childrenCount = node.ChildrenCount;
        for(int i = 0; i < childrenCount; i++)
        {
            var child = node.GetChildren(i);
            Visit(child);
        }
    }
}
/// <summary><c>ScriptVisitor</c>.</summary>
public
abstract partial class ScriptVisitor<TResult>
{
    /// <summary><c>Visit</c>.</summary>
    [return: MaybeNull]
    public virtual TResult Visit(ScriptNode node)
    {
        if (node is null)
            return default;

        return node.Accept(this);
    }
    /// <summary><c>DefaultVisit</c>.</summary>
    [return: MaybeNull]
    protected virtual TResult DefaultVisit(ScriptNode node)
    {
        if (node is null)
            return default;

        var childrenCount = node.ChildrenCount;
        for (int i = 0; i < childrenCount; i++)
        {
            var child = node.GetChildren(i);
            Visit(child);
        }

        return default;
    }
}
