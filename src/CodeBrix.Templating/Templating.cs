using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Runtime;
using CodeBrix.Templating.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CodeBrix.Templating; //was previously: Scriban;

/// <summary>
/// Basic entry point class to parse templates and render them. For more advanced scenario, you should use <see cref="TemplateContext"/> directly.
/// </summary>
public partial class Template
{

    /// <summary>
    /// Evaluates the template using the specified context. See remarks.
    /// </summary>
    /// <param name="context">The template context.</param>
    /// <param name="render"><c>true</c> to render the output to the <see cref="TemplateContext.Output"/></param>
    /// <exception cref="System.ArgumentNullException">If context is null</exception>
    /// <exception cref="System.InvalidOperationException">If the template <see cref="HasErrors"/>. Check the <see cref="Messages"/> property for more details</exception>
    private async ValueTask<object> EvaluateAndRenderAsync(TemplateContext context, bool render)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));
        CheckErrors();

        // Make sure that we are using the same parserOptions
        if (SourceFilePath is not null)
        {
            context.PushSourceFile(SourceFilePath);
        }

        try
        {
            context.UseScientific = LexerOptions.Lang == ScriptLang.Scientific;
            if (Page is null)
            {
                return null;
            }

            var result = await context.EvaluateAsync(Page).ConfigureAwait(false);
            if (render)
            {
                if (context.EnableOutput && result is not null)
                {
                    await context.WriteAsync(Page.Span, result).ConfigureAwait(false);
                }
            }
            return result;
        }
        finally
        {
            if (SourceFilePath is not null)
            {
                context.PopSourceFile();
            }
        }
    }

    /// <summary>
    /// Evaluates the template using the specified context. See remarks.
    /// </summary>
    /// <param name="context">The template context.</param>
    /// <exception cref="System.ArgumentNullException">If context is null</exception>
    /// <exception cref="System.InvalidOperationException">If the template <see cref="HasErrors"/>. Check the <see cref="Messages"/> property for more details</exception>
    /// <returns>Returns the result of the last statement</returns>
    public async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        var previousOutput = context.EnableOutput;
        try
        {
            context.UseScientific = LexerOptions.Lang == ScriptLang.Scientific;
            context.EnableOutput = false;
            return await EvaluateAndRenderAsync(context, false).ConfigureAwait(false);
        }
        finally
        {
            context.EnableOutput = previousOutput;
        }
    }

    /// <summary>
    /// Parse and evaluates a code only expression (without enclosing `{{` and `}}`) within the specified context.
    /// </summary>
    /// <param name="expression">A code only expression (without enclosing `{{` and `}}`)</param>
    /// <param name="context">The template context</param>
    /// <returns>The result of the evaluation of the expression</returns>
    public static async ValueTask<object> EvaluateAsync(string expression, TemplateContext context)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var lexerOption = new LexerOptions() { Mode = ScriptMode.ScriptOnly };
        var template = Parse(expression, lexerOptions: lexerOption);
        return await template.EvaluateAsync(context).ConfigureAwait(false);
    }

    /// <summary>
    /// Evaluates the template using the specified context
    /// </summary>
    /// <param name="model">An object model to use with the evaluation.</param>
    /// <param name="memberRenamer">The member renamer used to import this .NET object and transitive objects. See member renamer documentation for more details.</param>
    /// <param name="memberFilter">The member filter used to filter members for .NET objects being accessed through the template, including the model being passed to this method.</param>
    /// <exception cref="System.InvalidOperationException">If the template <see cref="HasErrors"/>. Check the <see cref="Messages"/> property for more details</exception>
    /// <returns>Returns the result of the last statement</returns>
    [RequiresUnreferencedCode("This overload imports the model object using reflection. Use Evaluate(TemplateContext) for AOT-safe evaluation.")]
    public async ValueTask<object> EvaluateAsync(object model = null, MemberRenamerDelegate memberRenamer = null, MemberFilterDelegate memberFilter = null)
    {
        var scriptObject = new ScriptObject();
        if (model is not null)
        {
            scriptObject.Import(model, renamer: memberRenamer, filter: memberFilter);
        }

        var context = LexerOptions.Lang == ScriptLang.Liquid ? new LiquidTemplateContext() : new TemplateContext();
        context.EnableOutput = false;
        context.MemberRenamer = memberRenamer ?? StandardMemberRenamer.Default;
        context.MemberFilter = memberFilter;
        context.UseScientific = LexerOptions.Lang == ScriptLang.Scientific;
        context.PushGlobal(scriptObject);
        var result = await EvaluateAsync(context).ConfigureAwait(false);
        context.PopGlobal();
        return result;
    }

    /// <summary>
    /// Parse and evaluates a code only expression (without enclosing `{{` and `}}`) within the specified context.
    /// </summary>
    /// <param name="expression">A code only expression (without enclosing `{{` and `}}`)</param>
    /// <param name="model">An object instance used as a model for evaluating this expression</param>
    /// <param name="memberRenamer">The member renamer used to import this .NET object and transitive objects. See member renamer documentation for more details.</param>
    /// <param name="memberFilter">The member filter used to filter members for .NET objects being accessed through the template, including the model being passed to this method.</param>
    /// <returns>The result of the evaluation of the expression</returns>
    [RequiresUnreferencedCode("This overload imports the model object using reflection. Use Evaluate(TemplateContext) for AOT-safe evaluation.")]
    public static async ValueTask<object> EvaluateAsync(string expression, object model, MemberRenamerDelegate memberRenamer = null, MemberFilterDelegate memberFilter = null)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        var lexerOption = new LexerOptions() { Mode = ScriptMode.ScriptOnly };
        var template = Parse(expression, lexerOptions: lexerOption);
        return await template.EvaluateAsync(model, memberRenamer, memberFilter).ConfigureAwait(false);
    }

    /// <summary>
    /// Renders this template using the specified context. See remarks.
    /// </summary>
    /// <param name="context">The template context.</param>
    /// <exception cref="System.ArgumentNullException">If context is null</exception>
    /// <exception cref="System.InvalidOperationException">If the template <see cref="HasErrors"/>. Check the <see cref="Messages"/> property for more details</exception>
    /// <remarks>
    /// When using this method, the result of rendering this page is output to <see cref="TemplateContext.Output"/>
    /// </remarks>
    public async ValueTask<string> RenderAsync(TemplateContext context)
    {
        await EvaluateAndRenderAsync(context, true).ConfigureAwait(false);
        var result = context.Output.ToString();
        var output = context.Output as StringBuilderOutput;
        if (output is not null)
        {
            output.Builder.Length = 0;
        }
        return result ?? string.Empty;
    }

    /// <summary>
    /// Renders this template using the specified object model.
    /// </summary>
    /// <param name="model">The object model.</param>
    /// <param name="memberRenamer">The member renamer used to import this .NET object and transitive objects. See member renamer documentation for more details.</param>
    /// <param name="memberFilter">The member filter used to filter members for .NET objects being accessed through the template, including the model being passed to this method.</param>
    /// <returns>A rendering result as a string </returns>
    [RequiresUnreferencedCode("This overload imports the model object using reflection. Use Render(TemplateContext) for AOT-safe rendering.")]
    public async ValueTask<string> RenderAsync(object model = null, MemberRenamerDelegate memberRenamer = null, MemberFilterDelegate memberFilter = null)
    {
        var scriptObject = new ScriptObject();
        if (model is not null)
        {
            scriptObject.Import(model, renamer: memberRenamer, filter: memberFilter);
        }

        var context = LexerOptions.Lang == ScriptLang.Liquid ? new LiquidTemplateContext() : new TemplateContext();
        context.MemberRenamer = memberRenamer ?? StandardMemberRenamer.Default;
        context.MemberFilter = memberFilter;
        context.PushGlobal(scriptObject);
        return await RenderAsync(context).ConfigureAwait(false);
    }
}
    
