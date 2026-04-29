// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System;
using CodeBrix.Templating.Syntax;

namespace CodeBrix.Templating.Runtime; //was previously: Scriban.Runtime;
/// <summary><c>ScriptLazy</c>.</summary>
public
class ScriptLazy<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T> : IScriptCustomFunction
{
    private readonly Lazy<T> _lazy;
    /// <summary><c>ScriptLazy</c>.</summary>
    public ScriptLazy(Func<T> valueFactory)
    {
        _lazy = new Lazy<T>(valueFactory);
    }
    /// <summary><c>RequiredParameterCount</c>.</summary>
    public int RequiredParameterCount => 0;
    /// <summary><c>ParameterCount</c>.</summary>
    public int ParameterCount => 0;
    /// <summary><c>VarParamKind</c>.</summary>
    public ScriptVarParamKind VarParamKind => ScriptVarParamKind.None;
    /// <summary><c>ReturnType</c>.</summary>
    public Type ReturnType => typeof(T);
    /// <summary><c>GetParameterInfo</c>.</summary>
    public ScriptParameterInfo GetParameterInfo(int index) => default;
    /// <summary><c>Invoke</c>.</summary>
    public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
    {
        return _lazy.Value;
    }
    /// <summary><c>InvokeAsync</c>.</summary>
    public ValueTask<object> InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
    {
        return new ValueTask<object>(_lazy.Value);
    }
}
