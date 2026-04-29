// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CodeBrix.Templating.Syntax;

namespace CodeBrix.Templating.Runtime; //was previously: Scriban.Runtime;

/// <summary>
/// Allows to create a custom function object.
/// </summary>
public
interface IScriptCustomFunction : IScriptFunctionInfo
{
    /// <summary>
    /// Calls the custom function object.
    /// </summary>
    /// <param name="context">The template context</param>
    /// <param name="callerContext">The script node originating this call</param>
    /// <param name="arguments">The parameters of the call</param>
    /// <param name="blockStatement">The current block statement this call is made</param>
    /// <returns>The result of the call</returns>
    object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement);

    /// <summary>
    /// Calls the custom function object asynchronously.
    /// </summary>
    /// <param name="context">The template context</param>
    /// <param name="callerContext">The script node originating this call</param>
    /// <param name="arguments">The parameters of the call</param>
    /// <param name="blockStatement">The current block statement this call is made</param>
    /// <returns>The result of the call</returns>
    ValueTask<object> InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement);
}
/// <summary><c>ScriptVarParamKind</c>.</summary>
public
enum ScriptVarParamKind
{
    /// <summary><c>None</c>.</summary>
    None,
    /// <summary><c>Direct</c>.</summary>
    Direct,
    /// <summary><c>LastParameter</c>.</summary>
    LastParameter
}
/// <summary><c>IScriptFunctionInfo</c>.</summary>
public
interface IScriptFunctionInfo
{
    /// <summary><c>RequiredParameterCount</c>.</summary>
    int RequiredParameterCount { get; }
    /// <summary><c>ParameterCount</c>.</summary>
    int ParameterCount { get; }
    /// <summary><c>VarParamKind</c>.</summary>
    ScriptVarParamKind VarParamKind { get; }
    /// <summary><c>ReturnType</c>.</summary>
    Type ReturnType { get; }
    /// <summary><c>GetParameterInfo</c>.</summary>
    ScriptParameterInfo GetParameterInfo(int index);
}
/// <summary><c>ScriptFunctionInfoExtensions</c>.</summary>
public
static class ScriptFunctionInfoExtensions
{
    /// <summary><c>IsParameterType</c>.</summary>
    public static bool IsParameterType<T>(this IScriptFunctionInfo functionInfo, int index)
    {
        var paramInfo = functionInfo.GetParameterInfo(index);
        return typeof(T).IsAssignableFrom(paramInfo.ParameterType);
    }
}
/// <summary><c>ScriptParameterInfo</c>.</summary>
[DebuggerDisplay("{ParameterType} {Name}")]
public
readonly struct ScriptParameterInfo : IEquatable<ScriptParameterInfo>
{
    /// <summary><c>ScriptParameterInfo</c>.</summary>
    public ScriptParameterInfo(Type parameterType, string name)
    {
        ParameterType = parameterType;
        Name = name;
        HasDefaultValue = false;
        DefaultValue = null;
    }
    /// <summary><c>ScriptParameterInfo</c>.</summary>
    public ScriptParameterInfo(Type parameterType, string name, object defaultValue)
    {
        ParameterType = parameterType;
        Name = name;
        HasDefaultValue = true;
        DefaultValue = defaultValue;
    }
    /// <summary><c>ParameterType</c>.</summary>
    public readonly Type ParameterType;
    /// <summary><c>Name</c>.</summary>
    public readonly string Name;
    /// <summary><c>HasDefaultValue</c>.</summary>
    public readonly bool HasDefaultValue;
    /// <summary><c>DefaultValue</c>.</summary>
    public readonly object DefaultValue;
    /// <summary><c>Equals</c>.</summary>
    public bool Equals(ScriptParameterInfo other)
    {
        return ParameterType == other.ParameterType && Name == other.Name;
    }
    /// <summary><c>Equals</c>.</summary>
    public override bool Equals(object obj)
    {
        return obj is ScriptParameterInfo other && Equals(other);
    }
    /// <summary><c>GetHashCode</c>.</summary>
    public override int GetHashCode()
    {
        unchecked
        {
            return (ParameterType.GetHashCode() * 397) ^ (Name?.GetHashCode() ?? 0);
        }
    }
    /// <summary><c>operator ==</c>.</summary>
    public static bool operator ==(ScriptParameterInfo left, ScriptParameterInfo right)
    {
        return left.Equals(right);
    }
    /// <summary><c>operator !=</c>.</summary>
    public static bool operator !=(ScriptParameterInfo left, ScriptParameterInfo right)
    {
        return !left.Equals(right);
    }
}