/// <summary>
/// The template context contains the state of the page, the model.
/// </summary>
public partial class TemplateContext
{
    /// <summary><c>CreateTemplateAsync</c>.</summary>
    protected virtual async ValueTask<Template> CreateTemplateAsync(string templatePath, ScriptNode callerContext)
    {
        var callerSpan = callerContext?.Span ?? CurrentSpan;
        var templateLoader = TemplateLoader ?? throw new ScriptRuntimeException(callerSpan, "No TemplateLoader registered in TemplateContext.TemplateLoader");
        string templateText;
        try
        {
            templateText = await templateLoader.LoadAsync(this, callerSpan, templatePath).ConfigureAwait(false);
        }
        catch (Exception ex) when (!(ex is ScriptRuntimeException))
        {
            throw new ScriptRuntimeException(callerSpan, $"Unexpected exception while creating template from path `{templatePath}`", ex);
        }

        if (templateText is null)
        {
            throw new ScriptRuntimeException(callerSpan, $"The result of including `{templatePath}` cannot be null");
        }

        var template = Template.Parse(templateText, templatePath, TemplateLoaderParserOptions, TemplateLoaderLexerOptions);

        // If the template has any errors, throw an exception
        if (template.HasErrors)
        {
            throw new ScriptParserRuntimeException(callerSpan, $"Error while parsing template `{templatePath}`", template.Messages);
        }

        CachedTemplates.Add(templatePath, template);

        return template;
    }

