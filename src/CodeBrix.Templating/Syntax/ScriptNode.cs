// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CodeBrix.Templating.Helpers;
using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Runtime;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;

/// <summary>
/// Base class for the abstract syntax tree of a scriban program.
/// </summary>
public
abstract class ScriptNode
{
    /// <summary>
    /// The source span of this node.
    /// </summary>
    public SourceSpan Span;

    /// <summary>
    /// Gets the parent of this node.
    /// </summary>
    public ScriptNode Parent { get; internal set; }

    /// <summary>
    /// Evaluates this instance with the specified context.
    /// </summary>
    /// <param name="context">The template context.</param>
    public abstract object Evaluate(TemplateContext context);
    /// <summary><c>ChildrenCount</c>.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public virtual int ChildrenCount => 0;

    /// <summary>
    /// Clones this node including its trivias.
    /// </summary>
    /// <returns>Return a clone of this node.</returns>
    public ScriptNode Clone()
    {
        return Clone(true);
    }

    /// <summary>
    /// Clones this node.
    /// </summary>
    /// <param name="withTrivias"><c>true</c> to copy the trivias.</param>
    /// <returns>Return a clone of this node.</returns>
    public ScriptNode Clone(bool withTrivias)
    {
        var cloner = withTrivias ? ScriptCloner.WithTrivias : ScriptCloner.Instance;
        return cloner.Visit(this) ?? throw new InvalidOperationException($"Unable to clone node `{GetType()}`.");
    }

    /// <summary>
    /// Gets a children at the specified index.
    /// </summary>
    /// <param name="index">Index of the children</param>
    /// <returns>A children at the specified index</returns>
    public ScriptNode GetChildren(int index)
    {
        if (index < 0) throw ThrowHelper.GetIndexNegativeArgumentOutOfRangeException();
        if (index > ChildrenCount) throw ThrowHelper.GetIndexArgumentOutOfRangeException(ChildrenCount);
        return GetChildrenImpl(index);
    }

    /// <summary>
    /// Gets a children at the specified index.
    /// </summary>
    /// <param name="index">Index of the children</param>
    /// <returns>A children at the specified index</returns>
    /// <remarks>The index is safe to use</remarks>
    protected virtual ScriptNode GetChildrenImpl(int index) => null;
    /// <summary><c>EvaluateAsync</c>.</summary>
    public virtual ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        return new ValueTask<object>(Evaluate(context));
    }
    /// <summary><c>CanHaveLeadingTrivia</c>.</summary>
    public virtual bool CanHaveLeadingTrivia()
    {
        return true;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public abstract void PrintTo(ScriptPrinter printer);
    /// <summary><c>Accept</c>.</summary>
    public virtual void Accept(ScriptVisitor visitor) => throw new NotImplementedException($"This method must be implemented by the type {this.GetType()}");
    /// <summary><c>Accept</c>.</summary>
    [return: MaybeNull]
    public virtual TResult Accept<TResult>(ScriptVisitor<TResult> visitor) => throw new NotImplementedException($"This method must be implemented by the type {this.GetType()}");
    /// <summary><c>Children</c>.</summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public IEnumerable<ScriptNode> Children
    {
        get
        {
            var count = ChildrenCount;
            for (int i = 0; i < count; i++)
            {
                var child = GetChildrenImpl(i);
                if (child is not null)
                {
                    yield return child;
                }
            }
        }
    }

    /// <summary>
    /// Helper method to deparent/parent a node to this instance.
    /// </summary>
    /// <typeparam name="TSyntaxNode">Type of the node</typeparam>
    /// <param name="set">The previous child node parented to this instance</param>
    /// <param name="node">The new child node to parent to this instance</param>
    protected void ParentToThis<TSyntaxNode>(ref TSyntaxNode set, TSyntaxNode node) where TSyntaxNode : ScriptNode
    {
        if (node is null) throw new ArgumentNullException(nameof(node));
        if (node == set) return;
        if (node.Parent is not null) throw ThrowHelper.GetExpectingNoParentException();
        if (set is not null)
        {
            set.Parent = null;
        }
        node.Parent = this;
        set = node;
    }
    /// <summary><c>ParentToThisNullable</c>.</summary>
    protected void ParentToThisNullable<TSyntaxNode>(ref TSyntaxNode set, TSyntaxNode node) where TSyntaxNode : ScriptNode
    {
        if (node == set) return;
        if (node?.Parent is not null) throw ThrowHelper.GetExpectingNoParentException();
        if (set is not null)
        {
            set.Parent = null;
        }
        if (node is not null)
        {
            node.Parent = this;
        }
        set = node;
    }
    /// <summary><c>ToString</c>.</summary>
    public sealed override string ToString()
    {
        var strOutput = new StringBuilderOutput();
        var printer = new ScriptPrinter(strOutput , new ScriptPrinterOptions() { Mode = ScriptMode.ScriptOnly });
        printer.Write(this);
        var result = strOutput.ToString();
        strOutput.Builder.Length = 0;
        return result;
    }
}
