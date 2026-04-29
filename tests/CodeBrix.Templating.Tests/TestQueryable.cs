using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using CodeBrix.Templating.Runtime;
using CodeBrix.Templating.Syntax;

namespace CodeBrix.Templating.Tests; //was previously: Scriban.Tests;

public class TestQueryable
{
    private static IScriptObject GetGlobal(TemplateContext context)
    {
        return context.CurrentGlobal ?? throw new Xunit.Sdk.XunitException("Expected a current global script object.");
    }

    [Fact]
    public void TestQueryableAll ()
    {
        var context = new TemplateContext();
        GetGlobal(context).Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 10).AsQueryable()));

        var template = Template.Parse(@"{{
for item in data
item
end
}}");
        var result = template.Render(context);

        TextAssert.AreEqual("0123456789", result);
    }

    [Fact]
    public void TestQueryableOffset()
    {
        var context = new TemplateContext();
        GetGlobal(context).Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 10).AsQueryable()));

        var template = Template.Parse(@"{{
for item in data offset:2
item
end
}}");
        var result = template.Render(context);

        TextAssert.AreEqual("23456789", result);
    }

    [Fact]
    public void TestQueryableLimit()
    {
        var context = new TemplateContext();
        GetGlobal(context).Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 10).AsQueryable()));

        var template = Template.Parse(@"{{
for item in data limit:2
item
end
}}");
        var result = template.Render(context);

        TextAssert.AreEqual("01", result);
    }

    [Fact]
    public void TestQueryableReversed()
    {
        var context = new TemplateContext();
        GetGlobal(context).Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 10).AsQueryable()));

        var template = Template.Parse(@"{{
for item in data reversed
item
end
}}");
        var result = template.Render(context);

        TextAssert.AreEqual("9876543210", result);
    }


    [Fact]
    public void TestQueryableIndex()
    {
        var context = new TemplateContext();
        GetGlobal(context).Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(5, 5).AsQueryable()));

        var template = Template.Parse(@"{{
for item in data
for.index
end
}}");
        var result = template.Render(context);

        TextAssert.AreEqual("01234", result);
    }

     [Fact]
    public void TestQueryableRIndex()
    {
        var context = new TemplateContext();
        GetGlobal(context).Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 5).AsQueryable()));

        var template = Template.Parse(@"{{
for item in data
for.rindex
end
}}");
        var result = template.Render(context);

        TextAssert.AreEqual("43210", result);
    }


    [Fact]
    public void TestQueryableFirst()
    {
        var context = new TemplateContext();
        GetGlobal(context).Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 4).AsQueryable()));

        var template = Template.Parse(@"{{
for item in data
for.first
end
}}");
        var result = template.Render(context);

        TextAssert.AreEqual("truefalsefalsefalse", result);
    }

    [Fact]
    public void TestQueryableLast()
    {
        var context = new TemplateContext();
        GetGlobal(context).Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 5).AsQueryable()));

        var template = Template.Parse(@"{{
for item in data
for.last
end
}}");
        var result = template.Render(context);

        // last will always be false with iqueryable
        TextAssert.AreEqual("falsefalsefalsefalsetrue", result);
    }


    [Fact]
    public void TestQueryableEven()
    {
        var context = new TemplateContext();
        GetGlobal(context).Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 4).AsQueryable()));

        var template = Template.Parse(@"{{
for item in data
for.even
end
}}");
        var result = template.Render(context);

        TextAssert.AreEqual("truefalsetruefalse", result);
    }

    [Fact]
    public void TestQueryableOdd()
    {
        var context = new TemplateContext();
        GetGlobal(context).Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 4).AsQueryable()));

        var template = Template.Parse(@"{{
for item in data
for.odd
end
}}");
        var result = template.Render(context);

        TextAssert.AreEqual("falsetruefalsetrue", result);
    }

    [Fact]
    public void TestQueryableChanged()
    {
        var context = new TemplateContext();
        GetGlobal(context).Import("data", new Func<IQueryable<int>>(() => new[] { 0,0,1,1,2 }.AsQueryable()));

        var template = Template.Parse(@"{{
for item in data
for.changed
end
}}");
        var result = template.Render(context);

        TextAssert.AreEqual("truefalsetruefalsetrue", result);
    }


    [Fact]
    public void TestQueryableLoopLimit()
    {
        var context = new TemplateContext
        {
            LoopLimit = 5
        };
        GetGlobal(context).Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 10).AsQueryable()));

        var template = Template.Parse(@"{{
for item in data
item
end
}}");
        var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context)) ?? throw new Xunit.Sdk.XunitException("Expected a ScriptRuntimeException.");
        TextAssert.AreEqual("<input>(2,1) : error : Exceeding number of iteration limit `5` for loop statement.", exception.Message);

    }

    [Fact]
    public void TestQueryableLoopLimitQueryableOverride()
    {
        var context = new TemplateContext
        {
            LoopLimit = 5,
            LoopLimitQueryable = 6,
        };
        GetGlobal(context).Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 10).AsQueryable()));

        var template = Template.Parse(@"{{
for item in data
item
end
}}");
        var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context)) ?? throw new Xunit.Sdk.XunitException("Expected a ScriptRuntimeException.");
        TextAssert.AreEqual("<input>(2,1) : error : Exceeding number of iteration limit `6` for loop statement.", exception.Message);

    }

    [Fact]
    public void TestQueryableLoopLimitQueryableDisable()
    {
        var context = new TemplateContext
        {
            LoopLimit = 5,
            LoopLimitQueryable = 0,
        };
        GetGlobal(context).Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 10).AsQueryable()));

        var template = Template.Parse(@"{{
for item in data
item
end
}}");
        var result = template.Render(context);

        TextAssert.AreEqual("0123456789", result);
    }

    [Fact]
    public void TestQueryableArraySizeUsesQueryableLoopLimit()
    {
        var context = new TemplateContext
        {
            LoopLimit = 5,
            LoopLimitQueryable = 6,
        };
        context.PushGlobal(new ScriptObject
        {
            { "data", Enumerable.Range(0, 10).AsQueryable() }
        });

        var template = Template.Parse("{{ data | array.size }}");

        var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context)) ?? throw new Xunit.Sdk.XunitException("Expected a ScriptRuntimeException.");

        TextAssert.AreEqual("<input>(1,11) : error : Exceeding number of iteration limit `6` for internal iteration.", exception.Message);
    }
}