    /// <summary>
    /// Evaluates the specified script node.
    /// </summary>
    /// <param name="scriptNode">The script node.</param>
    /// <param name="aliasReturnedFunction">if set to <c>true</c> and a function would be evaluated as part of this node, return the object function without evaluating it.</param>
    /// <returns>The result of the evaluation.</returns>
    public virtual async ValueTask<object> EvaluateAsync(ScriptNode scriptNode, bool aliasReturnedFunction)
    {
        if (scriptNode is null) return null;

        var previousFunctionCallState = _isFunctionCallDisabled;
        var previousLevel = _getOrSetValueLevel;
        var previousNode = CurrentNode;
        try
        {
            CurrentNode = scriptNode;
            _getOrSetValueLevel = 0;
            _isFunctionCallDisabled = aliasReturnedFunction;
            return await scriptNode.EvaluateAsync(this).ConfigureAwait(false);
        }
        catch (ScriptRuntimeException ex) when (this.RenderRuntimeException is not null)
        {
            return this.RenderRuntimeException(ex);
        }
        catch (Exception ex) when (!(ex is ScriptRuntimeException))
        {
            var toThrow = new ScriptRuntimeException(scriptNode.Span, ex.Message, ex);
            if (RenderRuntimeException is not null)
            {
                return RenderRuntimeException(toThrow);
            }
            throw toThrow;
        }
        finally
        {
            CurrentNode = previousNode;
            _getOrSetValueLevel = previousLevel;
            _isFunctionCallDisabled = previousFunctionCallState;
        }
    }

    /// <summary>
    /// Evaluates the specified script node.
    /// </summary>
    /// <param name="scriptNode">The script node.</param>
    /// <returns>The result of the evaluation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask<object> EvaluateAsync(ScriptNode scriptNode)
    {
        return await EvaluateAsync(scriptNode, false).ConfigureAwait(false);
    }

    private async ValueTask<Dictionary<string, object>> FetchNamedArgumentsAsync(ScriptNode callerContext)
    {
        if (!(callerContext is ScriptFunctionCall functionCall) || functionCall.Arguments.Count == 0)
        {
            return null;
        }

        var namedArgumentsValues = new Dictionary<string, object>();
        foreach (var arg in functionCall.Arguments)
        {
            if (arg is ScriptNamedArgument namedArg)
            {
                var name = namedArg.Name?.Name;
                if (name is null || namedArg.Value is null)
                {
                    continue;
                }

                var value = await namedArg.Value.EvaluateAsync(this).ConfigureAwait(false);
                namedArgumentsValues[name] = value;
            }
        }
        return namedArgumentsValues;
    }
    /// <summary><c>GetOrCreateTemplateAsync</c>.</summary>
    public async ValueTask<Template> GetOrCreateTemplateAsync(string templatePath, ScriptNode callerContext)
    {
        if (!CachedTemplates.TryGetValue(templatePath, out Template template))
        {
            template = await CreateTemplateAsync(templatePath, callerContext).ConfigureAwait(false);
            CachedTemplates[templatePath] = template;
        }
        return template;
    }


