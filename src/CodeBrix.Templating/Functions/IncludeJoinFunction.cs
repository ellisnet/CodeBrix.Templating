// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeBrix.Templating.Runtime;
using CodeBrix.Templating.Syntax;

namespace CodeBrix.Templating.Functions; //was previously: Scriban.Functions;

/// <summary>
/// The include join function available through the function 'include_join' in scriban.
/// </summary>
public
sealed partial class IncludeJoinFunction : IScriptCustomFunction
{
    /// <summary><c>IncludeJoinFunction</c>.</summary>
    public IncludeJoinFunction()
    {
    }
    /// <summary><c>Invoke</c>.</summary>
    public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
    {
        var resolvedCallerContext = callerContext ?? context.CurrentNode;
        if (resolvedCallerContext is null)
        {
            throw new ScriptRuntimeException(context.CurrentSpan, "Unable to resolve the include_join caller context.");
        }

        if (arguments.Count < 2)
        {
            throw new ScriptRuntimeException(resolvedCallerContext.Span, "Expecting at least the separator and components to include for the <include_join> function.");
        }

        var templateNames =
            (arguments[0] as ScriptArray)?.Select(x => context.ObjectToString(x)).Where(x => !string.IsNullOrEmpty(x)).Cast<string>().ToArray()
            ?? (arguments[0] as IEnumerable<string>)?.ToArray();

        if (templateNames is null)
        {
            return string.Empty;
        }

        var separator = RenderComponent(context, resolvedCallerContext, arguments, context.ObjectToString(arguments[1]) ?? string.Empty);
        var start = RenderComponent(context, resolvedCallerContext, arguments, arguments.Count > 2 ? context.ObjectToString(arguments[2]) ?? string.Empty : string.Empty);
        var end = RenderComponent(context, resolvedCallerContext, arguments, arguments.Count > 3 ? context.ObjectToString(arguments[3]) ?? string.Empty : string.Empty);

        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(start))
        {
            sb.Append(start);
        }
        for (int i = 0; i < templateNames.Length; ++i)
        {
            var templateName = templateNames[i];
            var templatePath = context.GetTemplatePathFromName(templateName, resolvedCallerContext);

            // liquid compatibility
            if (templatePath is null) continue;

            Template template = context.GetOrCreateTemplate(templatePath, resolvedCallerContext);

            sb.Append(context.RenderTemplate(template, arguments, resolvedCallerContext));

            if (!string.IsNullOrEmpty(separator) && i < templateNames.Length - 1)
            {
                sb.Append(separator);
            }
        }

        if (!string.IsNullOrEmpty(end))
        {
            sb.Append(end);
        }

        return sb.ToString();
    }
    /// <summary><c>RequiredParameterCount</c>.</summary>
    public int RequiredParameterCount => 2;
    /// <summary><c>ParameterCount</c>.</summary>
    public int ParameterCount => 4;
    /// <summary><c>VarParamKind</c>.</summary>
    public ScriptVarParamKind VarParamKind => ScriptVarParamKind.Direct;
    /// <summary><c>ReturnType</c>.</summary>
    public Type ReturnType => typeof(object);
    /// <summary><c>GetParameterInfo</c>.</summary>
    public ScriptParameterInfo GetParameterInfo(int index)
    {
        switch (index)
        {
            case 0:
                return new ScriptParameterInfo(typeof(IList), "template_names");
            case 1:
                return new ScriptParameterInfo(typeof(string), "separator");
            case 2:
                return new ScriptParameterInfo(typeof(string), "prefix");
            case 3:
                return new ScriptParameterInfo(typeof(string), "suffix");
            
        }
        throw new IndexOutOfRangeException();
    }

    private string RenderComponent(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, string component)
    {
        if (!component.StartsWith("tpl:"))
            return component;

        var path = context.GetTemplatePathFromName(component.Substring(4), callerContext);
        if (path is null)
        {
            return string.Empty;
        }
        var template = context.GetOrCreateTemplate(path, callerContext);
        return context.RenderTemplate(template, arguments, callerContext); 
    }
}
