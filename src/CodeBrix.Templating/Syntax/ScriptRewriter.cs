// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;

/// <summary>
/// Base class for a script rewriter.
/// </summary>
public
abstract partial class ScriptRewriter : ScriptVisitor<ScriptNode>
{
    /// <summary><c>ScriptRewriter</c>.</summary>
    protected ScriptRewriter()
    {
        CopyTrivias = true;
    }
    /// <summary><c>CopyTrivias</c>.</summary>
    public bool CopyTrivias { get; set; }
    /// <summary><c>Visit</c>.</summary>
    public override ScriptNode Visit(ScriptNode node)
    {
        if (node is null) return null;
        var newNode = node.Accept(this);
        if (newNode is null)
        {
            return null;
        }

        newNode.Span = node.Span;
        if (CopyTrivias && !ReferenceEquals(node, newNode) && node is IScriptTerminal nodeTerminal && newNode is IScriptTerminal newNodeTerminal)
        {
            newNodeTerminal.Trivias = nodeTerminal.Trivias;
        }

        return newNode;
    }
    /// <summary><c>Visit</c>.</summary>
    public override ScriptNode Visit(ScriptVariableGlobal node)
    {
        return new ScriptVariableGlobal(node.BaseName);
    }
    /// <summary><c>Visit</c>.</summary>
    public override ScriptNode Visit(ScriptVariableLocal node)
    {
        return new ScriptVariableLocal(node.BaseName);
    }
    /// <summary><c>VisitAll</c>.</summary>
    protected ScriptList<TNode> VisitAll<TNode>(ScriptList<TNode> nodes)
        where TNode : ScriptNode
    {
        if (nodes is null)
            return null;

        var newNodes = new ScriptList<TNode>();
        foreach (var node in nodes)
        {
            var newNode = (TNode)Visit(node);
            if (newNode is not null)
            {
                newNodes.Add(newNode);
            }
        }
        return newNodes;
    }
}
