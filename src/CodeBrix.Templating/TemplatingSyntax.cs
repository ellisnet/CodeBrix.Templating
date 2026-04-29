using CodeBrix.Templating.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

﻿using CodeBrix.Templating.Parsing;
#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;

/// <summary>
/// Used to rewrite a function call expression at evaluation time based
/// on the arguments required by a function. Used by scientific mode scripting.
/// </summary>
internal partial class ScientificFunctionCallRewriter
{
    private static async ValueTask FlattenBinaryExpressionsAsync(TemplateContext context,
        ScriptExpression expression, List<BinaryExpressionOrOperator> expressions)
    {
        while (true)
        {
            if (!(expression is ScriptBinaryExpression binaryExpression))
            {
                expressions.Add(new BinaryExpressionOrOperator(expression,
                    await GetFunctionCallKindAsync(context, expression).ConfigureAwait(false)));
                return;
            }

            if (binaryExpression.Left is null || binaryExpression.Right is null)
            {
                throw new ScriptRuntimeException(binaryExpression.Span,
                    "Invalid binary expression while rewriting a scientific expression.");
            }

            var left = (ScriptExpression)binaryExpression.Left.Clone();
            var right = (ScriptExpression)binaryExpression.Right.Clone();
            var token = binaryExpression.OperatorToken is not null
                ? (ScriptToken)binaryExpression.OperatorToken.Clone()
                : null;
            await FlattenBinaryExpressionsAsync(context, left, expressions).ConfigureAwait(false);
            expressions.Add(new BinaryExpressionOrOperator(binaryExpression.Operator, token));
            // Iterate on right (equivalent to tail recursive call)
            expression = right;
        }
    }

    private static async ValueTask<FunctionCallKind> GetFunctionCallKindAsync(TemplateContext context,
        ScriptExpression expression)
    {
        var restoreStrictVariables = context.StrictVariables;
        // Don't fail on trying to lookup for a variable
        context.StrictVariables = false;
        object result = null;
        try
        {
            result = await context.EvaluateAsync(expression, true).ConfigureAwait(false);
        }
        catch (ScriptRuntimeException) when (context.IgnoreExceptionsWhileRewritingScientific)
        {
            // ignore any exceptions during trial evaluating as we could try to evaluate
            // variable that aren't setup
        }
        finally
        {
            context.StrictVariables = restoreStrictVariables;
        }

        // If one argument is a function, the remaining arguments
        if (result is IScriptCustomFunction function)
        {
            var maxArg = function.RequiredParameterCount != 0 ? function.RequiredParameterCount :
                function.ParameterCount > 0 ? 1 : 0;
            // We match all functions with at least one argument.
            // If we are expecting more than one argument, let the error happen later with the function call.
            if (maxArg > 0)
            {
                var isExpectingExpression = function.IsParameterType<ScriptExpression>(0);
                return isExpectingExpression ? FunctionCallKind.Expression : FunctionCallKind.Regular;
            }
        }

        return FunctionCallKind.None;
    }

    public static async ValueTask<ScriptExpression> RewriteAsync(TemplateContext context,
        ScriptBinaryExpression binaryExpression)
    {
        Debug.Assert(ImplicitFunctionCallPrecedence < Parser.PrecedenceOfMultiply);
        if (!HasImplicitBinaryExpression(binaryExpression))
        {
            return binaryExpression;
        }

        // TODO: use a TLS cache
        var iterator = new BinaryExpressionIterator();
        // a b c / d + e
        // stack will contain:
        // [0] a
        // [1] implicit *
        // [2] b
        // [3] implicit *
        // [4] c
        // [5] /
        // [6] d
        // [7] +
        // [8] e
        await FlattenBinaryExpressionsAsync(context, binaryExpression, iterator).ConfigureAwait(false);
        return ParseBinaryExpressionTree(iterator, 0, false);
    }
}

public
    partial class ScriptArrayInitializerExpression
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        var scriptArray = new ScriptArray();
        foreach (var value in Values)
        {
            var valueEval = await context.EvaluateAsync(value).ConfigureAwait(false);
            scriptArray.Add(valueEval);
        }

        return scriptArray;
    }
}

public
    partial class ScriptAssignExpression
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        if (Target is null || Value is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid assignment expression. Target and value are required.");
        }

        var valueObject = EqualToken.TokenType == TokenType.Equal
            ? await context.EvaluateAsync(Value).ConfigureAwait(false)
            : await GetValueToSetAsync(context).ConfigureAwait(false);
        await context.SetValueAsync(Target, valueObject).ConfigureAwait(false);
        return null;
    }

    private async ValueTask<object> GetValueToSetAsync(TemplateContext context)
    {
        if (Target is null || Value is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid assignment expression. Target and value are required.");
        }

        var right = await context.EvaluateAsync(Value).ConfigureAwait(false);
        var left = await context.EvaluateAsync(Target).ConfigureAwait(false);
        var op = this.EqualToken.TokenType switch
        {
            TokenType.PlusEqual => ScriptBinaryOperator.Add,
            TokenType.MinusEqual => ScriptBinaryOperator.Subtract,
            TokenType.AsteriskEqual => ScriptBinaryOperator.Multiply,
            TokenType.DivideEqual => ScriptBinaryOperator.Divide,
            TokenType.DoubleDivideEqual => ScriptBinaryOperator.DivideRound,
            TokenType.PercentEqual => ScriptBinaryOperator.Modulus,
            _ => throw new ScriptRuntimeException(context.CurrentSpan,
                $"Operator {this.EqualToken} is not a valid compound assignment operator"),
        };
        return ScriptBinaryExpression.Evaluate(context, this.Span, op, left, right);
    }
}

public
    partial class ScriptBinaryExpression
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        var leftExpression = Left;
        var rightExpression = Right;
        if (leftExpression is null || rightExpression is null)
        {
            throw new ScriptRuntimeException(Span,
                "Invalid binary expression. Left and right expressions are required.");
        }

        // If we are in scientific mode and we have a function which takes arguments, and is not an explicit call (e.g sin(x) rather then sin * x)
        // Then we need to rewrite the call to a proper expression.
        if (context.UseScientific)
        {
            var newExpression =
                await ScientificFunctionCallRewriter.RewriteAsync(context, this).ConfigureAwait(false);
            if (!ReferenceEquals(newExpression, this))
            {
                return await context.EvaluateAsync(newExpression).ConfigureAwait(false);
            }
        }

        var leftValue = await context.EvaluateAsync(leftExpression).ConfigureAwait(false);
        switch (Operator)
        {
            case ScriptBinaryOperator.And:
            {
                var leftBoolValue = context.ToBool(leftExpression.Span, leftValue);
                if (!leftBoolValue)
                    return false;
                var rightValue = await context.EvaluateAsync(rightExpression).ConfigureAwait(false);
                var rightBoolValue = context.ToBool(rightExpression.Span, rightValue);
                return leftBoolValue && rightBoolValue;
            }

            case ScriptBinaryOperator.Or:
            {
                var leftBoolValue = context.ToBool(leftExpression.Span, leftValue);
                if (leftBoolValue)
                    return true;
                var rightValue = await context.EvaluateAsync(rightExpression).ConfigureAwait(false);
                return context.ToBool(rightExpression.Span, rightValue);
            }

            default:
            {
                var rightValue = await context.EvaluateAsync(rightExpression).ConfigureAwait(false);
                return Evaluate(context, OperatorToken?.Span ?? Span, Operator, leftExpression.Span, leftValue,
                    rightExpression.Span, rightValue);
            }
        }
    }
}

