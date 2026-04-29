using System;
using System.Threading;
using System.Threading.Tasks;

﻿using CodeBrix.Templating.Syntax;
#pragma warning disable IDE0130

namespace CodeBrix.Templating.Runtime; //was previously: Scriban.Runtime;

public
    abstract partial class DynamicCustomFunction
{
    /// <summary><c>GetValueFromNamedArgumentAsync</c>.</summary>
    protected async ValueTask<ArgumentValue> GetValueFromNamedArgumentAsync(TemplateContext context, ScriptNode callerContext, ScriptNamedArgument namedArg)
    {
        for (int j = 0; j < Parameters.Length; j++)
        {
            var arg = Parameters[j];
            if (arg.Name == namedArg.Name?.Name)
            {
                return new ArgumentValue(j, arg.ParameterType, await context.EvaluateAsync(namedArg).ConfigureAwait(false));
            }
        }
        throw new ScriptRuntimeException(callerContext.Span, $"Invalid argument `{namedArg.Name}` not found for function `{callerContext}`");
    }

}

/// <summary>
/// Extensions for <see cref="IScriptOutput"/>
/// </summary>
public
    static partial class ScriptOutputExtensions
{
    /// <summary><c>WriteAsync</c>.</summary>
    public static async ValueTask WriteAsync(this IScriptOutput scriptOutput, string text, CancellationToken cancellationToken)
    {
        if (text is null) throw new ArgumentNullException(nameof(text));
        await scriptOutput.WriteAsync(text, 0, text.Length, cancellationToken).ConfigureAwait(false);
    }
    /// <summary><c>WriteAsync</c>.</summary>
    public static async ValueTask WriteAsync(this IScriptOutput scriptOutput, ScriptStringSlice text, CancellationToken cancellationToken)
    {
        if (text.FullText is null) throw new ArgumentNullException(nameof(text));
        if (text.Length == 0) return;
        await scriptOutput.WriteAsync(text.FullText, text.Index, text.Length, cancellationToken).ConfigureAwait(false);
    }
}
