// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeBrix.Templating.Helpers;
using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Runtime;
using System.Threading.Tasks;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;

/// <summary>
/// Base class for a loop statement
/// </summary>
public
abstract partial class ScriptLoopStatementBase : ScriptStatement
{
    /// <summary><c>BeforeLoop</c>.</summary>
    protected virtual void BeforeLoop(TemplateContext context)
    {
    }

    /// <summary>
    /// Base implementation for a loop single iteration
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="state">The state of the loop</param>
    /// <returns></returns>
    protected abstract object LoopItem(TemplateContext context, LoopState state);
    /// <summary><c>CreateLoopState</c>.</summary>
    protected virtual LoopState CreateLoopState() {  return new LoopState(); }
    /// <summary><c>ContinueLoop</c>.</summary>
    protected bool ContinueLoop(TemplateContext context)
    {
        // Return must bubble up to call site
        if (context.FlowState == ScriptFlowState.Return)
        {
            return false;
        }

        // If we need to break, restore to none state
        var result = context.FlowState != ScriptFlowState.Break;
        context.FlowState = ScriptFlowState.None;
        return result;
    }
    /// <summary><c>AfterLoop</c>.</summary>
    protected virtual void AfterLoop(TemplateContext context)
    {
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        // Notify the context that we enter a loop block (used for variable with scope Loop)
        object result = null;
        context.EnterLoop(this);
        try
        {
            result = EvaluateImpl(context);
        }
        finally
        {
            // Level scope block
            context.ExitLoop(this);

            if (context.FlowState != ScriptFlowState.Return)
            {
                // Revert to flow state to none unless we have a return that must be handled at a higher level
                context.FlowState = ScriptFlowState.None;
            }
        }
        return result;
    }
    /// <summary><c>EvaluateImpl</c>.</summary>
    protected abstract object EvaluateImpl(TemplateContext context);

#if !SCRIBAN_NO_ASYNC
    /// <summary><c>EvaluateImplAsync</c>.</summary>
    protected abstract ValueTask<object> EvaluateImplAsync(TemplateContext context);
    /// <summary><c>LoopItemAsync</c>.</summary>
    protected abstract ValueTask<object> LoopItemAsync(TemplateContext context, LoopState state);
    /// <summary><c>BeforeLoopAsync</c>.</summary>
    protected virtual ValueTask BeforeLoopAsync(TemplateContext context)
    {
        return new ValueTask();
    }
    /// <summary><c>AfterLoopAsync</c>.</summary>
    protected virtual ValueTask AfterLoopAsync(TemplateContext context)
    {
        return new ValueTask();
    }
#endif

    /// <summary>
    /// Store the loop state
    /// </summary>
    protected class LoopState : IScriptObject
    {
        private int _length;
        private object _lengthObject;
        private IEnumerable _list;
        private IEnumerator _it;
        private bool _isLast;
        private bool _isLastTaken;
        /// <summary><c>SetEnumerable</c>.</summary>
        public void SetEnumerable(IEnumerable list, IEnumerator it)
        {
            _list = list;
            _it = it;
        }
        /// <summary><c>Index</c>.</summary>
        public int Index { get; set; }
        /// <summary><c>IsFirst</c>.</summary>
        public bool IsFirst => Index == 0;
        /// <summary><c>IsEven</c>.</summary>
        public bool IsEven => (Index & 1) == 0;
        /// <summary><c>IsOdd</c>.</summary>
        public bool IsOdd => !IsEven;
        /// <summary><c>ValueChanged</c>.</summary>
        public bool ValueChanged { get; set; }
        /// <summary><c>ResetLast</c>.</summary>
        public void ResetLast() => _isLastTaken = false;
        /// <summary><c>MoveNextAndIsLast</c>.</summary>
        public bool MoveNextAndIsLast()
        {
            if (_it is null) return false;

            if (!_isLastTaken)
            {
                _isLast = !_it.MoveNext();
                _isLastTaken = true;
            }

            return _isLast;
        }
        /// <summary><c>Length</c>.</summary>
        public int Length
        {
            get
            {
                if (_lengthObject is null)
                {
                    _length = _list is IList list ? list.Count : _list?.Cast<object>().Count() ?? 0;
                    _lengthObject = _length;
                }
                return _length;
            }
        }
        /// <summary><c>Count</c>.</summary>
        public int Count { get; set; }
        /// <summary><c>GetMembers</c>.</summary>
        public IEnumerable<string> GetMembers()
        {
            return Enumerable.Empty<string>();
        }
        /// <summary><c>Contains</c>.</summary>
        public virtual bool Contains(string member)
        {
            switch (member)
            {
                case "index":
                case "index0":
                case "first":
                case "even":
                case "odd":
                case "last":
                case "length":
                case "rindex":
                case "rindex0":
                case "changed":
                    return true;
            }
            return false;
        }
        /// <summary><c>IsReadOnly</c>.</summary>
        public bool IsReadOnly { get; set; }
        /// <summary><c>TryGetValue</c>.</summary>
        public virtual bool TryGetValue(TemplateContext context, SourceSpan span, string member, out object value)
        {
            value = null;
            var isLiquid = context.IsLiquid;
            switch (member)
            {
                case "index":
                    value = isLiquid ? Index + 1 : Index;
                    return true;
                case "length":
                    value = Length;
                    return true;
                case "first":
                    value = IsFirst ? BoxHelper.TrueObject : BoxHelper.FalseObject;
                    return true;
                case "even":
                    value = IsEven ? BoxHelper.TrueObject : BoxHelper.FalseObject;
                    return true;
                case "odd":
                    value = IsOdd ? BoxHelper.TrueObject : BoxHelper.FalseObject;
                    return true;
                case "last":
                    value = MoveNextAndIsLast() ? BoxHelper.TrueObject : BoxHelper.FalseObject;
                    return true;
                case "changed":
                    value = ValueChanged ? BoxHelper.TrueObject : BoxHelper.FalseObject;
                    return true;
                case "rindex":
                    value = isLiquid ? Length - Index : Length - Index - 1;
                    return true;
                default:
                    if (isLiquid)
                    {
                        if (member == "index0")
                        {
                            value = Index;
                            return true;
                        }
                        if (member == "rindex0")
                        {
                            value = Length - Index - 1;
                            return true;
                        }
                    }
                    return false;
            }
        }
        /// <summary><c>CanWrite</c>.</summary>
        public bool CanWrite(string member)
        {
            throw new System.NotImplementedException();
        }
        /// <summary><c>TrySetValue</c>.</summary>
        public bool TrySetValue(TemplateContext context, SourceSpan span, string member, object value, bool readOnly)
        {
            return false;
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
            return (IScriptObject)MemberwiseClone();
        }
    }
}