public
    partial class ScriptBlockStatement
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        var autoIndent = context.AutoIndent;
        object result = null;
        var statements = Statements;
        string previousIndent = context.CurrentIndent;
        string currentIndent = previousIndent;
        try
        {
            for (int i = 0; i < statements.Count; i++)
            {
                var statement = statements[i];
                // Throws a cancellation
                context.CheckAbort();
                if (autoIndent && statement is ScriptEscapeStatement escape)
                {
                    if (escape.IsEntering)
                    {
                        currentIndent = escape.Indent;
                    }
                    else if (escape.IsClosing)
                    {
                        currentIndent = previousIndent;
                    }
                }

                context.CurrentIndent = currentIndent;
                if (statement.CanSkipEvaluation)
                {
                    continue;
                }

                result = await context.EvaluateAsync(statement).ConfigureAwait(false);
                // Top-level assignment expression don't output anything
                if (!statement.CanOutput)
                {
                    result = null;
                }
                else if (result is not null && context.FlowState != ScriptFlowState.Return && context.EnableOutput)
                {
                    await context.WriteAsync(Span, result).ConfigureAwait(false);
                    result = null;
                }

                // If flow state is different, we need to exit this loop
                if (context.FlowState != ScriptFlowState.None)
                {
                    break;
                }
            }
        }
        finally
        {
            context.CurrentIndent = previousIndent;
        }

        return result;
    }
}

public
    partial class ScriptCaptureStatement
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        var body = Body;
        var target = Target;
        if (body is null || target is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid capture statement. Target and body are required.");
        }

        // unit test: 230-capture-statement.txt
        context.PushOutput();
        try
        {
            await context.EvaluateAsync(body).ConfigureAwait(false);
        }
        finally
        {
            var result = context.PopOutput();
            await context.SetValueAsync(target, result).ConfigureAwait(false);
        }

        return null;
    }
}

public
    partial class ScriptCaseStatement
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        if (Value is null || Body is null)
        {
            return null;
        }

        var caseValue = await context.EvaluateAsync(Value).ConfigureAwait(false);
        context.PushCase(caseValue);
        try
        {
            return await context.EvaluateAsync(Body).ConfigureAwait(false);
        }
        finally
        {
            context.PopCase();
        }
    }
}

public
    partial class ScriptConditionalExpression
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        if (Condition is null || ThenValue is null || ElseValue is null)
        {
            return null;
        }

        var condValue = await context.EvaluateAsync(Condition).ConfigureAwait(false);
        var result = context.ToBool(Condition.Span, condValue);
        return await context.EvaluateAsync(result ? ThenValue : ElseValue).ConfigureAwait(false);
    }
}

public
    partial class ScriptElseStatement
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        return Body is null ? null : await context.EvaluateAsync(Body).ConfigureAwait(false);
    }
}

public
    partial class ScriptExpressionStatement
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        if (Expression is null)
        {
            return null;
        }

        var result = await context.EvaluateAsync(Expression).ConfigureAwait(false);
        // This code is necessary for wrap to work
        var codeDelegate = result as ScriptNode;
        if (codeDelegate is not null)
        {
            return await context.EvaluateAsync(codeDelegate).ConfigureAwait(false);
        }

        return result;
    }
}

public
    partial class ScriptForStatement
{
    /// <summary><c>EvaluateImplAsync</c>.</summary>
    protected override async ValueTask<object> EvaluateImplAsync(TemplateContext context)
    {
        var iterator = Iterator;
        var variable = Variable;
        if (iterator is null || variable is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid for statement. Variable and iterator are required.");
        }

        var loopIterator = await context.EvaluateAsync(iterator).ConfigureAwait(false);
        var list = loopIterator as IEnumerable;
        if (list is not null)
        {
            object loopResult = null;
            object previousValue = null;
            var loopType = loopIterator is System.Linq.IQueryable
                ? TemplateContext.LoopType.Queryable
                : TemplateContext.LoopType.Default;
            //int startIndex = 0;
            int limit = -1;
            int continueIndex = 0;
            if (NamedArguments is not null)
            {
                bool reversed = false;
                int offset = 0;
                var listTyped = System.Linq.Enumerable.Cast<object>(list);
                foreach (var option in NamedArguments)
                {
                    var optionName = option.Name?.Name;
                    switch (optionName)
                    {
                        case "offset":
                            if (option.Value is null)
                                throw new ScriptRuntimeException(option.Span,
                                    "Invalid `offset` argument. Value is required.");
                            offset = context.ToInt(option.Value.Span,
                                await context.EvaluateAsync(option.Value).ConfigureAwait(false));
                            continueIndex = offset;
                            break;
                        case "reversed":
                            reversed = true;
                            break;
                        case "limit":
                            if (option.Value is null)
                                throw new ScriptRuntimeException(option.Span,
                                    "Invalid `limit` argument. Value is required.");
                            limit = context.ToInt(option.Value.Span,
                                await context.EvaluateAsync(option.Value).ConfigureAwait(false));
                            break;
                        default:
                            await ProcessArgumentAsync(context, option).ConfigureAwait(false);
                            break;
                    }
                }

                if (offset > 0)
                {
                    listTyped = System.Linq.Enumerable.Skip(listTyped, offset);
                }

                if (reversed)
                {
                    listTyped = System.Linq.Enumerable.Reverse(listTyped);
                }

                if (limit > 0)
                {
                    listTyped = System.Linq.Enumerable.Take(listTyped, limit);
                }

                list = listTyped;
            }

            bool isFirst = true;
            int index = 0;
            await BeforeLoopAsync(context).ConfigureAwait(false);
            var loopState = CreateLoopState();
            context.SetLoopVariable(GetLoopVariable(context), loopState);
            var it = list.GetEnumerator();
            loopState.SetEnumerable(list, it);
            bool enteredLoop = false;
            if (it.MoveNext())
            {
                enteredLoop = true;
                while (true)
                {
                    if (!context.StepLoop(this, loopType))
                    {
                        return null;
                    }

                    loopState.ResetLast();
                    // We update on next run on previous value (in order to handle last)
                    var value = it.Current;
                    loopState.Index = index;
                    loopState.ValueChanged = isFirst || !Equals(previousValue, value);
                    if (variable is ScriptVariable loopVariable)
                    {
                        context.SetLoopVariable(loopVariable, value);
                    }
                    else
                    {
                        await context.SetValueAsync(variable, value).ConfigureAwait(false);
                    }

                    loopResult = await LoopItemAsync(context, loopState).ConfigureAwait(false);
                    var isLast = loopState.MoveNextAndIsLast();
                    if (!ContinueLoop(context) || isLast)
                    {
                        break;
                    }

                    previousValue = value;
                    isFirst = false;
                    index++;
                    continueIndex++;
                }
            }

            await AfterLoopAsync(context).ConfigureAwait(false);
            if (SetContinue)
            {
                context.SetValue(ScriptVariable.Continue, continueIndex + 1);
            }

            if (!enteredLoop && Else is not null)
            {
                loopResult = await context.EvaluateAsync(Else).ConfigureAwait(false);
            }

            return loopResult;
        }

        if (loopIterator is not null)
        {
            throw new ScriptRuntimeException(iterator.Span,
                $"Unexpected type `{loopIterator.GetType()}` for iterator");
        }

        return null;
    }
    /// <summary><c>LoopItemAsync</c>.</summary>
    protected override async ValueTask<object> LoopItemAsync(TemplateContext context, LoopState state)
    {
        if (Body is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid for statement. Body is required.");
        }

        return await context.EvaluateAsync(Body).ConfigureAwait(false);
    }
}

