// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Xunit;
using CodeBrix.Templating.Runtime;
using CodeBrix.Templating.Syntax;

namespace CodeBrix.Templating.Tests; //was previously: Scriban.Tests;

public class TestAsync
{
    [Fact]
    public async Task AccessDirectlyOnFunctionResult()
    {
        var templateBody = "{{my_function().value}}";

        var templateContext = new TemplateContext
        {
            EnableRelaxedMemberAccess = false,
            StrictVariables = true
        };

        var template = Template.Parse(templateBody);
        Assert.False(template.HasErrors);

        var so = new ScriptObject();
        so.Import("my_function", new Func<Task<ValueWrapper>>(async () =>
        {
            await Task.Delay(1);
            return new ValueWrapper("hello");
        }));

        templateContext.PushGlobal(so);

        var result = await template.RenderAsync(templateContext);

        Assert.Equal("hello", result);
    }

    [Fact]
    public async Task IndirectAccess()
    {
        var templateBody = @"{{v = my_function()
v.value}}";

        var templateContext = new TemplateContext
        {
            EnableRelaxedMemberAccess = false,
            StrictVariables = true
        };

        var template = Template.Parse(templateBody);
        Assert.False(template.HasErrors);

        var so = new ScriptObject();
        so.Import("my_function", new Func<Task<ValueWrapper>>(async () =>
        {
            await Task.Delay(1);
            return new ValueWrapper("hello");
        }));

        templateContext.PushGlobal(so);

        var result = await template.RenderAsync(templateContext);

        Assert.Equal("hello", result);
    }

    [Fact]
    public async Task NullConditionalShouldShortCircuitFollowingIndexersAsync()
    {
        var template = Template.Parse("{{ a?.b[0][1] }}");

        var nullResult = await template.RenderAsync(new { a = (object)null });
        Assert.Equal(string.Empty, nullResult);

        var valueResult = await template.RenderAsync(new { a = new { b = new[] { new[] { "skip", "ok" } } } });
        Assert.Equal("ok", valueResult);
    }

    [Fact]
    public async Task RenderAsyncShouldAwaitTaskMemberValues()
    {
        var template = Template.Parse("{{ value }}|{{ value + 1 }}");

        var result = await template.RenderAsync(new { value = Task.FromResult(41) });

        Assert.Equal("41|42", result);
    }

    [Fact]
    public async Task RenderAsyncShouldAwaitValueTaskMemberValues()
    {
        var template = Template.Parse("{{ value }}");

        var result = await template.RenderAsync(new { value = ValueTask.FromResult("hello") });

        Assert.Equal("hello", result);
    }

    [Fact]
    public async Task RenderAsyncShouldUseFunctionScopeForParametricFunctions()
    {
        var template = Template.Parse(@"
{{-
my_global_var = 1

func mutate_global(x)
    my_global_var += 1
end

mutate_global 0
my_global_var
-}}
");

        var result = await template.RenderAsync();

        Assert.Equal("2", result);
    }

    [Fact]
    public async Task RenderAsyncShouldShareNestedIterationLoopLimit()
    {
        var context = new TemplateContext
        {
            LoopLimit = 10
        };
        var template = Template.Parse("{{ for i in 1..2; [1, 2, 3, 4, 5] | array.reverse | array.size; end }}");

        var exception = await Assert.ThrowsAsync<ScriptRuntimeException>(async () => await template.RenderAsync(context));

        Assert.Contains("iteration limit `10`", exception.Message);
    }

    [Fact]
    public async Task RenderAsyncShouldResetCumulativeOutputTracking()
    {
        var context = new TemplateContext
        {
            LimitToString = 5
        };
        var largeTemplate = Template.Parse("{{ 'abc' }}{{ 'def' }}");
        var smallTemplate = Template.Parse("{{ 'xy' }}");

        Assert.Equal("abcde...", await largeTemplate.RenderAsync(context));
        Assert.Equal("xy", await smallTemplate.RenderAsync(context));
    }

    [Theory]
    [InlineData(0, "abc...abc...")]
    [InlineData(8, "abc...ab...")]
    public async Task RenderAsyncShouldUseIndependentOutputLimit(int outputLimit, string expected)
    {
        var context = new TemplateContext { LimitToString = 3, OutputLimit = outputLimit };
        var template = Template.Parse("{{ 'abcd' }}{{ 'abcd' }}");

        Assert.Equal(expected, await template.RenderAsync(context));
        Assert.Equal(expected, await template.RenderAsync(context));
    }

    [Fact]
    public async Task RenderAsyncShouldThrowOnOutputLimitAndRecover()
    {
        var context = new TemplateContext { OutputLimit = 5, OnOutputLimit = ScriptLimitBehavior.Throw };

        var exception = await Assert.ThrowsAsync<ScriptRuntimeException>(async () => await Template.Parse("abc{{ 'def' }}").RenderAsync(context));

        Assert.Contains("OutputLimit `5`", exception.Message);
        Assert.Equal("abc", context.Output.ToString());
        context.Reset();
        Assert.Equal("abcde", await Template.Parse("abcde").RenderAsync(context));
    }

    [Fact]
    public async Task RenderAsyncShouldThrowOnStringLimit()
    {
        var context = new TemplateContext { LimitToString = 3, OutputLimit = 0, OnStringLimit = ScriptLimitBehavior.Throw };

        var exception = await Assert.ThrowsAsync<ScriptRuntimeException>(async () => await Template.Parse("{{ 'abcd' }}").RenderAsync(context));

        Assert.Contains("LimitToString `3`", exception.Message);
    }

    public class ValueWrapper
    {
        public string Value { get; set; }


        public ValueWrapper(string value)
        {
            Value = value;
        }
    }

}
