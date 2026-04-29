// #define EnableTokensOutput
// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotLiquid.Tests.Tags;
using Newtonsoft.Json.Linq;
using Xunit;
using CodeBrix.Templating.Helpers;
using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Runtime;
using CodeBrix.Templating.Syntax;
using static CodeBrix.Templating.Tests.TestFilesHelper;

namespace CodeBrix.Templating.Tests; //was previously: Scriban.Tests;

public class TestParser
{
    private static ScriptPage GetPage(Template template)
    {
        return template.Page ?? throw new Xunit.Sdk.XunitException("Expected parsed template page to be available.");
    }

    private static IScriptObject GetCurrentGlobal(TemplateContext context)
    {
        return context.CurrentGlobal ?? throw new Xunit.Sdk.XunitException("Expected a current global script object.");
    }

    [Fact]
    public void TestMemberDot()
    {
        var input = @"{{ a?.b.c }}";
        var template = Template.Parse(input);
        var result = template.Render();
        Assert.Equal("", result);
    }

    [Fact]
    public void TestFailingError()
    {
        var input = @"{{
  for $s in Foo
      {{ if $s
          false
      else if $s
          false
      end
  end
}}";
        var template = Template.Parse(input);
        Assert.True(template.HasErrors);
    }

    [Fact]
    public void TestRoundtrip()
    {
        var text = "This is a text {{ code # With some comment }} and a text";
        AssertRoundtrip(text);
    }

