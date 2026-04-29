// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using CodeBrix.Templating.Runtime;
using CodeBrix.Templating.Syntax;
using Enum = System.Enum;

namespace CodeBrix.Templating.Helpers; //was previously: Scriban.Helpers;
/// <summary><c>ReflectionHelper</c>.</summary>
public
static class ReflectionHelper
{
    /// <summary><c>IsPrimitiveOrDecimal</c>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPrimitiveOrDecimal(this Type type)
    {
        return type.IsPrimitive || type == typeof(decimal) || type == typeof(BigInteger);
    }
    /// <summary><c>IsNumber</c>.</summary>
    public static bool IsNumber(this Type type)
    {
        return (type.IsPrimitive && type != typeof(bool)) || type == typeof(decimal) || type == typeof(BigInteger);
    }

    internal static Type GetBaseOrInterface([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] this Type type, Type lookInterfaceType)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        if (lookInterfaceType is null)
            throw new ArgumentNullException(nameof(lookInterfaceType));

        if (lookInterfaceType.IsGenericTypeDefinition)
        {
            if (lookInterfaceType.IsInterface)
                foreach (var interfaceType in type.GetInterfaces())
                    if (interfaceType.IsGenericType
                        && interfaceType.GetGenericTypeDefinition()  == lookInterfaceType)
                        return interfaceType;

            for (var t = type; t is not null; t = t.BaseType)
                if (t.IsGenericType && t.GetGenericTypeDefinition() == lookInterfaceType)
                    return t;
        }
        else
        {
            if (lookInterfaceType.IsAssignableFrom(type))
                return lookInterfaceType;
        }

        return null;
    }
    /// <summary><c>ScriptPrettyName</c>.</summary>
    public static string ScriptPrettyName(this Type type)
    {
        if (type is null) return "null";

        if (type == typeof(bool)) return "bool";
        if (type == typeof(byte)) return "byte";
        if (type == typeof(sbyte)) return "sbyte";
        if (type == typeof(ushort)) return "ushort";
        if (type == typeof(short)) return "short";
        if (type == typeof(uint)) return "uint";
        if (type == typeof(int)) return "int";
        if (type == typeof(ulong)) return "ulong";
        if (type == typeof(long)) return "long";
        if (type == typeof(string)) return "string";
        if (type == typeof(float)) return "float";
        if (type == typeof(double)) return "double";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(BigInteger)) return "bigint";
        if (type.IsEnum) return "enum";
        if (type == typeof(ScriptRange)) return "range";
        if (type == typeof(ScriptArray) || typeof(System.Collections.IList).IsAssignableFrom(type)) return "array";
        if (typeof(IScriptObject).IsAssignableFrom(type)) return "object";
        if (typeof(IScriptCustomFunction).IsAssignableFrom(type)) return "function";

        string name = type.Name;

        var indexOfGenerics = name.IndexOf('`');
        if (indexOfGenerics > 0)
        {
            name = name.Substring(0, indexOfGenerics);

            var builder = new StringBuilder();
            builder.Append(name);
            builder.Append('<');
            var genericArguments = type.GenericTypeArguments;
            for (var i = 0; i < genericArguments.Length; i++)
            {
                var argType = genericArguments[i];
                if (i > 0) builder.Append(", ");
                builder.Append(ScriptPrettyName(argType));
            }
            builder.Append('>');
            name = builder.ToString();
        }

        var typeNameAttr = type.GetCustomAttribute<ScriptTypeNameAttribute>();
        if (typeNameAttr is not null)
        {
            return typeNameAttr.TypeName;
        }

        // For any CodeBrix.Templating ScriptXxxYyy name, return xxx_yyy
        if (type.Namespace is not null && type.Namespace.StartsWith("CodeBrix.Templating."))
        {
            if (name.StartsWith("Script"))
            {
                name = name.Substring("Script".Length);
            }
            return StandardMemberRenamer.Rename(name);
        }

        return name;
    }
}
