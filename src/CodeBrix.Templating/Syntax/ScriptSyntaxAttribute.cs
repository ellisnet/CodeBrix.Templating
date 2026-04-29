// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Reflection;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptTypeNameAttribute</c>.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct)]
public
class ScriptTypeNameAttribute : Attribute
{
    /// <summary><c>ScriptTypeNameAttribute</c>.</summary>
    public ScriptTypeNameAttribute(string typeName)
    {
        TypeName = typeName;
    }
    /// <summary><c>TypeName</c>.</summary>
    public string TypeName { get; }
}
/// <summary><c>ScriptSyntaxAttribute</c>.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct)]
public
class ScriptSyntaxAttribute : ScriptTypeNameAttribute
{
    /// <summary><c>ScriptSyntaxAttribute</c>.</summary>
    public ScriptSyntaxAttribute(string typeName, string example) : base(typeName)
    {
        Example = example;
    }
    /// <summary><c>Example</c>.</summary>
    public string Example { get; }
    /// <summary><c>Get</c>.</summary>
    public static ScriptSyntaxAttribute Get(object obj)
    {
        if (obj is null) return null;
        return Get(obj.GetType());
    }
    /// <summary><c>Get</c>.</summary>
    public static ScriptSyntaxAttribute Get(Type type)
    {
        if (type is null) throw new ArgumentNullException(nameof(type));

        var attribute = type.GetCustomAttribute<ScriptSyntaxAttribute>() ??
                        new ScriptSyntaxAttribute(type.Name, "...");
        return attribute;
    }
}