public
    partial class ScriptFrontMatter
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        return await context.EvaluateAsync(Statements).ConfigureAwait(false);
    }
}

public
    partial class ScriptFunction
{
    /// <summary><c>InvokeAsync</c>.</summary>
    public async ValueTask<object> InvokeAsync(TemplateContext context, ScriptNode callerContext,
        ScriptArray arguments, ScriptBlockStatement blockStatement)
    {
        bool hasParams = HasParameters;
        if (hasParams)
        {
            context.PushGlobal(new ScriptObject());
        }
        else
        {
            context.PushLocal();
        }

        try
        {
            if (NameOrDoToken is ScriptVariableLocal localVariable)
            {
                context.SetValue(localVariable, this);
            }

            context.SetValue(ScriptVariable.Arguments, arguments, true);
            if (hasParams)
            {
                var glob = context.CurrentGlobal;
                if (glob is null)
                {
                    throw new ScriptRuntimeException(Span, "Missing global scope for function invocation.");
                }

                var parameters = Parameters;
                if (parameters is null)
                {
                    throw new ScriptRuntimeException(Span, "Missing function parameters.");
                }

                for (var i = 0; i < parameters.Count; i++)
                {
                    var param = parameters[i];
                    var parameterName = param.Name?.Name;
                    if (parameterName is null)
                    {
                        throw new ScriptRuntimeException(param.Span, "Missing function parameter name.");
                    }

                    glob.SetValue(parameterName, arguments[i], false);
                }
            }

            // Set the block delegate
            if (blockStatement is not null)
            {
                context.SetValue(ScriptVariable.BlockDelegate, blockStatement, true);
            }

            var result = Body is null ? null : await context.EvaluateAsync(Body).ConfigureAwait(false);
            return result;
        }
        finally
        {
            if (hasParams)
            {
                context.PopGlobal();
            }
            else
            {
                context.PopLocal();
            }
        }
    }
}

public
    partial class ScriptFunctionCall
{
    /// <summary><c>CallAsync</c>.</summary>
    public static async ValueTask<object> CallAsync(TemplateContext context, ScriptNode callerContext,
        object functionObject, bool processPipeArguments, IReadOnlyList<ScriptExpression> arguments)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        // Pop immediately the block
        ScriptBlockStatement blockDelegate = null;
        if (context.BlockDelegates.Count > 0)
        {
            blockDelegate = context.BlockDelegates.Pop();
        }

        var callerSpan = callerContext?.Span ?? context.CurrentSpan;
        if (functionObject is null)
        {
            throw new ScriptRuntimeException(callerSpan, $"The target function `{callerContext}` is null");
        }

        var scriptFunction = functionObject as ScriptFunction;
        var function = functionObject as IScriptCustomFunction;
        var isPipeCall = processPipeArguments && context.CurrentPipeArguments is not null &&
                         context.CurrentPipeArguments.Count > 0;
        if (function is null)
        {
            if ((isPipeCall) && (callerContext is ScriptFunctionCall funcCall))
            {
                throw new ScriptRuntimeException(callerSpan,
                    $"Pipe expression destination `{funcCall.Target}` is not a valid function ");
            }
            else
            {
                throw new ScriptRuntimeException(callerSpan,
                    $"Invalid target function `{functionObject}` ({context.GetTypeName(functionObject)})");
            }
        }

        if (function.ParameterCount >= MaximumParameterCount)
        {
            throw new ScriptRuntimeException(callerSpan,
                $"Out of range number of parameters {function.ParameterCount} for target function `{functionObject}`. The maximum number of parameters for a function is: {MaximumParameterCount}.");
        }

        // Generates an error only if the context is configured for it
        if (context.ErrorForStatementFunctionAsExpression && function.ReturnType == typeof(void) &&
            callerContext?.Parent is ScriptExpression)
        {
            var firstToken = callerContext.FindFirstTerminal();
            throw new ScriptRuntimeException(callerContext.Span,
                $"The function `{firstToken}` is a statement and cannot be used within an expression.");
        }

        // We can't cache this array because it might be collect by the function
        // So we absolutely need to generate a new array everytime we call a function
        ScriptArray argumentValues;
        List<ScriptExpression> allArgumentsWithPipe = null;
        // Handle pipe arguments here
        if (isPipeCall)
        {
            var argCount = Math.Max(function.RequiredParameterCount, 1 + (arguments?.Count ?? 0));
            allArgumentsWithPipe = context.GetOrCreateListOfScriptExpressions(argCount);
            var currentPipeArguments = context.CurrentPipeArguments;
            if (currentPipeArguments is null)
            {
                throw new ScriptRuntimeException(callerSpan, "Invalid pipe state. Missing pipe arguments.");
            }

            var pipeFrom = currentPipeArguments.Pop();
            argumentValues = new ScriptArray(argCount);
            allArgumentsWithPipe.Add(pipeFrom);
            if (arguments is not null)
            {
                allArgumentsWithPipe.AddRange(arguments);
            }

            arguments = allArgumentsWithPipe;
        }
        else
        {
            argumentValues = new ScriptArray(arguments?.Count ?? 0);
        }