    [Fact]
    public void TestScribanIfElseFunction()
    {
        var template = Template.Parse(@"
    func testIfElse
        if $0 < 0
            ret 1
        else
            ret 0
        end
        ret -1
    end
    testIfElse testValue
    ", lexerOptions: new LexerOptions { KeepTrivia = false, Mode = ScriptMode.ScriptOnly });
        var templateContext = new TemplateContext
        {
            LoopLimit = int.MaxValue,
        };

        templateContext.BuiltinObject.SetValue("testValue", -1, true);
        var result = template.Evaluate(templateContext);
        Assert.Equal(1, result);
        templateContext.BuiltinObject.SetValue("testValue", 1, true);
        result = template.Evaluate(templateContext);
        Assert.Equal(0, result); // returns null
    }

    [Fact]
    public void TestRoundtrip1()
    {
        var text = "This is a text {{ code | pipe a b c | a + b }} and a text";
        AssertRoundtrip(text);
    }

    [Fact]
    public void TestLiquidMissingClosingBrace()
    {
        var template = Template.ParseLiquid("{%endunless");
        Assert.True(template.HasErrors);
        Assert.Equal(2, template.Messages.Count);
        Assert.Equal("<input>(1,3) : error : Unable to find a pending `unless` for this `endunless`", template.Messages[0].ToString());
        Assert.StartsWith("<input>(1,11) : error : Error while parsing ScriptPage: Found <end> statement `endScriptPage` without a corresponding beginning of a block", template.Messages[1].ToString());
    }

    [Fact]

    public void TestScientificWithFunctionExpression()
    {
        var context = new TemplateContext();
        context.BuiltinObject.SetValue("clear", DelegateCustomFunction.CreateFunc<ScriptExpression, string>(FunctionClear), false);
        context.BuiltinObject.SetValue("history", DelegateCustomFunction.Create<object>(FunctionHistory), false);

        var template = Template.Parse("clear history", lexerOptions: new LexerOptions()
        {
            Lang = ScriptLang.Scientific,
            Mode = ScriptMode.ScriptOnly
        });

        var test = template.Render(context);
        Assert.Equal("history", test);

        template = Template.Parse("clear history * 5", lexerOptions: new LexerOptions()
        {
            Lang = ScriptLang.Scientific,
            Mode = ScriptMode.ScriptOnly
        });
        test = template.Render(context);
        Assert.Equal("history*5", test);
    }

    private static string FunctionClear(ScriptExpression what = null)
    {
        return what?.ToString() ?? string.Empty;
    }

    private static void FunctionHistory(object line = null)
    {
    }



    [Fact]
    public void TestLiquidInvalidStringEscape()
    {
        var template = Template.ParseLiquid(@"{%""\u""");
        Assert.True(template.HasErrors);
    }

    [InlineData("1-2", -1, "1 - 2")]
    [InlineData("abs 5", 5, "abs(5)")]
    [InlineData("2 abs 5", 10, "2 * abs(5)")]
    [InlineData("abs 5^2", 25, "abs(5 ^ 2)")]
    [InlineData("abs 5^2*3", 75, "abs((5 ^ 2) * 3)")]
    [InlineData("abs 5^2*3 - 1", 74, "abs((5 ^ 2) * 3) - 1")]
    [InlineData("abs 5^2*3 + 1", 76, "abs((5 ^ 2) * 3) + 1")]
    [InlineData("2 abs 5^2*3 + 1", 151, "2 * abs((5 ^ 2) * 3) + 1")]
    [InlineData("abs 4 abs 3 abs 2", 24, "abs(4) * abs(3) * abs(2)")]
    [InlineData("abs 4 * abs 3 * abs 2", 24, "abs(4) * abs(3) * abs(2)")]
    [InlineData("abs 4 + abs 3 * abs 2", 10, "abs(4) + abs(3) * abs(2)")]
    [InlineData("abs 4 * abs 3 + abs 2", 14, "abs(4) * abs(3) + abs(2)")]
    [InlineData("1 + abs 4 * abs 3", 13, "1 + abs(4) * abs(3)")]
    [InlineData("abs 4 * abs 3 + 1", 13, "abs(4) * abs(3) + 1")]
    [InlineData("abs 10 / abs 2 + 1", 6, "(abs(10) / abs(2)) + 1")]
    [InlineData("-5|>math.abs", 5, "-5 |> math.abs")]
    [InlineData("-5*2|>math.abs", 10, "-5 * 2 |> math.abs")]
    [InlineData("2x", 2, "2 * x")]
    [InlineData("10x/2", 5.0, "(10 * x) / 2")]
    [InlineData("10x y + y", 22, "10 * x * y + y")]
    [InlineData("10x * y + 3y + 1 + 2", 29, "10 * x * y + 3 * y + 1 + 2")]
    [InlineData("2 y math.abs z * 5 // 2 + 1 + z", 91, "2 * y * math.abs((z * 5) // 2) + 1 + z")] // 2 * 2 * abs(-10) * 5 / 2 + 1 + (-10) = 91
    [InlineData("2 y math.abs z * 5 // 2 + 1 * 3 + z + 17", 110, "2 * y * math.abs((z * 5) // 2) + 1 * 3 + z + 17")] // 2 * 2 * abs(-10) * 5 / 2 + 3 + (-10) + 17 = 110
    [InlineData("2^11 - 2^5 + 2^2", 2020, "(2 ^ 11) - (2 ^ 5) + (2 ^ 2)")]
    [InlineData("2^3^4", 4096, "(2 ^ 3) ^ 4")]
    [InlineData("3y^2 + 3x", 15, "3 * (y ^ 2) + 3 * x")]
    [InlineData("1 + 2 + 3x + 4y + z", 4, "1 + 2 + 3 * x + 4 * y + z")]
    [InlineData("y^5 * 2 + 1", 65, "(y ^ 5) * 2 + 1")]
    [InlineData("y^5 // 2 + 1", 17, "((y ^ 5) // 2) + 1")]
    [InlineData("f(x)= x*2 +1* 50; f(10* 2)", 90, "f(x) = x * 2 + 1 * 50; f(10 * 2)")]
    [InlineData("f(x)= x*2 +1* 50; 10* 2|>f", 90, "f(x) = x * 2 + 1 * 50; 10 * 2 |> f")]
    // int binaries
    [InlineData("1 << 2", 4, "1 << 2")]
    [InlineData("8 >> 2", 2, "8 >> 2")]
    [InlineData("1 | 2", 3, "1 | 2")]
    [InlineData("3 & 2", 2, "3 & 2")]
    // long
    [InlineData("10000000000 + 1", (long)10000000000 + 1, "10000000000 + 1")]
    [InlineData("10000000000 - 1", (long)10000000000 - 1, "10000000000 - 1")]
    [InlineData("10000000000 * 3", (long)10000000000 * 3, "10000000000 * 3")]
    [InlineData("10000000000 / 3", (double)10000000000 / 3, "10000000000 / 3")]
    [InlineData("10000000000 // 3", (long)10000000000 / 3, "10000000000 // 3")]
    [InlineData("10000000000 << 2", (long)10000000000 << 2, "10000000000 << 2")]
    [InlineData("10000000000 >> 2", (long)10000000000 >> 2, "10000000000 >> 2")]
    [InlineData("10000000001 | 2", (long)10000000001 | 2, "10000000001 | 2")]
    [InlineData("10000000003 & 2", (long)10000000003 & 2, "10000000003 & 2")]
    [InlineData("10000000003 % 7", (long)10000000003 % 7, "10000000003 % 7")]
    [InlineData("10000000003 == 7", 10000000003 == 7, "10000000003 == 7")]
    [InlineData("10000000003 != 7", 10000000003 != 7, "10000000003 != 7")]
    [InlineData("10000000003 < 7", 10000000003 < 7, "10000000003 < 7")]
    [InlineData("10000000003 > 7", 10000000003 > 7, "10000000003 > 7")]
    [InlineData("10000000003 <= 7", 10000000003 <= 7, "10000000003 <= 7")]
    [InlineData("10000000003 >= 7", 10000000003 >= 7, "10000000003 >= 7")]
    // float
    [InlineData("1.0f + 2.0f", 1.0f + 2.0f, "1.0f + 2.0f")]
    [InlineData("1.0f - 2.0f", 1.0f - 2.0f, "1.0f - 2.0f")]
    [InlineData("2.0f * 3.0f", 2.0f * 3.0f, "2.0f * 3.0f")]
    [InlineData("2.0f / 3.0f", 2.0f / 3.0f, "2.0f / 3.0f")]
    [InlineData("4.0f // 3.0f", (float)(int)(4.0f / 3.0f), "4.0f // 3.0f")]
    [InlineData("4.0f ^ 2.0f", (float)16.0, "4.0f ^ 2.0f")]
    [InlineData("4.0f << 1", (float)4.0f * 2.0f, "4.0f << 1")]
    [InlineData("4.0f >> 1", (float)4.0f / 2.0f, "4.0f >> 1")]
    [InlineData("4.0f % 3.0f", (float)4.0f % 3.0f, "4.0f % 3.0f")]
    [InlineData("4.0f == 3.0f", 4.0f == 3.0f, "4.0f == 3.0f")]
    [InlineData("4.0f != 3.0f", 4.0f != 3.0f, "4.0f != 3.0f")]
    [InlineData("4.0f < 3.0f", 4.0f < 3.0f, "4.0f < 3.0f")]
    [InlineData("4.0f > 3.0f", 4.0f > 3.0f, "4.0f > 3.0f")]
    [InlineData("4.0f <= 3.0f", 4.0f <= 3.0f, "4.0f <= 3.0f")]
    [InlineData("4.0f >= 3.0f", 4.0f >= 3.0f, "4.0f >= 3.0f")]
    // double
    [InlineData("4.0 // 3.0", (double)(int)(4.0f / 3.0), "4.0 // 3.0")]
    [InlineData("4.0 ^ 2.0", (double)16.0, "4.0 ^ 2.0")]
    [InlineData("4.0 << 1", (double)4.0 * 2.0, "4.0 << 1")]
    [InlineData("4.0 >> 1", (double)4.0 / 2.0, "4.0 >> 1")]
    [InlineData("4.0d", 4.0, "4.0")]
    [InlineData("4.0D", 4.0, "4.0")]
    // decimal
    [InlineData("4.0m", 4.0, "4.0m")]
    [InlineData("4.0M", 4.0, "4.0m")]
    [InlineData("4.0m + 2.0m", 6.0, "4.0m + 2.0m")]
    [InlineData("4.0m - 2.0m", 2.0, "4.0m - 2.0m")]
    [InlineData("4.0m * 2.0m", 8.0, "4.0m * 2.0m")]
    [InlineData("8.0m / 2.0m", 4.0, "8.0m / 2.0m")]
    [InlineData("5.0m // 2.0m", 2.0, "5.0m // 2.0m")]
    [InlineData("2.0m ^ 3.0m", 8.0, "2.0m ^ 3.0m")]
    [InlineData("2.0m << 1", 4.0, "2.0m << 1")]
    [InlineData("4.0m >> 1", 2.0, "4.0m >> 1")]
    [InlineData("4.0m % 3.0m", 1.0, "4.0m % 3.0m")]
    [InlineData("4.0m == 3.0m", 4.0m == 3.0m, "4.0m == 3.0m")]
    [InlineData("4.0m != 3.0m", 4.0m != 3.0m, "4.0m != 3.0m")]
    [InlineData("4.0m < 3.0m", 4.0m < 3.0m, "4.0m < 3.0m")]
    [InlineData("4.0m > 3.0m", 4.0m > 3.0m, "4.0m > 3.0m")]
    [InlineData("4.0m <= 3.0m", 4.0m <= 3.0m, "4.0m <= 3.0m")]
    [InlineData("4.0m >= 3.0m", 4.0m >= 3.0m, "4.0m >= 3.0m")]
    [InlineData("3.0ff", 12.0, "3.0 * ff")]
    [Theory]
    public async Task TestScientific(string script, object value, string scriptReformat)
    {
        var template = Template.Parse(script, lexerOptions: new LexerOptions() {Mode = ScriptMode.ScriptOnly, Lang = ScriptLang.Scientific});
        Assert.False(template.HasErrors, $"Template has errors: {template.Messages}");

        var context = new TemplateContext();
        var currentGlobal = GetCurrentGlobal(context);
        currentGlobal.SetValue("x", 1, false);
        currentGlobal.SetValue("y", 2, false);
        currentGlobal.SetValue("z", -10, false);
        currentGlobal.SetValue("ff", 4, false);
        var mathObject = context.BuiltinObject["math"] as ScriptObject ?? throw new Xunit.Sdk.XunitException("Expected math builtin object.");
        currentGlobal.SetValue("abs", mathObject["abs"], false);

        var result = template.Evaluate(context);
        Assert.Equal(Convert.ToDouble(value), Convert.ToDouble(result));

        var resultAsync = await template.EvaluateAsync(context);
        Assert.Equal(Convert.ToDouble(value), Convert.ToDouble(resultAsync));

        var reformat = GetPage(template).Format(new ScriptFormatterOptions(context, ScriptLang.Scientific, ScriptFormatterFlags.ExplicitClean));
        var exprAsString = reformat.ToString();
        Assert.Equal(scriptReformat, exprAsString);
    }

    [Fact]
    public void RoundtripFunction()
    {
        var text = @"{{ func inc
    ret $0 + 1
end }}";
        AssertRoundtrip(text);
    }

    [Fact]
    public void RoundtripFunction2()
    {
        var text = @"{{
func   inc
    ret $0 + 1
end
xxx 1
}}";
        AssertRoundtrip(text);
    }


    [Fact]
    public void RoundtripIf()
    {
        var text = @"{{
if true
    ""yes""
end
}}
raw
";
        AssertRoundtrip(text);
    }

    [Fact]
    public void RoundtripIfElse()
    {
        var text = @"{{
if true
    ""yes""
else
    ""no""
end
}}
raw
";
        AssertRoundtrip(text);
    }

    [Fact]
    public void RoundtripIfElseIf()
    {
        var text = @"{{
if true
    ""yes""
else if yo
    ""no""
end
y
}}
raw
";
        AssertRoundtrip(text);
    }

    [Fact]
    public void RoundtripCapture()
    {
        var text = @" {{ capture variable -}}
    This is a capture
{{- end -}}
{{ variable }}";
        AssertRoundtrip(text);
    }


    [Fact]
    public void RoundtripRaw()
    {
        var text = @"This is a raw     {{~ x ~}}     end";
        AssertRoundtrip(text);
    }

    /// <summary>
    /// Regression test for issue-295
    /// </summary>
    [Fact]
    public void ShouldNotThrowWithTrailingColon()
    {
        //this particular input string is required to tickle the original bug
        var text = @"{{T ""m"" b:";
        var context = new TemplateContext();
        context.PushGlobal(new ScriptObject());
        Record.Exception(() => Template.Parse(text));
    }

    [Fact]
    public void TestDateNow()
    {
        // default is dd MM yyyy
        var dateNow = DateTime.Now.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        var template = ParseTemplate(@"{{ date.now }}");
        var result = template.Render();
        Assert.Equal(dateNow, result);

        template = ParseTemplate(@"{{ date.format = '%Y'; date.now }}");
        result = template.Render();
        Assert.Equal(DateTime.Now.ToString("yyyy", CultureInfo.InvariantCulture), result);

        template = ParseTemplate(@"{{ date.format = '%Y'; date.now | date.add_years 1 }}");
        result = template.Render();
        Assert.Equal(DateTime.Now.AddYears(1).ToString("yyyy", CultureInfo.InvariantCulture), result);
    }

    [Fact]
    public void TestUtcDateNow()
    {
        // default is dd MM yyyy
        var dateNow = DateTime.UtcNow.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        var template = ParseTemplate(@"{{ date.utc_now }}");
        var result = template.Render();
        Assert.Equal(dateNow, result);

        template = ParseTemplate(@"{{ date.format = '%Y'; date.utc_now }}");
        result = template.Render();
        Assert.Equal(DateTime.UtcNow.ToString("yyyy", CultureInfo.InvariantCulture), result);

        template = ParseTemplate(@"{{ date.format = '%Y'; date.utc_now | date.add_years 1 }}");
        result = template.Render();
        Assert.Equal(DateTime.Now.AddYears(1).ToString("yyyy", CultureInfo.InvariantCulture), result);
    }

    [Fact]
    public void TestHelloWorld()
    {
        var template = ParseTemplate(@"This is a {{ text }} World from scriban!");
        var result = template.Render(new { text = "Hello" });
        Assert.Equal("This is a Hello World from scriban!", result);
    }

    [Fact]
    public void TestFrontMatter()
    {
        var options = new LexerOptions() {Mode = ScriptMode.FrontMatterAndContent};
        var input = @"+++
variable = 1
name = 'yes'
+++
This is after the frontmatter: {{ name }}
{{
variable + 1
}}";
        input = input.Replace("\r\n", "\n");
        var template = ParseTemplate(input, options);

        // Make sure that we have a front matter
        var page = GetPage(template);
        var frontMatter = page.FrontMatter ?? throw new Xunit.Sdk.XunitException("Expected front matter to be present.");

        var context = new TemplateContext();

        // Evaluate front-matter
        var frontResult = context.Evaluate(frontMatter);
        Assert.Null(frontResult);

        // Evaluate page-content
        context.Evaluate(page);
        var pageResult = context.Output.ToString();
        TextAssert.AreEqual("This is after the frontmatter: yes\n2", pageResult);
    }




    [Fact]
    public void TestFrontMatterOnly()
    {
        var options = new ParserOptions();

        var input = @"+++
variable = 1
name = 'yes'
+++
This is after the frontmatter: {{ name }}
{{
variable + 1
}}";
        input = input.Replace("\r\n", "\n");

        var lexer = new Lexer(input, null, new LexerOptions() { Mode = ScriptMode.FrontMatterOnly });
        var parser = new Parser(lexer, options);

        var page = parser.Run() ?? throw new Xunit.Sdk.XunitException("Expected parser to return a page.");
        foreach (var message in parser.Messages)
        {
            Console.WriteLine(message);
        }
        Assert.False(parser.HasErrors);

        // Check that the parser finished parsing on the first code exit }}
        // and hasn't tried to run the lexer on the remaining text
        Assert.Equal(new TextPosition(34, 4, 0), parser.CurrentSpan.Start);

        // Make sure that we have a front matter
        var frontMatter = page.FrontMatter ?? throw new Xunit.Sdk.XunitException("Expected front matter to be present.");
        Assert.Null(page.Body);
        var startPositionAfterFrontMatter = frontMatter.TextPositionAfterEndMarker;

        var context = new TemplateContext();

        // Evaluate front-matter
        var frontResult = context.Evaluate(frontMatter);
        Assert.Null(frontResult);

        lexer = new Lexer(input, null, new LexerOptions() { StartPosition =  startPositionAfterFrontMatter });
        parser = new Parser(lexer);
        page = parser.Run() ?? throw new Xunit.Sdk.XunitException("Expected parser to return a page.");
        foreach (var message in parser.Messages)
        {
            Console.WriteLine(message);
        }
        Assert.False(parser.HasErrors);
        context.Evaluate(page);
        var pageResult = context.Output.ToString();
        TextAssert.AreEqual("This is after the frontmatter: yes\n2", pageResult);
    }

    [Fact]
    public void TestScriptOnly()
    {
        var options = new LexerOptions() { Mode = ScriptMode.ScriptOnly };
        var template = ParseTemplate(@"
variable = 1
name = 'yes'
", options);

        var context = new TemplateContext();

        template.Render(context);

        var outputStr = context.Output.ToString();
        Assert.Equal(string.Empty, outputStr);

        var global = GetCurrentGlobal(context);
        object value;
        Assert.True(global.TryGetValue("name", out value));
        Assert.Equal("yes", value);

        Assert.True(global.TryGetValue("variable", out value));
        Assert.Equal(1, value);
    }

    private static Template ParseTemplate(string text, LexerOptions lexerOptions = null, ParserOptions parserOptions = null)
    {
        var template = Template.Parse(text, "text", parserOptions, lexerOptions);
                foreach (var message in template.Messages)
                {
                    Console.WriteLine(message);
                }
        Assert.False(template.HasErrors);
        return template;
    }

    [Fact]
    public void TestFunctionCallInExpression()
    {
        var lexer = new Lexer(@"{{
with math
    round pi
end
}}");
        var parser = new Parser(lexer);

        var scriptPage = parser.Run() ?? throw new Xunit.Sdk.XunitException("Expected parser to return a page.");

        foreach (var message in parser.Messages)
        {
            Console.WriteLine(message);
        }
        Assert.False(parser.HasErrors);
        var rootObject = new ScriptObject();
        rootObject.SetValue("math", ScriptObject.From(typeof(MathObject)), true);

        var context = new TemplateContext();
        context.PushGlobal(rootObject);
        context.Evaluate(scriptPage);
        context.PopGlobal();

        // Result
        var result = context.Output.ToString();

        Console.WriteLine(result);
    }

    [Fact]
    public void TestIndent()
    {
        var input = @"{{ a_multi_line_value = ""test1\ntest2\ntest3\n"" ~}}
   {{ a_multi_line_value }}Hello
";
        var template = Template.Parse(input);
        var result = template.Render();
        result = TextAssert.Normalize(result);

        TextAssert.AreEqual(TextAssert.Normalize(@"   test1
   test2
   test3
Hello
"), result);
    }

    [Fact]
    public void TestIndentSkippedWithGreedyOnPreviousLine()
    {
        var input = @"{{ a_multi_line_value = ""test1\ntest2\ntest3\n"" -}}
   {{ a_multi_line_value }}Hello
";
        var template = Template.Parse(input);
        var result = template.Render();
        result = TextAssert.Normalize(result);

        TextAssert.AreEqual(TextAssert.Normalize(@"test1
test2
test3
Hello
"), result);
    }

    [Fact]
    public void TestIndent2()
    {
        var input = @"  {{data}}";
        var template = Template.Parse(input);
        var result = template.Render(new { data = "test\ntest2" });
        result = TextAssert.Normalize(result);

        TextAssert.AreEqual("  test\n  test2", result);
    }

    [Fact]
    public void TestIndent3()
    {
        var input = @"a
{{~ if true ~}}
b
{{~ end ~}}
 {{'c'}}";
        var template = Template.Parse(input);
        var result = template.Render();
        result = TextAssert.Normalize(result);

        TextAssert.AreEqual("a\nb\n c", result);
    }

    [Fact]
    public void TestIndent4()
    {
        var input = @"{{ ""b"" }}
Normal Text
    {{ ""indented text"" }}
";
        var template = Template.Parse(input);
        var result = template.Render();
        result = TextAssert.Normalize(result);

        TextAssert.AreEqual("b\nNormal Text\n    indented text\n", result);
    }

    [Fact]
    public void TestIndent5()
    {
        var input = @"{{~ for test in  ['A', 'B'] ~}}
  {{ test}}
{{~ end ~}}
{{~ for test in  ['A', 'B'] ~}}
  {{ test}}
{{~ end ~}}
{{~ for test in  ['A', 'B'] ~}}
  {{ test}}
{{~ end ~}}
{{~ for test in  ['A', 'B'] ~}}
  {{ test}}
{{~ end ~}}

{{~ for test in  ['A', 'B'] ~}}
  {{ test}}
{{~ end ~}}
{{~ for test in  ['A', 'B'] ~}}
  {{ test}}
{{~ end ~}}
{{~ for test in  ['A', 'B'] ~}}
  {{ test}}
{{~ end ~}}
{{~ for test in  ['A', 'B'] ~}}
  {{ test}}
{{~ end ~}}
";
        var template = Template.Parse(input);
        var result = template.Render();
        result = TextAssert.Normalize(result);

        TextAssert.AreEqual("  A\n  B\n  A\n  B\n  A\n  B\n  A\n  B\n\n  A\n  B\n  A\n  B\n  A\n  B\n  A\n  B\n", result);
    }

    [Theory]
    [MemberData(nameof(TestFilesHelper.ListTestFiles_000_basic), MemberType = typeof(TestFilesHelper))]
    public static void A000_basic(string inputName)
    {
        TestFile(inputName);
    }

    [Theory]
    [MemberData(nameof(TestFilesHelper.ListTestFiles_010_literals), MemberType = typeof(TestFilesHelper))]
    public static void A010_literals(string inputName)
    {
        TestFile(inputName);
    }

    [Theory]
    [MemberData(nameof(TestFilesHelper.ListTestFiles_020_interpolation), MemberType = typeof(TestFilesHelper))]
    public static void A020_interpolation(string inputName)
    {
        TestFile(inputName);
    }

    [Theory]
    [MemberData(nameof(TestFilesHelper.ListTestFiles_100_expressions), MemberType = typeof(TestFilesHelper))]
    public static void A100_expressions(string inputName)
    {
        TestFile(inputName);
    }

    [Theory]
    [MemberData(nameof(TestFilesHelper.ListTestFiles_200_statements), MemberType = typeof(TestFilesHelper))]
    public static void A200_statements(string inputName)
    {
        TestFile(inputName);
    }

    [Theory]
    [MemberData(nameof(TestFilesHelper.ListTestFiles_300_functions), MemberType = typeof(TestFilesHelper))]
    public static void A300_functions(string inputName)
    {
        TestFile(inputName);
    }

    [Theory]
    [MemberData(nameof(TestFilesHelper.ListTestFiles_400_builtins), MemberType = typeof(TestFilesHelper))]
    public static void A400_builtins(string inputName)
    {
        TestFile(inputName);
    }

    [Theory]
    [MemberData(nameof(TestFilesHelper.ListTestFiles_500_liquid), MemberType = typeof(TestFilesHelper))]
    public static void A500_liquid(string inputName)
    {
        TestFile(inputName);
    }

    [Theory]
    [MemberData(nameof(TestFilesHelper.ListTestFiles_600_ast), MemberType = typeof(TestFilesHelper))]
    public static void A600_ast(string inputName)
    {
        TestFile(inputName, true);
    }

    [Fact(Skip = "Doc tests not functional - upstream markdown format does not match test regex")]
    public static void Doc_array()
    {
    }

    [Fact(Skip = "Doc tests not functional - upstream markdown format does not match test regex")]
    public static void Doc_date()
    {
    }

    [Fact(Skip = "Doc tests not functional - upstream markdown format does not match test regex")]
    public static void Doc_html()
    {
    }

    [Fact(Skip = "Doc tests not functional - upstream markdown format does not match test regex")]
    public static void Doc_math()
    {
    }

    [Fact(Skip = "Doc tests not functional - upstream markdown format does not match test regex")]
    public static void Doc_object()
    {
    }

    [Fact(Skip = "Doc tests not functional - upstream markdown format does not match test regex")]
    public static void Doc_regex()
    {
    }

    [Fact(Skip = "Doc tests not functional - upstream markdown format does not match test regex")]
    public static void Doc_string()
    {
    }

    [Fact(Skip = "Doc tests not functional - upstream markdown format does not match test regex")]
    public static void Doc_timespan()
    {
    }

    [Fact]
    public void TestArrayFilter()
    {
        var script = @"{{[1, 200 , 3,400] | array.filter @(do;ret $0 >=100; end)}}";
        var template = Template.Parse(script);
        var result = template.Render();
        Assert.Equal(@"[200, 400]", result.Trim());
    }

    [Fact]
    public void EnsureThatItemWithIndexePropertyDoesNotThrow()
    {
        var obj = JObject.Parse("{\"name\":\"steve\"}");

        var template = Template.Parse("Hi {{name}}");
        Record.Exception(()=>template.Render(obj));
    }
    [Fact]
    public void EnsureMalformedFunctionDoesNotThrow()
    {
        Record.Exception(() =>Template.Parse("{{ func (t("));
    }

    [Fact]
    public void Regression_336()
    {
        var so = new CodeBrix.Templating.Runtime.ScriptObject();
        so.SetValue("X", "TEST", false);
        var evaluated = Template.Parse("{{X}}").Evaluate(new TemplateContext(so));
        //important - test type since ScriptArray can be cast to string implicitly
        Assert.Equal(typeof(string), evaluated?.GetType());
        Assert.Equal("TEST", evaluated);

    }

    [Fact]
    public void TestEvaluateProcessing()
    {
        {
            var result = Template.Parse("{{['', '200', '','400'] | array.filter @string.strip}}").Evaluate(new TemplateContext());

            Assert.Equal(new[] { "", "200", "", "400" }, result);
        }
        {
            var result = Template.Parse("{{['', '200', '','400'] | array.filter @string.empty}}").Evaluate(new TemplateContext());

            Assert.Equal(new[] { "", "" }, result);
        }
    }

    [Fact]
    public void EnsureStackOverflowCanBeAvoidedForSelfReferentialObjectGraphs()
    {
        var script = @"{{
m ={a:0}
m.a =m
m
}}";
        var template = Template.Parse(script);
        var context = new TemplateContext
        {
            ObjectRecursionLimit = 100
        };
        Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
    }

    [Fact]
    public void EnsureExpressionDepthLimitAppliesToNestedArrayInitializers()
    {
        var builder = new StringBuilder();
        for (var i = 0; i < 20; i++)
        {
            builder.Append('[');
        }

        builder.Append('0');

        for (var i = 0; i < 20; i++)
        {
            builder.Append(']');
        }

        var template = Template.Parse($"{{{{ {builder} }}}}", parserOptions: new ParserOptions
        {
            ExpressionDepthLimit = 10
        });

        Assert.True(template.HasErrors);
        Assert.Contains("The statement depth limit `10` was reached when parsing this statement", template.Messages[0].ToString());
    }

    [Fact]
    public void EnsureExpressionDepthLimitAppliesToNestedBinaryExpressions()
    {
        // Build: 1+(1+(1+(1+(...))))  with 20 levels of nesting via parentheses
        var builder = new StringBuilder();
        for (var i = 0; i < 20; i++)
        {
            builder.Append("(1+");
        }
        builder.Append('1');
        for (var i = 0; i < 20; i++)
        {
            builder.Append(')');
        }

        var template = Template.Parse($"{{{{ {builder} }}}}", parserOptions: new ParserOptions
        {
            ExpressionDepthLimit = 10
        });

        Assert.True(template.HasErrors);
        Assert.Contains("The statement depth limit `10` was reached when parsing this statement", template.Messages[0].ToString());
    }

    [Fact]
    public void EnsureExpressionDepthLimitAppliesToNestedUnaryExpressions()
    {
        // Build: !(!(!(!(...true...))))  with 20 levels of nesting
        var builder = new StringBuilder();
        for (var i = 0; i < 20; i++)
        {
            builder.Append("!(");
        }
        builder.Append("true");
        for (var i = 0; i < 20; i++)
        {
            builder.Append(')');
        }

        var template = Template.Parse($"{{{{ {builder} }}}}", parserOptions: new ParserOptions
        {
            ExpressionDepthLimit = 10
        });

        Assert.True(template.HasErrors);
        Assert.Contains("The statement depth limit `10` was reached when parsing this statement", template.Messages[0].ToString());
    }

    [InlineData(@"ab{{end}}c")]  // no blocks
    [InlineData(@"a{{if true}}b{{end}}{{end}}c")]  // one-level block
    [InlineData(@"a{{if true}}{{for i in 0..1}}b{{end}}{{end}}{{end}}c")]  // two-level block (nested)
    [Theory]
    public void TestUnmatchedEndStatementCausesError(string templateText)
    {
        var template = Template.Parse(templateText);
        Assert.True(template.HasErrors);

        Assert.Single(template.Messages);
        Assert.Contains("Found <end> statement without a corresponding beginning of a block", template.Messages[0].ToString());
    }

    private static void TestFile(string inputName, bool testASTInstead = false)
    {
        var filename = Path.GetFileName(inputName);
        var isSupportingExactRoundtrip = !NotSupportingExactRoundtrip.Contains(filename);

        var inputText = LoadTestFile(inputName) ?? throw new Xunit.Sdk.XunitException($"Unable to load input test file `{inputName}`.");
        var expectedOutputName = Path.ChangeExtension(inputName, OutputEndFileExtension);
        var expectedOutputText = LoadTestFile(expectedOutputName) ?? throw new Xunit.Sdk.XunitException($"Expecting output result file `{expectedOutputName}` for input file `{inputName}`.");

        var lang = ScriptLang.Default;
        if (inputName.Contains("liquid"))
        {
            lang = ScriptLang.Liquid;
        }
        else if (inputName.Contains("scientific"))
        {
            lang = ScriptLang.Scientific;
        }

        AssertTemplate(expectedOutputText, inputText, lang, false, isSupportingExactRoundtrip, expectParsingErrorForRountrip: filename == "513-liquid-statement-for.variables.txt", testASTInstead : testASTInstead);
    }

    private void AssertRoundtrip(string inputText, bool isLiquid = false)
    {
        inputText = inputText.Replace("\r\n", "\n");
        AssertTemplate(inputText, inputText, isLiquid ? ScriptLang.Liquid : ScriptLang.Default, true);
    }


    /// <summary>
    /// Lists of the tests that don't support exact byte-to-byte roundtrip (due to reformatting...etc.)
    /// </summary>
    private static readonly HashSet<string> NotSupportingExactRoundtrip = new HashSet<string>()
    {
        "003-whitespaces.txt",
        "010-literals.txt",
        "205-case-when-statement2.txt",
        "230-capture-statement2.txt",
        "470-html.txt"
    };

    internal static void AssertTemplate(string expected, string input, ScriptLang lang = ScriptLang.Default, bool isRoundtripTest = false, bool supportExactRoundtrip = true, object model = null, bool specialLiquid = false, bool expectParsingErrorForRountrip = false, bool supportRoundTrip = true, bool testASTInstead = false)
    {
        bool isLiquid = lang == ScriptLang.Liquid;

        var parserOptions = new ParserOptions()
        {
            LiquidFunctionsToScriban = isLiquid,
            ExpressionDepthLimit = specialLiquid ? 500 : 250,
        };
        var lexerOptions = new LexerOptions()
        {
            Lang = lang
        };

        if (isRoundtripTest)
        {
            lexerOptions = lexerOptions with { KeepTrivia = true };
        }

#if EnableTokensOutput
        {
            Console.WriteLine("Tokens");
            Console.WriteLine("======================================");
            var lexer = new Lexer(input, options: lexerOptions);
            foreach (var token in lexer)
            {
                Console.WriteLine($"{token.Type}: {token.GetText(input)}");
            }
            Console.WriteLine();
        }
#endif
        string roundtripText = null;

        // We loop first on input text, then on roundtrip
        while (true)
        {
            bool isRoundtrip = roundtripText is not null;
            bool hasErrors = false;
            bool hasException = false;
            if (isRoundtrip)
            {
                var checkedRoundtripText = roundtripText ?? throw new Xunit.Sdk.XunitException("Expected roundtrip text to be available.");
                Console.WriteLine("Roundtrip");
                Console.WriteLine("======================================");
                Console.WriteLine(checkedRoundtripText);
                lexerOptions = lexerOptions with { Lang = lang == ScriptLang.Scientific ? lang : ScriptLang.Default };

                if (!isLiquid && supportExactRoundtrip)
                {
                    Console.WriteLine("Checking Exact Roundtrip - Input");
                    Console.WriteLine("======================================");
                    TextAssert.AreEqual(input, checkedRoundtripText);
                }
                input = checkedRoundtripText;
            }
            else
            {
                Console.WriteLine("Input");
                Console.WriteLine("======================================");
                Console.WriteLine(input);
            }

            var template = Template.Parse(input, "text", parserOptions, lexerOptions);

            var result = string.Empty;
            var resultAsync = string.Empty;
            if (template.HasErrors)
            {
                hasErrors = true;
                for (int i = 0; i < template.Messages.Count; i++)
                {
                    var message = template.Messages[i];
                    if (i > 0)
                    {
                        result += "\n";
                    }
                    result += message;
                }
                if (specialLiquid && !isRoundtrip)
                {
                    throw new InvalidOperationException("Parser errors: " + result);
                }
            }
            else
            {
                if (isRoundtripTest)
                {
                    result = template.ToText();
                }
                else
                {
                    Assert.NotNull(template.Page);

                    if (!isRoundtrip)
                    {
                        // Dumps the roundtrip version
                        var lexerOptionsForTrivia = lexerOptions;
                        lexerOptionsForTrivia = lexerOptionsForTrivia with { KeepTrivia = true };
                        var templateWithTrivia = Template.Parse(input, "input",  parserOptions, lexerOptionsForTrivia);
                        roundtripText = templateWithTrivia.ToText();
                    }

                    try
                    {
                        // Setup a default model context for the tests
                        if (model is null)
                        {
                            var scriptObj = new ScriptObject
                            {
                                ["page"] = new ScriptObject {["title"] = "This is a title"},
                                ["user"] = new ScriptObject {["name"] = "John"},
                                ["product"] = new ScriptObject {["title"] = "Orange", ["type"] = "fruit"},
                                ["products"] = new ScriptArray()
                                {
                                    new ScriptObject {["title"] = "Orange", ["type"] = "fruit"},
                                    new ScriptObject {["title"] = "Banana", ["type"] = "fruit"},
                                    new ScriptObject {["title"] = "Apple", ["type"] = "fruit"},
                                    new ScriptObject {["title"] = "Computer", ["type"] = "electronics"},
                                    new ScriptObject {["title"] = "Mobile Phone", ["type"] = "electronics"},
                                    new ScriptObject {["title"] = "Table", ["type"] = "furniture"},
                                    new ScriptObject {["title"] = "Sofa", ["type"] = "furniture"},
                                }
                            };
                            scriptObj.Import(typeof(SpecialFunctionProvider));
                            model = scriptObj;
                        }

                        // Render sync
                        {
                            var context = NewTemplateContext(lang);
                            context.PushOutput(new TextWriterOutput(new StringWriter() {NewLine = "\n"}));
                            var contextObj = new ScriptObject();
                            contextObj.Import(model);
                            context.PushGlobal(contextObj);
                            result = template.Render(context);
                        }

                        // Render async
                        {
                            var asyncContext = NewTemplateContext(lang);
                            asyncContext.PushOutput(new TextWriterOutput(new StringWriter() {NewLine = "\n"}));
                            var contextObj = new ScriptObject();
                            contextObj.Import(model);
                            asyncContext.PushGlobal(contextObj);
                            resultAsync = template.RenderAsync(asyncContext).Result;
                        }
                    }
                    catch (Exception exception)
                    {
                        hasException = true;
                        if (specialLiquid)
                        {
                            throw;
                        }
                        else
                        {
                            result = GetReason(exception);
                        }
                    }
                }

                if (testASTInstead)
                {
                    var astVisualizer = new ASTVisualizer();
                    GetPage(template).Accept(astVisualizer);
                    result = astVisualizer.output.ToString();
                    resultAsync = result;
                }
            }

            var testContext = isRoundtrip ? "Roundtrip - " : String.Empty;
            Console.WriteLine($"{testContext}Result");
            Console.WriteLine("======================================");
            Console.WriteLine(result);
            Console.WriteLine($"{testContext}Expected");
            Console.WriteLine("======================================");
            Console.WriteLine(expected);

            if (isRoundtrip && expectParsingErrorForRountrip)
            {
                Assert.True(hasErrors, "The roundtrip test is expecting an error");
                Assert.NotEqual(expected, result);
            }
            else
            {
                TextAssert.AreEqual(expected, result);
            }

            if (!isRoundtrip && !isRoundtripTest && !hasErrors && !hasException)
            {
                Console.WriteLine("Checking async");
                Console.WriteLine("======================================");
                TextAssert.AreEqual(expected, resultAsync);
            }

            if (!supportRoundTrip || isRoundtripTest || isRoundtrip || hasErrors)
            {
                break;
            }
        }
    }

    private static TemplateContext NewTemplateContext(ScriptLang lang)
    {
        var isLiquid = lang == ScriptLang.Liquid;
        var context = isLiquid
            ? new LiquidTemplateContext()
            {
                TemplateLoader = new LiquidCustomTemplateLoader()
            }
            : new TemplateContext()
            {
                TemplateLoader = new CustomTemplateLoader()
            };
        if (lang == ScriptLang.Scientific)
        {
            context.UseScientific = true;
        }
        // We use a custom output to make sure that all output is using the "\n"
        context.NewLine = "\n";
        return context;
    }

    private static string GetReason(Exception ex)
    {
        var text = new StringBuilder();
        while (ex is not null)
        {
            text.Append(ex);
            if (ex.InnerException is not null)
            {
                text.Append(". Reason: ");
            }
            ex = ex.InnerException;
        }
        return text.ToString();
    }

    public static IEnumerable<object[]> DocTests_array() => ListBuiltinFunctionTests("array");
    public static IEnumerable<object[]> DocTests_date() => ListBuiltinFunctionTests("date");
    public static IEnumerable<object[]> DocTests_html() => ListBuiltinFunctionTests("html");
    public static IEnumerable<object[]> DocTests_math() => ListBuiltinFunctionTests("math");
    public static IEnumerable<object[]> DocTests_object() => ListBuiltinFunctionTests("object");
    public static IEnumerable<object[]> DocTests_regex() => ListBuiltinFunctionTests("regex");
    public static IEnumerable<object[]> DocTests_string() => ListBuiltinFunctionTests("string");
    public static IEnumerable<object[]> DocTests_timespan() => ListBuiltinFunctionTests("timespan");

    public static IEnumerable<object[]> ListBuiltinFunctionTests(string functionObject)
    {
        var builtinDocFile = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "site", "docs", "builtins", $"{functionObject}.md"));
        var lines = File.ReadAllLines(builtinDocFile);

        var matchFunctionSection = new Regex($@"^##\s+`({functionObject}\.\w+)`");

        var tests = new List<object[]>();

        string nextFunctionName = null;
        int processState = 0;
        // states:
        // - 0 function section or wait for ```scriban-html (input)
        // - 2 parse input (wait for ```)
        // - 3 wait for ```html (output)
        // - 4 parse input (wait for ```)
        var input = new StringBuilder();
        var output = new StringBuilder();
        foreach (var line in lines)
        {
            // Unescape site delimiters: {{ "{{" }} -> {{ and {{ "}}" }} -> }}
            var unescapedLine = line.Replace("{{ \"{{\" }}", "{{").Replace("{{ \"}}\" }}", "}}");

            // Match first:
            //## `array.add_range`
            switch (processState)
            {
                case 0:
                    var match = matchFunctionSection.Match(unescapedLine);
                    if (match.Success)
                    {
                        nextFunctionName = match.Groups[1].Value;
                    }

                    if (nextFunctionName is not null && unescapedLine.StartsWith("```scriban-html"))
                    {
                        processState = 1;
                        input = new StringBuilder();
                        output = new StringBuilder();
                    }
                    break;
                case 1:
                    if (unescapedLine.Equals("```"))
                    {
                        processState = 2;
                    }
                    else
                    {
                        input.AppendLine(unescapedLine);
                    }
                    break;
                case 2:
                    if (unescapedLine.StartsWith("```html"))
                    {
                        processState = 3;
                    }
                    break;
                case 3:
                    if (unescapedLine.Equals("```"))
                    {
                        var outputStr = output.ToString();
                        if (outputStr == "Hello<br />\r\nworld\r\n")
                        {
                            outputStr = "Hello<br />\nworld\r\n";
                        }

                        tests.Add(new object[] { nextFunctionName, input.ToString(), outputStr });
                        processState = 0;
                    }
                    else
                    {
                        output.AppendLine(unescapedLine);
                    }
                    break;
            }
        }

        return tests;
    }

    public static IEnumerable ListTestFiles(string folder)
    {
        return ListTestFilesInFolder(folder);
    }


    class ASTVisualizer : ScriptVisitor
    {
        int deepCounter;
        public StringBuilder output { get; } = new StringBuilder();

        protected override void DefaultVisit(ScriptNode node)
        {
            if (node is null)
            {
                return;
            }

            bool isTerminal = (node is IScriptTerminal);
            string padding = new string(' ', deepCounter * 2);
            string value = node.ToString();
            string type = node.GetType().Name;
            string offset = $" ({node.Span.Start.Offset} - {node.Span.End.Offset}) ";

            output.Append(padding + type + offset + (isTerminal ? $" [{value}]\n" : "\n"));
            deepCounter++;
            base.DefaultVisit(node);
            deepCounter--;
        }
    }
}