    /// <summary>
    /// Evaluates the specified expression
    /// </summary>
    /// <param name="targetExpression">The expression to evaluate</param>
    /// <param name="valueToSet">A value to set in case of a setter</param>
    /// <param name="setter">true if this a setter</param>
    /// <returns>The value of the targetExpression</returns>
    private async ValueTask<object> GetOrSetValueAsync(ScriptExpression targetExpression, object valueToSet, bool setter)
    {
        object value = null;

        try
        {
            if (targetExpression is IScriptVariablePath nextPath)
            {
                if (setter)
                {
                    await nextPath.SetValueAsync(this, valueToSet).ConfigureAwait(false);
                }
                else
                {
                    value = await nextPath.GetValueAsync(this).ConfigureAwait(false);
                }
            }
            else if (!setter)
            {
                value = await EvaluateAsync(targetExpression).ConfigureAwait(false);
            }
            else
            {
                throw new ScriptRuntimeException(targetExpression.Span, $"Unsupported target expression for assignment."); // unit test: 105-assign-error1.txt
            }
        }
        catch (Exception readonlyException) when (_getOrSetValueLevel == 1 && !(readonlyException is ScriptRuntimeException))
        {
            throw new ScriptRuntimeException(targetExpression.Span, $"Unexpected exception while accessing target expression: {readonlyException.Message}", readonlyException);
        }

        value = await AwaitIfNeededAsync(value).ConfigureAwait(false);

        // If the variable being returned is a function, we need to evaluate it
        // If function call is disabled, it will be only when returning the final object (level 0 of recursion)
        var allowFunctionCall = (_isFunctionCallDisabled && _getOrSetValueLevel > 1) || !_isFunctionCallDisabled;
        if (allowFunctionCall && ScriptFunctionCall.IsFunction(value))
        {
            // Allow to pipe arguments only for top level returned function
            value = await ScriptFunctionCall.CallAsync(this, targetExpression, value, _getOrSetValueLevel == 1, null).ConfigureAwait(false);
        }

        return value;
    }

    /// <summary>
    /// Gets the value from the specified expression using the current <see cref="ScriptObject"/> bound to the model context.
    /// </summary>
    /// <param name="target">The expression</param>
    /// <returns>The value of the expression</returns>
    public async ValueTask<object> GetValueAsync(ScriptExpression target)
    {
        if (target is null)
        {
            return null;
        }

        var previousNode = CurrentNode;
        _getOrSetValueLevel++;
        try
        {
            CurrentNode = target;
            return await GetOrSetValueAsync(target, null, false).ConfigureAwait(false);
        }
        finally
        {
            CurrentNode = previousNode;
            _getOrSetValueLevel--;
        }
    }
    /// <summary><c>RenderTemplateAsync</c>.</summary>
    public async ValueTask<string> RenderTemplateAsync(Template template, ScriptArray arguments, ScriptNode callerContext)
    {
        // Make sure that we cannot recursively include a template
        var result = string.Empty;
        EnterRecursive(callerContext);
        var previousIndent = CurrentIndent;
        var previousTextWasNewLine = _previousTextWasNewLine;
        CurrentIndent = null;
        PushOutput();
        // Fetch any named argument values before pushing a new local scope, i.e. use the current context for evaluating the named arguments
        var namedArgumentsValues = await FetchNamedArgumentsAsync(callerContext).ConfigureAwait(false);
        // Start new local variables scope
        PushLocal();
        try
        {
            SetValue(ScriptVariable.Arguments, arguments, true, true);

            if (namedArgumentsValues is not null)
            {
                // Add local variables for each named argument
                foreach (var kv in namedArgumentsValues)
                {
                    var newLocalVariable = ScriptVariable.Create(kv.Key, ScriptVariableScope.Local);
                    SetValue(variable: newLocalVariable, value: kv.Value, asReadOnly: false, force: true);
                }
            }
            if (previousIndent is not null)
            {
                // Isolate the included template output from the caller newline state.
                ResetPreviousNewLine();
            }
            result = await template.RenderAsync(this).ConfigureAwait(false);
        }
        finally
        {
            PopLocal();
            PopOutput();
            CurrentIndent = previousIndent;
            _previousTextWasNewLine = previousTextWasNewLine;
            ExitRecursive(callerContext);
        }
        return result;
    }

    /// <summary>
    /// Sets the target expression with the specified value.
    /// </summary>
    /// <param name="target">The target expression.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="System.ArgumentNullException">If target is null</exception>
    public async ValueTask SetValueAsync(ScriptExpression target, object value)
    {
        if (target is null) return;
        _getOrSetValueLevel++;
        try
        {
            await GetOrSetValueAsync(target, value, true).ConfigureAwait(false);
        }
        finally
        {
            _getOrSetValueLevel--;
        }
    }