        object result = null;
        try
        {
            // Process direct arguments
            ulong argMask = 0;
            if (arguments is not null)
            {
                argMask = await ProcessArgumentsAsync(context, callerContext, arguments, function, scriptFunction,
                    argumentValues).ConfigureAwait(false);
            }

            // Fill remaining argument default values
            var hasVariableParams = function.VarParamKind != ScriptVarParamKind.None;
            var requiredParameterCount = function.RequiredParameterCount;
            var parameterCount = function.ParameterCount;
            if (function.VarParamKind != ScriptVarParamKind.Direct)
            {
                FillRemainingOptionalArguments(ref argMask, argumentValues.Count, parameterCount - 1, function,
                    argumentValues);
            }

            // Check the required number of arguments
            var requiredMask = (1U << requiredParameterCount) - 1;
            argMask = argMask & requiredMask;
            // Create a span after the caller for missing arguments
            var afterCallerSpan = callerSpan;
            afterCallerSpan.Start = afterCallerSpan.End.NextColumn();
            afterCallerSpan.End = afterCallerSpan.End.NextColumn();
            if (argMask != requiredMask)
            {
                int argCount = 0;
                while (argMask != 0)
                {
                    if ((argMask & 1) != 0)
                        argCount++;
                    argMask = argMask >> 1;
                }

                throw new ScriptRuntimeException(afterCallerSpan,
                    $"Invalid number of arguments `{argCount}` passed to `{callerContext}` while expecting `{requiredParameterCount}` arguments");
            }

            if (!hasVariableParams && argumentValues.Count > parameterCount)
            {
                if (argumentValues.Count > 0 && arguments is not null && argumentValues.Count <= arguments.Count)
                {
                    throw new ScriptRuntimeException(arguments[argumentValues.Count - 1].Span,
                        $"Invalid number of arguments `{argumentValues.Count}` passed to `{callerContext}` while expecting `{parameterCount}` arguments");
                }

                throw new ScriptRuntimeException(afterCallerSpan,
                    $"Invalid number of arguments `{argumentValues.Count}` passed to `{callerContext}` while expecting `{parameterCount}` arguments");
            }

            if (callerContext is not null)
            {
                context.EnterFunction(callerContext);
            }

            try
            {
                result = await function.InvokeAsync(context, callerContext, argumentValues, blockDelegate)
                    .ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                // Slow path to detect the argument index from the name if we can
                var index = GetParameterIndexByName(function, ex.ParamName);
                if (index >= 0 && arguments is not null && index < arguments.Count)
                {
                    throw new ScriptRuntimeException(arguments[index].Span, ex.Message);
                }

                throw;
            }
            catch (ScriptArgumentException ex)
            {
                var index = ex.ArgumentIndex;
                if (index >= 0 && arguments is not null && index < arguments.Count)
                {
                    throw new ScriptRuntimeException(arguments[index].Span, ex.Message);
                }

                throw;
            }
            finally
            {
                if (callerContext is not null)
                {
                    context.ExitFunction(callerContext);
                }
            }
        }
        finally
        {
            if (allArgumentsWithPipe is not null)
            {
                context.ReleaseListOfScriptExpressions(allArgumentsWithPipe);
            }
        }

        // Restore the flow state to none
        context.FlowState = ScriptFlowState.None;
        return result;
    }

    /// <summary>
    /// Call a custom function with the already resolved parameters.
    /// </summary>
    /// <param name = "context"></param>
    /// <param name = "callerContext"></param>
    /// <param name = "function"></param>
    /// <param name = "arguments"></param>
    /// <returns></returns>
    public static async ValueTask<object> CallAsync(TemplateContext context, ScriptNode callerContext,
        IScriptCustomFunction function, ScriptArray arguments)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (function is null)
            throw new ArgumentNullException(nameof(function));
        if (arguments is null)
            throw new ArgumentNullException(nameof(arguments));
        var parameterCount = function.ParameterCount;
        var argumentValues = new ScriptArray();
        var span = callerContext?.Span ?? context.CurrentSpan;
        // Fast path if we don't have complicated parameters to handle (direct call, same amount of arguments than expected parameters)
        if (function.VarParamKind == ScriptVarParamKind.None && parameterCount == arguments.Count)
        {
            for (int i = 0; i < parameterCount; i++)
            {
                var arg = arguments[i];
                var paramType = function.GetParameterInfo(i).ParameterType;
                var value = context.ToObject(span, arg, paramType);
                argumentValues.Add(value);
            }
        }
        else
        {
            // Otherwise we need to do a slow path
            ulong argMask = 0;
            foreach (var arg in arguments)
            {
                int index = argumentValues.Count;
                {
                    var paramType = function.GetParameterInfo(index).ParameterType;
                    var value = context.ToObject(span, arg, paramType);
                    SetArgumentValue(index, value, function, ref argMask, argumentValues, parameterCount);
                }
            }

            FillRemainingOptionalArguments(ref argMask, argumentValues.Count, parameterCount - 1, function,
                argumentValues);
            int requiredParameterCount = function.RequiredParameterCount;
            // Check the required number of arguments
            var requiredMask = (1U << requiredParameterCount) - 1;
            argMask = argMask & requiredMask;
            // Create a span after the caller for missing arguments
            var afterCallerSpan = callerContext?.Span ?? span;
            afterCallerSpan.Start = afterCallerSpan.End.NextColumn();
            afterCallerSpan.End = afterCallerSpan.End.NextColumn();
            if (argMask != requiredMask)
            {
                int argCount = 0;
                while (argMask != 0)
                {
                    if ((argMask & 1) != 0)
                        argCount++;
                    argMask = argMask >> 1;
                }

                throw new ScriptRuntimeException(afterCallerSpan,
                    $"Invalid number of arguments `{argCount}` passed to `{callerContext}` while expecting `{requiredParameterCount}` arguments");
            }
        }

        object result = null;
        if (callerContext is not null)
        {
            context.EnterFunction(callerContext);
        }

        try
        {
            result = await function.InvokeAsync(context, callerContext, argumentValues, null).ConfigureAwait(false);
        }
        catch (ArgumentException ex)
        {
            // Slow path to detect the argument index from the name if we can
            var index = GetParameterIndexByName(function, ex.ParamName);
            if (index >= 0 && arguments is not null && index < arguments.Count)
            {
                throw new ScriptRuntimeException(span, ex.Message);
            }

            throw;
        }
        catch (ScriptArgumentException ex)
        {
            var index = ex.ArgumentIndex;
            if (index >= 0 && arguments is not null && index < arguments.Count)
            {
                throw new ScriptRuntimeException(span, ex.Message);
            }

            throw;
        }
        finally
        {
            if (callerContext is not null)
            {
                context.ExitFunction(callerContext);
            }
        }

        // Restore the flow state to none
        context.FlowState = ScriptFlowState.None;
        return result;
    }
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        var target = Target;
        if (target is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid function call. Target is required.");
        }

        // Invoke evaluate on the target, but don't automatically call the function as if it was a parameterless call.
        var targetFunction = await context.EvaluateAsync(target, true).ConfigureAwait(false);
        // Throw an exception if the target function is null
        if (targetFunction is null)
        {
            if (context.EnableRelaxedFunctionAccess)
            {
                return null;
            }

            throw new ScriptRuntimeException(target.Span, $"The function `{target}` was not found");
        }

        return await CallAsync(context, this, targetFunction, context.AllowPipeArguments, Arguments)
            .ConfigureAwait(false);
    }

    private static async ValueTask<ulong> ProcessArgumentsAsync(TemplateContext context, ScriptNode callerContext,
        IReadOnlyList<ScriptExpression> arguments, IScriptCustomFunction function, ScriptFunction scriptFunction,
        ScriptArray argumentValues)
    {
        ulong argMask = 0;
        var parameterCount = function.ParameterCount;
        var callerSpan = callerContext?.Span ?? context.CurrentSpan;
        for (var argIndex = 0; argIndex < arguments.Count; argIndex++)
        {
            var argument = arguments[argIndex];
            int index = argumentValues.Count;
            object value;
            // Handle named arguments
            var namedArg = argument as ScriptNamedArgument;
            if (namedArg is not null)
            {
                var argName = namedArg.Name?.Name;
                if (argName is null)
                {
                    throw new ScriptRuntimeException(argument.Span, "Invalid null argument name");
                }

                index = GetParameterIndexByName(function, argName);
                // In case of a ScriptFunction, we write the named argument into the ScriptArray directly
                if (function.VarParamKind != ScriptVarParamKind.None)
                {
                    if (index >= 0)
                    {
                    }
                    // We can't add an argument that is "size" for array
                    else if (argumentValues.CanWrite(argName))
                    {
                        argumentValues.TrySetValue(context, callerSpan, argName,
                            await context.EvaluateAsync(namedArg).ConfigureAwait(false), false);
                        continue;
                    }
                    else
                    {
                        throw new ScriptRuntimeException(argument.Span,
                            $"Cannot pass argument {argName} to function. This name is not supported by this function.");
                    }
                }

                if (index < 0)
                {
                    index = argumentValues.Count;
                }
            }

            if (function.IsParameterType<ScriptExpression>(index))
            {
                value = namedArg is not null ? namedArg.Value : argument;
            }
            else
            {
                value = await context.EvaluateAsync(argument).ConfigureAwait(false);
            }

            // Handle parameters expansion for a function call when the operator ^ is used
            if (argument is ScriptUnaryExpression unaryExpression &&
                unaryExpression.Operator == ScriptUnaryOperator.FunctionParametersExpand &&
                !(value is ScriptExpression))
            {
                var valueEnumerator = value as IEnumerable;
                if (valueEnumerator is not null)
                {
                    foreach (var subValue in valueEnumerator)
                    {
                        var paramType = function.GetParameterInfo(argumentValues.Count).ParameterType;
                        var newValue = context.ToObject(callerSpan, subValue, paramType);
                        SetArgumentValue(index, newValue, function, ref argMask, argumentValues, parameterCount);
                        index++;
                    }

                    continue;
                }
            }

            {
                var paramType = function.GetParameterInfo(index).ParameterType;
                value = context.ToObject(argument.Span, value, paramType);
            }

            SetArgumentValue(index, value, function, ref argMask, argumentValues, parameterCount);
        }

        return argMask;
    }
}

