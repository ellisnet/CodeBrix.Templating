// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CodeBrix.Templating.Helpers;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;

/// <summary>
/// Abstract list of <see cref="ScriptNode"/>
/// </summary>
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(ScriptListDebug))]
public
abstract class ScriptList : ScriptNode
{
    internal InlineList<ScriptNode> _children;

    internal ScriptList()
    {
        _children = new InlineList<ScriptNode>(0);
    }
    /// <summary><c>Count</c>.</summary>
    public int Count => _children.Count;
    /// <summary><c>ChildrenCount</c>.</summary>
    public sealed override int ChildrenCount => _children.Count;
    /// <summary><c>this[int]</c>.</summary>
    public ScriptNode this[int index] => _children[index];
    /// <summary><c>GetChildrenImpl</c>.</summary>
    protected override ScriptNode GetChildrenImpl(int index)
    {
        return _children[index];
    }

    private sealed class ScriptListDebug
    {
        private readonly InlineList<ScriptNode> _children;

        public ScriptListDebug(ScriptList list)
        {
            _children = list._children;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public ScriptNode[] Items => _children.ToArray();
    }
}

/// <summary>
/// Abstract list of <see cref="ScriptNode"/>
/// </summary>
/// <typeparam name="TScriptNode">Type of the node</typeparam>
[DebuggerTypeProxy(typeof(ScriptList<>.DebugListView)), DebuggerDisplay("Count = {Count}")]
public
sealed class ScriptList<TScriptNode> : ScriptList, IList<TScriptNode>, IReadOnlyList<TScriptNode> where TScriptNode : ScriptNode
{
    /// <summary>
    /// Creates an instance of <see cref="ScriptList{TScriptNode}"/>
    /// </summary>
    public ScriptList()
    {
    }

    /// <summary>
    /// Adds the specified node to this list.
    /// </summary>
    /// <param name="node">Node to add to this list</param>
    public void Add(TScriptNode node)
    {
        if (node is null) throw new ArgumentNullException(nameof(node));
        if (node.Parent is not null) throw ThrowHelper.GetExpectingNoParentException();
        _children.Add(node);
        node.Parent = this;
    }
    /// <summary><c>AddRange</c>.</summary>
    public void AddRange(IEnumerable<TScriptNode> nodes)
    {
        if (nodes is null) throw new ArgumentNullException(nameof(nodes));
        foreach (var node in nodes)
        {
            Add(node);
        }
    }
    /// <summary><c>Clear</c>.</summary>
    public void Clear()
    {
        var children = _children;
        var items = children.Items ?? Array.Empty<ScriptNode>();
        for(int i = 0; i < children.Count; i++)
        {
            var item = items[i];
            item.Parent = null;
        }
        _children.Clear();
    }
    /// <summary><c>Contains</c>.</summary>
    public bool Contains(TScriptNode item)
    {
        return _children.Contains(item);
    }
    /// <summary><c>CopyTo</c>.</summary>
    public void CopyTo(TScriptNode[] array, int arrayIndex)
    {
        _children.CopyTo((ScriptNode[])array, arrayIndex);
    }
    /// <summary><c>Remove</c>.</summary>
    public bool Remove(TScriptNode item)
    {
        if (item is null) throw new ArgumentNullException(nameof(item));
        if (_children.Remove(item))
        {
            item.Parent = null;
            return true;
        }

        return false;
    }
    /// <summary><c>IsReadOnly</c>.</summary>
    public bool IsReadOnly => false;
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        throw new InvalidOperationException("A list cannot be evaluated.");
    }
    /// <summary><c>GetChildren</c>.</summary>
    public new TScriptNode GetChildren(int index)
    {
        return (TScriptNode)base.GetChildren(index);
    }
    /// <summary><c>GetChildrenImpl</c>.</summary>
    protected override ScriptNode GetChildrenImpl(int index)
    {
        return _children[index];
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        throw new NotImplementedException();
    }
    /// <summary><c>Accept</c>.</summary>
    public override void Accept(ScriptVisitor visitor)
    {
        visitor.Visit(this);
    }
    /// <summary><c>Accept</c>.</summary>
    [return: MaybeNull]
    public override TResult Accept<TResult>(ScriptVisitor<TResult> visitor)
    {
        return visitor.Visit(this);
    }

    /// <summary>
    /// Gets the default enumerator.
    /// </summary>
    /// <returns>The enumerator of this list</returns>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(_children.Items ?? Array.Empty<ScriptNode>(), _children.Count);
    }

    IEnumerator<TScriptNode> IEnumerable<TScriptNode>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Enumerator of a <see cref="ScriptList{TScriptNode}"/>
    /// </summary>
    public struct Enumerator : IEnumerator<TScriptNode>
    {
        private readonly ScriptNode[] _nodes;
        private readonly int _count;
        private int _index;

        /// <summary>
        /// Initialize an enumerator with a list of <see cref="ScriptNode"/>
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="count"></param>
        public Enumerator(ScriptNode[] nodes, int count)
        {
            _nodes = nodes;
            _count = count;
            _index = -1;
        }
        /// <summary><c>MoveNext</c>.</summary>
        public bool MoveNext()
        {
            if (_index + 1 == _count) return false;
            _index++;
            return true;
        }
        /// <summary><c>Reset</c>.</summary>
        public void Reset()
        {
            _index = -1;
        }
        /// <summary><c>Current</c>.</summary>
        public TScriptNode Current
        {
            get
            {
                if (_index < 0) throw new InvalidOperationException("MoveNext must be called before accessing Current");
                return (TScriptNode)(_nodes[_index]);
            }
        }

        object IEnumerator.Current => Current;
        /// <summary><c>Dispose</c>.</summary>
        public void Dispose()
        {
        }
    }
    /// <summary><c>IndexOf</c>.</summary>
    public int IndexOf(TScriptNode item)
    {
        return _children.IndexOf(item);
    }
    /// <summary><c>Insert</c>.</summary>
    public void Insert(int index, TScriptNode item)
    {
        AssertNoParent(item);
        _children.Insert(index, item);
        if (item is not null)
        {
            item.Parent = this;
        }
    }
    /// <summary><c>RemoveAt</c>.</summary>
    public void RemoveAt(int index)
    {
        var previous = _children[index];
        _children.RemoveAt(index);
        if (previous is not null) previous.Parent = null;
    }
    /// <summary><c>this[int]</c>.</summary>
    public new TScriptNode this[int index]
    {
        get => (TScriptNode)_children[index];
        set
        {
            var previous = _children[index];
            if (previous == value) return;
            AssertNoParent(value);
            _children[index] = value;
            if (previous is not null) previous.Parent = null;
        }
    }

    private void AssertNoParent(ScriptNode node)
    {
        if (node is not null && node.Parent is not null) throw new ArgumentException("Cannot add this node which is already attached to another list instance");
    }

    internal class DebugListView
    {
        private readonly ScriptList<TScriptNode> _collection;

        public DebugListView(ScriptList<TScriptNode> collection)
        {
            this._collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TScriptNode[] Items
        {
            get
            {
                var array = new TScriptNode[this._collection.Count];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = _collection[i];
                }
                return array;
            }
        }
    }
}