    /// <summary>
    /// Writes the text to the current <see cref="Output"/>
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="startIndex">The zero-based position of the substring of text</param>
    /// <param name="count">The number of characters to output starting at <paramref name="startIndex"/> position from the text</param>
    public async ValueTask<TemplateContext> WriteAsync(string text, int startIndex, int count)
    {
        if (text is not null)
        {
            // Indented text
            if (CurrentIndent is not null)
            {
                var index = startIndex;
                var indexEnd = startIndex + count;

                while (index < indexEnd)
                {
                    var newLineIndex = text.IndexOf('\n', index);
                    if (newLineIndex < 0 || newLineIndex >= indexEnd)
                    {
                        // Write indents if necessary
                        if (_previousTextWasNewLine)
                        {
                            if (!await WriteOutputChunkAsync(CurrentIndent, 0, CurrentIndent.Length).ConfigureAwait(false))
                            {
                                return this;
                            }
                            _previousTextWasNewLine = false;
                        }
                        if (!await WriteOutputChunkAsync(text, index, indexEnd - index).ConfigureAwait(false))
                        {
                            return this;
                        }
                        break;
                    }

                    var length = newLineIndex - index;
                    // Write indents if necessary
                    if (_previousTextWasNewLine && (IndentOnEmptyLines || length != 0 && (length != 1 || text[index] != '\r')))
                    {
                        if (!await WriteOutputChunkAsync(CurrentIndent, 0, CurrentIndent.Length).ConfigureAwait(false))
                        {
                            return this;
                        }
                        _previousTextWasNewLine = false;
                    }

                    // We output the new line
                    if (!await WriteOutputChunkAsync(text, index, length + 1).ConfigureAwait(false))
                    {
                        return this;
                    }
                    index = newLineIndex + 1;
                    _previousTextWasNewLine = true;
                }
            }
            else
            {
                if (count > 0)
                {
                    _previousTextWasNewLine = text[startIndex + count - 1] == '\n';
                }
                await WriteOutputChunkAsync(text, startIndex, count).ConfigureAwait(false);
            }
        }

        return this;
    }

    /// <summary>
    /// Writes the text to the current <see cref="Output"/>
    /// </summary>
    /// <param name="text">The text.</param>
    public async ValueTask<TemplateContext> WriteAsync(string text)
    {
        if (text is not null)
        {
            await WriteAsync(text, 0, text.Length).ConfigureAwait(false);
        }
        return this;
    }

    /// <summary>
    /// Writes the text to the current <see cref="Output"/>
    /// </summary>
    /// <param name="slice">The text.</param>
    public async ValueTask<TemplateContext> WriteAsync(ScriptStringSlice slice)
    {
        await WriteAsync(slice.FullText ?? string.Empty, slice.Index, slice.Length).ConfigureAwait(false);
        return this;
    }

    /// <summary>
    /// Writes an object value to the current <see cref="Output"/>.
    /// </summary>
    /// <param name="span">The span of the object to render.</param>
    /// <param name="textAsObject">The text as object.</param>
    public virtual async ValueTask<TemplateContext> WriteAsync(SourceSpan span, object textAsObject)
    {
        if (textAsObject is not null)
        {
            textAsObject = await AwaitIfNeededAsync(textAsObject).ConfigureAwait(false);
            var text = ObjectToString(textAsObject);
            if (text is not null)
            {
                await WriteAsync(text).ConfigureAwait(false);
            }
        }
        return this;
    }

    /// <summary>
    /// Writes the a new line to the current <see cref="Output"/>
    /// </summary>
    public async ValueTask<TemplateContext> WriteLineAsync()
    {
        await WriteAsync(NewLine).ConfigureAwait(false);
        return this;
    }

    private async ValueTask<bool> WriteOutputChunkAsync(string text, int startIndex, int count)
    {
        if (count <= 0)
        {
            return true;
        }

        var allowedCount = GetAllowedOutputCount(count);
        if (allowedCount > 0)
        {
            await Output.WriteAsync(text, startIndex, allowedCount, CancellationToken).ConfigureAwait(false);
            _currentOutputLength += allowedCount;
        }

        if (allowedCount < count)
        {
            await WriteOutputLimitEllipsisAsync().ConfigureAwait(false);
            return false;
        }

        return true;
    }

    private async ValueTask WriteOutputLimitEllipsisAsync()
    {
        if (_hasOutputLimitEllipsis)
        {
            return;
        }

        await Output.WriteAsync("...", 0, 3, CancellationToken).ConfigureAwait(false);
        _hasOutputLimitEllipsis = true;
    }
}