public
    partial class ScriptIfStatement
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        if (Condition is null)
        {
            return null;
        }

        var conditionValue =
            context.ToBool(Condition.Span, await context.EvaluateAsync(Condition).ConfigureAwait(false));
        return conditionValue ? Then is null ? null : await context.EvaluateAsync(Then).ConfigureAwait(false) :
            Else is null ? null : await context.EvaluateAsync(Else).ConfigureAwait(false);
    }
}

public
    partial class ScriptImportStatement
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        if (Expression is null)
        {
            return null;
        }

        var value = await context.EvaluateAsync(Expression).ConfigureAwait(false);
        if (value is null)
        {
            return null;
        }

        context.Import(Expression.Span, value);
        return null;
    }
}

public
    partial class ScriptIncrementDecrementExpression
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        if (Right is null)
        {
            return null;
        }

        var increment = this.Operator == ScriptUnaryOperator.Increment ? 1 : -1;
        var value = Evaluate(context, this.Right.Span, ScriptUnaryOperator.Plus,
            await context.EvaluateAsync(this.Right).ConfigureAwait(false));
        var incrementedValue =
            ScriptBinaryExpression.Evaluate(context, this.Right.Span, ScriptBinaryOperator.Add, value, increment);
        await context.SetValueAsync(Right, incrementedValue).ConfigureAwait(false);
        return Post ? value : incrementedValue;
    }
}

