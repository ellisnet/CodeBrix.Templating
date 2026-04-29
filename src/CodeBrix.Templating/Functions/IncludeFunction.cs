// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Runtime;
using CodeBrix.Templating.Syntax;

namespace CodeBrix.Templating.Functions; //was previously: Scriban.Functions;

/// <summary>
/// The include function available through the function 'include' in scriban.
/// </summary>
public
sealed partial class IncludeFunction : IScriptCustomFunction
{
    /// <summary><c>IncludeFunction</c>.</summary>
    public IncludeFunction()
    {
    }
    /// <summary><c>Invoke</c>.</summary>
    public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
    {
        var callerSpan = callerContext?.Span ?? context.CurrentSpan;
        var resolvedCallerContext = callerContext ?? context.CurrentNode;
        if (arguments.Count == 0)
        {
            throw new ScriptRuntimeException(callerSpan, "Expecting at least the name of the template to include for the <include> function");
        }

        var templateName = context.ObjectToString(arguments[0]);
        if (resolvedCallerContext is null)
        {
            throw new ScriptRuntimeException(callerSpan, "Unable to resolve the include caller context.");
        }

        var templatePath = context.GetTemplatePathFromName(templateName, resolvedCallerContext);
        // liquid compatibility
        if (templatePath is null) return null;

        Template template = context.GetOrCreateTemplate(templatePath, resolvedCallerContext);

        return context.RenderTemplate(template, arguments, resolvedCallerContext);
    }
    /// <summary><c>RequiredParameterCount</c>.</summary>
    public int RequiredParameterCount => 1;
    /// <summary><c>ParameterCount</c>.</summary>
    public int ParameterCount => 1;
    /// <summary><c>VarParamKind</c>.</summary>
    public ScriptVarParamKind VarParamKind => ScriptVarParamKind.Direct;
    /// <summary><c>ReturnType</c>.</summary>
    public Type ReturnType => typeof(object);
    /// <summary><c>GetParameterInfo</c>.</summary>
    public ScriptParameterInfo GetParameterInfo(int index)
    {
        if (index == 0) return new ScriptParameterInfo(typeof(string), "template_name");
        return new ScriptParameterInfo(typeof(object), "value");
    }
    /// <summary><c>GetParameterIndexByName</c>.</summary>
    public int GetParameterIndexByName(string name)
    {
        throw new NotImplementedException();
    }
}
