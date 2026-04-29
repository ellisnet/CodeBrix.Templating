// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Xunit;
using CodeBrix.Templating.Runtime;

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

    public class ValueWrapper
    {
        public string Value { get; set; }


        public ValueWrapper(string value)
        {
            Value = value;
        }
    }

}