public
    partial class ScriptIndexerExpression
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        return await context.GetValueAsync(this).ConfigureAwait(false);
    }

    private async ValueTask<object> GetOrSetValueAsync(TemplateContext context, object valueToSet, bool setter)
    {
        object value = null;
        var target = Target;
        var indexExpression = Index;
        if (target is null || indexExpression is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid indexer expression. Target and index are required.");
        }

        var targetObject = await context.GetValueAsync(target).ConfigureAwait(false);
        if (targetObject is null)
        {
            if (!setter && (context.EnableRelaxedTargetAccess || HasNullConditionalTarget(target)))
            {
                return null;
            }
            else
            {
                throw new ScriptRuntimeException(target.Span,
                    $"Object `{target}` is null. Cannot access indexer: {this}"); // unit test: 130-indexer-accessor-error1.txt
            }
        }

        var index = await context.EvaluateAsync(indexExpression).ConfigureAwait(false);
        if (index is null)
        {
            if (context.EnableNullIndexer)
            {
                return null;
            }
            else
            {
                throw new ScriptRuntimeException(indexExpression.Span,
                    $"Cannot access target `{target}` with a null indexer: {this}"); // unit test: 130-indexer-accessor-error2.txt
            }
        }

        var listAccessor = context.TryGetListAccessor(targetObject);
        if (targetObject is IDictionary ||
            (targetObject is IScriptObject && (listAccessor is null || index is string)) || listAccessor is null)
        {
            var accessor = context.GetMemberAccessor(targetObject);
            if (accessor.HasIndexer)
            {
                var indexType = accessor.IndexType;
                if (indexType is null)
                {
                    throw new ScriptRuntimeException(indexExpression.Span,
                        $"Cannot access target `{target}` with an untyped indexer: {this}");
                }

                var itemIndex = context.ToObject(indexExpression.Span, index, indexType);
                if (itemIndex is null)
                {
                    if (context.EnableNullIndexer)
                    {
                        return null;
                    }

                    throw new ScriptRuntimeException(indexExpression.Span,
                        $"Cannot access target `{target}` with a null indexer: {this}");
                }

                if (setter)
                {
                    if (!accessor.TrySetItem(context, indexExpression.Span, targetObject, itemIndex, valueToSet))
                    {
                        throw new ScriptRuntimeException(indexExpression.Span,
                            $"Cannot set a value for the readonly member `{itemIndex}` in the indexer: {target}['{itemIndex}']");
                    }
                }
                else
                {
                    var result = accessor.TryGetItem(context, indexExpression.Span, targetObject, itemIndex,
                        out value);
                    if (!context.EnableRelaxedMemberAccess && !result)
                    {
                        throw new ScriptRuntimeException(indexExpression.Span,
                            $"Cannot access target `{target}` with an indexer: {indexExpression}");
                    }
                }
            }
            else
            {
                var indexAsString = context.ObjectToString(index) ?? string.Empty;
                if (setter)
                {
                    if (!accessor.TrySetValue(context, indexExpression.Span, targetObject, indexAsString,
                            valueToSet))
                    {
                        throw new ScriptRuntimeException(indexExpression.Span,
                            $"Cannot set a value for the readonly member `{indexAsString}` in the indexer: {target}['{indexAsString}']"); // unit test: 130-indexer-accessor-error3.txt
                    }
                }
                else
                {
                    if (!accessor.TryGetValue(context, indexExpression.Span, targetObject, indexAsString,
                            out value))
                    {
                        var result = context.TryGetMember?.Invoke(context, indexExpression.Span, targetObject,
                            indexAsString, out value) ?? false;
                        if (!context.EnableRelaxedMemberAccess && !result)
                        {
                            throw new ScriptRuntimeException(indexExpression.Span,
                                $"Cannot access target `{target}` with an indexer: {indexExpression}");
                        }
                    }
                }
            }
        }
        else
        {
            int i = context.ToInt(indexExpression.Span, index);
            var length = listAccessor.GetLength(context, target.Span, targetObject);
            // Allow negative index from the end of the array
            if (i < 0)
            {
                i = length + i;
            }

            if (!context.EnableRelaxedIndexerAccess && (i < 0 || i >= length))
            {
                throw new ScriptRuntimeException(indexExpression.Span,
                    $"The index {i} is out of bounds [0, {length}] on the `{target}` with the indexer: {indexExpression}");
            }

            if (i >= 0)
            {
                if (setter)
                {
                    listAccessor.SetValue(context, indexExpression.Span, targetObject, i, valueToSet);
                }
                else
                {
                    value = listAccessor.GetValue(context, indexExpression.Span, targetObject, i);
                }
            }
        }

        return value;
    }
    /// <summary><c>GetValueAsync</c>.</summary>
    public async ValueTask<object> GetValueAsync(TemplateContext context)
    {
        return await GetOrSetValueAsync(context, null, false).ConfigureAwait(false);
    }
    /// <summary><c>SetValueAsync</c>.</summary>
    public async ValueTask SetValueAsync(TemplateContext context, object valueToSet)
    {
        await GetOrSetValueAsync(context, valueToSet, true).ConfigureAwait(false);
    }
}

public
    partial class ScriptInterpolatedExpression
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        if (Expression is null)
        {
            return null;
        }

        // A nested expression will reset the pipe arguments for the group
        context.PushPipeArguments();
        try
        {
            return await context.EvaluateAsync(Expression).ConfigureAwait(false);
        }
        finally
        {
            if (context.CurrentPipeArguments is not null)
            {
                context.PopPipeArguments();
            }
        }
    }
}

public
    partial class ScriptInterpolatedStringExpression
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        // A nested expression will reset the pipe arguments for the group
        context.PushPipeArguments();
        try
        {
            var builder = new System.Text.StringBuilder(); // TODO: use thread local
            foreach (var scriptExpression in Parts)
            {
                var value = await context.EvaluateAsync(scriptExpression).ConfigureAwait(false);
                if (value is not null)
                {
                    builder.Append(value);
                }
            }

            return builder.ToString();
        }
        finally
        {
            if (context.CurrentPipeArguments is not null)
            {
                context.PopPipeArguments();
            }
        }
    }
}

public
    partial class ScriptIsEmptyExpression
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        return await context.GetValueAsync(this).ConfigureAwait(false);
    }

    private async ValueTask<object> GetTargetObjectAsync(TemplateContext context, bool isSet)
    {
        var target = Target;
        if (target is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid `.empty?` expression. Target is required.");
        }

        var targetObject = await context.GetValueAsync(target).ConfigureAwait(false);
        if (targetObject is null)
        {
            if (isSet || !context.EnableRelaxedMemberAccess)
            {
                throw new ScriptRuntimeException(this.Span,
                    $"Object `{this.Target}` is null. Cannot access property `empty?`");
            }
        }

        return targetObject;
    }
    /// <summary><c>GetValueAsync</c>.</summary>
    public override async ValueTask<object> GetValueAsync(TemplateContext context)
    {
        var targetObject = await GetTargetObjectAsync(context, false).ConfigureAwait(false);
        return context.IsEmpty(Span, targetObject);
    }
}

/// <summary>
/// Base class for a loop statement
/// </summary>

public
    abstract partial class ScriptLoopStatementBase
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        // Notify the context that we enter a loop block (used for variable with scope Loop)
        object result = null;
        context.EnterLoop(this);
        try
        {
            result = await EvaluateImplAsync(context).ConfigureAwait(false);
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
}

public
    partial class ScriptMemberExpression
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        return await context.GetValueAsync(this).ConfigureAwait(false);
    }

    private async ValueTask<object> GetTargetObjectAsync(TemplateContext context, bool isSet)
    {
        var member = Member;
        var target = Target;
        if (member is null || target is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid member expression. Target and member are required.");
        }

        var targetObject = await context.GetValueAsync(target).ConfigureAwait(false);
        if (targetObject is null)
        {
            if (isSet || (context.EnableRelaxedMemberAccess == false &&
                          DotToken.TokenType != TokenType.QuestionDot))
            {
                throw new ScriptRuntimeException(member.Span,
                    $"Object `{target}` is null. Cannot access member: {this}"); // unit test: 131-member-accessor-error1.txt
            }
        }

        return targetObject;
    }
    /// <summary><c>GetValueAsync</c>.</summary>
    public virtual async ValueTask<object> GetValueAsync(TemplateContext context)
    {
        var member = Member;
        if (member is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid member expression. Member is required.");
        }

        var targetObject = await GetTargetObjectAsync(context, false).ConfigureAwait(false);
        // In case TemplateContext.EnableRelaxedMemberAccess
        if (targetObject is null)
        {
            if (context.EnableRelaxedTargetAccess || DotToken.TokenType == TokenType.QuestionDot)
            {
                return null;
            }

            throw new ScriptRuntimeException(member.Span, $"Cannot get the member {this} for a null object.");
        }

        var accessor = context.GetMemberAccessor(targetObject);
        var memberName = member.Name;
        object value;
        if (!accessor.TryGetValue(context, member.Span, targetObject, memberName, out value))
        {
            var result = context.TryGetMember?.Invoke(context, member.Span, targetObject, memberName, out value);
            if (!context.EnableRelaxedMemberAccess && (!result.HasValue || !result.Value))
            {
                throw new ScriptRuntimeException(member.Span,
                    $"Cannot get member with name {memberName}."); // unit test: 132-member-accessor-error2.txt
            }
        }

        return value;
    }
    /// <summary><c>SetValueAsync</c>.</summary>
    public virtual async ValueTask SetValueAsync(TemplateContext context, object valueToSet)
    {
        var member = Member;
        if (member is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid member expression. Member is required.");
        }

        var targetObject = await GetTargetObjectAsync(context, true).ConfigureAwait(false);
        if (targetObject is null)
        {
            throw new ScriptRuntimeException(member.Span,
                $"Object `{Target}` is null. Cannot access member: {this}");
        }

        var accessor = context.GetMemberAccessor(targetObject);
        var memberName = member.Name;
        if (!accessor.TrySetValue(context, member.Span, targetObject, memberName, valueToSet))
        {
            throw new ScriptRuntimeException(member.Span,
                $"Cannot set a value for the readonly member: {this}"); // unit test: 132-member-accessor-error3.txt
        }
    }
}

