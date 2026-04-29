using CodeBrix.Templating.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

﻿using CodeBrix.Templating.Runtime;
#pragma warning disable IDE0130

namespace CodeBrix.Templating.Functions; //was previously: Scriban.Functions;

/// <summary>
/// The include function available through the function 'include' in scriban.
/// </summary>
public
    sealed partial class IncludeFunction
{
    /// <summary><c>InvokeAsync</c>.</summary>
    public async ValueTask<object> InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
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

        Template template = await context.GetOrCreateTemplateAsync(templatePath, resolvedCallerContext).ConfigureAwait(false);

        return await context.RenderTemplateAsync(template, arguments, resolvedCallerContext).ConfigureAwait(false);
    }
}
/// <summary>
/// The include join function available through the function 'include_join' in scriban.
/// </summary>
public
    sealed partial class IncludeJoinFunction
{
    /// <summary><c>InvokeAsync</c>.</summary>
    public async ValueTask<object> InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
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

        var separator = await RenderComponentAsync(context, resolvedCallerContext, arguments, context.ObjectToString(arguments[1]) ?? string.Empty).ConfigureAwait(false);
        var start = await RenderComponentAsync(context, resolvedCallerContext, arguments, arguments.Count > 2 ? context.ObjectToString(arguments[2]) ?? string.Empty : string.Empty).ConfigureAwait(false);
        var end = await RenderComponentAsync(context, resolvedCallerContext, arguments, arguments.Count > 3 ? context.ObjectToString(arguments[3]) ?? string.Empty : string.Empty).ConfigureAwait(false);

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

            Template template = await context.GetOrCreateTemplateAsync(templatePath, resolvedCallerContext).ConfigureAwait(false);

            sb.Append(await context.RenderTemplateAsync(template, arguments, resolvedCallerContext).ConfigureAwait(false));

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

    private async ValueTask<string> RenderComponentAsync(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, string component)
    {
        if (!component.StartsWith("tpl:"))
            return component;

        var path = context.GetTemplatePathFromName(component.Substring(4), callerContext);
        if (path is null)
        {
            return string.Empty;
        }
        var template = await context.GetOrCreateTemplateAsync(path, callerContext).ConfigureAwait(false);
        return await context.RenderTemplateAsync(template, arguments, callerContext).ConfigureAwait(false);
    }
}