public
    partial class ScriptNamedArgument
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        if (Value is not null)
            return await context.EvaluateAsync(Value).ConfigureAwait(false);
        return true;
    }
}

public
    partial class ScriptNestedExpression
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        if (Expression is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid nested expression. Inner expression is required.");
        }

        // A nested expression will reset the pipe arguments for the group
        context.PushPipeArguments();
        try
        {
            return await context.GetValueAsync(this).ConfigureAwait(false);
        }
        finally
        {
            if (context.CurrentPipeArguments is not null)
            {
                context.PopPipeArguments();
            }
        }
    }
    /// <summary><c>GetValueAsync</c>.</summary>
    public async ValueTask<object> GetValueAsync(TemplateContext context)
    {
        if (Expression is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid nested expression. Inner expression is required.");
        }

        return await context.EvaluateAsync(Expression).ConfigureAwait(false);
    }
    /// <summary><c>SetValueAsync</c>.</summary>
    public async ValueTask SetValueAsync(TemplateContext context, object valueToSet)
    {
        if (Expression is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid nested expression. Inner expression is required.");
        }

        await context.SetValueAsync(Expression, valueToSet).ConfigureAwait(false);
    }
}

public
    partial class ScriptObjectInitializerExpression
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        var obj = new ScriptObject();
        context.PushGlobalOnly(obj);
        try
        {
            foreach (var member in Members)
            {
                await member.EvaluateAsync(context).ConfigureAwait(false);
            }
        }
        finally
        {
            context.PopGlobalOnly();
        }

        return obj;
    }
}

public
    partial class ScriptObjectMember
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        if (Name is null || Value is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid object member. Name and value are required.");
        }

        var variable = Name as ScriptVariable;
        var literal = Name as ScriptLiteral;
        var name = variable?.Name ?? literal?.Value?.ToString();
        if (string.IsNullOrEmpty(name))
        {
            throw new ScriptRuntimeException(Span, "Object member name cannot be empty.");
        }

        var currentGlobal = context.CurrentGlobal ??
                            throw new ScriptRuntimeException(Span, "No current global object is available.");
        var memberName = name ?? throw new ScriptRuntimeException(Span, "Object member name cannot be empty.");
        currentGlobal.TrySetValue(context, Span, memberName,
            await context.EvaluateAsync(Value).ConfigureAwait(false), false);
        return null;
    }
}

public
    partial class ScriptPage
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        context.FlowState = ScriptFlowState.None;
        try
        {
            return Body is null ? null : await context.EvaluateAsync(Body).ConfigureAwait(false);
        }
        finally
        {
            context.FlowState = ScriptFlowState.None;
        }
    }
}

public
    partial class ScriptPipeCall
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        var from = From;
        var to = To;
        if (from is null || to is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid pipe expression. Source and destination are required.");
        }

        bool newPipe = context.CurrentPipeArguments is null;
        try
        {
            // Push a new pipe arguments
            if (newPipe)
                context.PushPipeArguments();
            var pipeArguments = context.CurrentPipeArguments ??
                                throw new ScriptRuntimeException(Span, "Pipe arguments were not initialized.");
            pipeArguments.Push(from);
            var result = await context.EvaluateAsync(to).ConfigureAwait(false);
            // If the result returns by the evaluation is a function and we haven't yet consumed the pipe argument
            // that means that we need to evaluate this function with the actual pipe arguments.
            if (result is IScriptCustomFunction && pipeArguments.Count > 0 && pipeArguments.Peek() == from)
            {
                result = await ScriptFunctionCall.CallAsync(context, to, result, true, null).ConfigureAwait(false);
            }

            // If we have still remaining arguments, it is likely that the destination expression is not a function
            // so pipe arguments were not picked up and this is an error
            if (pipeArguments.Count > 0 && pipeArguments.Peek() == from)
            {
                throw new ScriptRuntimeException(to.Span,
                    $"Pipe expression destination `{to}` is not a valid function ");
            }

            return result;
        }
        catch
        {
            // If we have an exception clear all the pipe froms
            newPipe = false; // Don't try to clear the pipe
            context.ClearPipeArguments();
            throw;
        }
        finally
        {
            if (newPipe)
            {
                context.PopPipeArguments();
            }
        }
    }
}

public
    partial class ScriptRawStatement
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        if (Text.Length == 0 || string.IsNullOrEmpty(Text.FullText))
            return null;
        if (Text.Length > 0)
        {
            // If we are in the context of output, output directly to TemplateContext.Output
            if (context.EnableOutput)
            {
                await context.WriteAsync(Text).ConfigureAwait(false);
            }
            else
            {
                return Text.ToString();
            }
        }

        return null;
    }
}

public
    partial class ScriptReturnStatement
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        var result = Expression is null ? null : await context.EvaluateAsync(Expression).ConfigureAwait(false);
        //ensure that deferred array interators are evaluated before we lose context
        if (result is ScriptRange range)
        {
            result = new ScriptArray(range);
        }

        context.FlowState = ScriptFlowState.Return;
        return result;
    }
}

public
    partial class ScriptTableRowStatement
{
    /// <summary><c>AfterLoopAsync</c>.</summary>
    protected override async ValueTask AfterLoopAsync(TemplateContext context)
    {
        await context.Write("</tr>").WriteLineAsync().ConfigureAwait(false);
    }
    /// <summary><c>BeforeLoopAsync</c>.</summary>
    protected override async ValueTask BeforeLoopAsync(TemplateContext context)
    {
        await context.WriteAsync("<tr class=\"row1\">").ConfigureAwait(false);
    }
    /// <summary><c>LoopItemAsync</c>.</summary>
    protected override async ValueTask<object> LoopItemAsync(TemplateContext context, LoopState state)
    {
        var localIndex = state.Index;
        var columnIndex = localIndex % _columnsCount;
        var tableRowLoopState = (TableRowLoopState)state;
        tableRowLoopState.Col = columnIndex;
        tableRowLoopState.ColFirst = columnIndex == 0;
        tableRowLoopState.ColLast = ((localIndex + 1) % _columnsCount) == 0;
        if (columnIndex == 0 && localIndex > 0)
        {
            await context.Write("</tr>").WriteAsync(context.NewLine).ConfigureAwait(false);
            var rowIndex = (localIndex / _columnsCount) + 1;
            await context.Write("<tr class=\"row").Write(rowIndex.ToString(CultureInfo.InvariantCulture))
                .WriteAsync("\">").ConfigureAwait(false);
        }

        await context.Write("<td class=\"col").Write((columnIndex + 1).ToString(CultureInfo.InvariantCulture))
            .WriteAsync("\">").ConfigureAwait(false);
        var result = await base.LoopItemAsync(context, state).ConfigureAwait(false);
        await context.WriteAsync("</td>").ConfigureAwait(false);
        return result;
    }
    /// <summary><c>ProcessArgumentAsync</c>.</summary>
    protected override async ValueTask ProcessArgumentAsync(TemplateContext context, ScriptNamedArgument argument)
    {
        _columnsCount = 1;
        if (argument.Name?.Name == "cols")
        {
            if (argument.Value is null)
            {
                throw new ScriptRuntimeException(argument.Span, "Invalid `cols` argument. Value is required.");
            }

            _columnsCount = context.ToInt(argument.Value.Span,
                await context.EvaluateAsync(argument.Value).ConfigureAwait(false));
            if (_columnsCount <= 0)
            {
                _columnsCount = 1;
            }

            return;
        }

        await base.ProcessArgumentAsync(context, argument).ConfigureAwait(false);
    }
}

public
    partial class ScriptThisExpression
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        return await context.GetValueAsync(this).ConfigureAwait(false);
    }
}

public
    partial class ScriptUnaryExpression
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        if (Operator == ScriptUnaryOperator.FunctionAlias)
        {
            return await context.EvaluateAsync(Right, true).ConfigureAwait(false);
        }

        if (Right is null)
        {
            return Evaluate(context, Span, Operator, null);
        }

        var value = await context.EvaluateAsync(Right).ConfigureAwait(false);
        return Evaluate(context, Right.Span, Operator, value);
    }
}

public
    abstract partial class ScriptVariable
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        return await context.GetValueAsync((ScriptExpression)this).ConfigureAwait(false);
    }
    /// <summary><c>GetValueAsync</c>.</summary>
    public virtual async ValueTask<object> GetValueAsync(TemplateContext context)
    {
        return await context.GetValueAsync(this).ConfigureAwait(false);
    }
}

public
    partial class ScriptVariableGlobal
{
    /// <summary><c>GetValueAsync</c>.</summary>
    public override async ValueTask<object> GetValueAsync(TemplateContext context)
    {
        // Used a specialized overrides on contxet for ScriptVariableGlobal
        return await context.GetValueAsync(this).ConfigureAwait(false);
    }
}

public
    partial class ScriptWhenStatement
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        var caseValue = context.PeekCase();
        foreach (var value in Values)
        {
            var whenValue = await context.EvaluateAsync(value).ConfigureAwait(false);
            var result = ScriptBinaryExpression.Evaluate(context, Span, ScriptBinaryOperator.CompareEqual,
                caseValue, whenValue);
            if (result is bool booleanResult && booleanResult)
            {
                return Body is null ? null : await context.EvaluateAsync(Body).ConfigureAwait(false);
            }
        }

        return Next is null ? null : await context.EvaluateAsync(Next).ConfigureAwait(false);
    }
}

public
    partial class ScriptWhileStatement
{
    /// <summary><c>EvaluateImplAsync</c>.</summary>
    protected override async ValueTask<object> EvaluateImplAsync(TemplateContext context)
    {
        var index = 0;
        object result = null;
        await BeforeLoopAsync(context).ConfigureAwait(false);
        var loopState = CreateLoopState();
        context.SetLoopVariable(ScriptVariable.WhileObject, loopState);
        while (context.StepLoop(this))
        {
            if (Condition is null)
            {
                break;
            }

            var conditionResult = context.ToBool(Condition.Span,
                await context.EvaluateAsync(Condition).ConfigureAwait(false));
            if (!conditionResult)
            {
                break;
            }

            loopState.Index = index++;
            result = await LoopItemAsync(context, loopState).ConfigureAwait(false);
            if (!ContinueLoop(context))
            {
                break;
            }
        }

        ;
        await AfterLoopAsync(context).ConfigureAwait(false);
        return result;
    }
    /// <summary><c>LoopItemAsync</c>.</summary>
    protected override async ValueTask<object> LoopItemAsync(TemplateContext context, LoopState state)
    {
        return Body is null ? null : await context.EvaluateAsync(Body).ConfigureAwait(false);
    }
}

public
    partial class ScriptWithStatement
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        if (Name is null)
        {
            return null;
        }

        var target = await context.GetValueAsync(Name).ConfigureAwait(false);
        if (target is not IScriptObject scriptObject)
        {
            var targetName = target?.GetType().Name ?? "null";
            throw new ScriptRuntimeException(Name.Span,
                $"Invalid target property `{Name}` used for [with] statement. Must be a ScriptObject instead of `{targetName}`");
        }

        context.PushGlobal(scriptObject);
        try
        {
            var result = Body is null ? null : await context.EvaluateAsync(Body).ConfigureAwait(false);
            return result;
        }
        finally
        {
            context.PopGlobal();
        }
    }
}

public
    partial class ScriptWrapStatement
{
    /// <summary><c>EvaluateAsync</c>.</summary>
    public override async ValueTask<object> EvaluateAsync(TemplateContext context)
    {
        var target = Target;
        var body = Body;
        if (target is null || body is null)
        {
            throw new ScriptRuntimeException(Span, "Invalid wrap statement. Target and body are required.");
        }

        // Check that the Target is actually a function
        var functionCall = target as ScriptFunctionCall;
        if (functionCall is null)
        {
            var parameterLessFunction = await context.EvaluateAsync(target, true).ConfigureAwait(false);
            if (!(parameterLessFunction is IScriptCustomFunction))
            {
                var targetPrettyName = ScriptSyntaxAttribute.Get(target);
                throw new ScriptRuntimeException(target.Span,
                    $"Expecting a direct function instead of the expression `{target}/{targetPrettyName?.TypeName ?? "unknown"}`");
            }

            context.BlockDelegates.Push(body);
            return await ScriptFunctionCall.CallAsync(context, this, parameterLessFunction, false, null)
                .ConfigureAwait(false);
        }

        context.BlockDelegates.Push(body);
        return await context.EvaluateAsync(functionCall).ConfigureAwait(false);
    }
}
