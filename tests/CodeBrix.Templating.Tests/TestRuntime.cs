// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Runtime;
using CodeBrix.Templating.Syntax;

namespace CodeBrix.Templating.Tests; //was previously: Scriban.Tests;

delegate string Args(object[] args);
delegate string OptionalTextDelegate(string text = "default");

public class TestRuntime
{
    private static ScriptPage GetPage(Template template)
    {
        return template.Page ?? throw new Xunit.Sdk.XunitException("Expected parsed template page to be available.");
    }

    private static IScriptObject GetCurrentGlobal(TemplateContext context)
    {
        return context.CurrentGlobal ?? throw new Xunit.Sdk.XunitException("Expected a current global script object.");
    }

    private static TException AssertThrows<TException>(System.Action code) where TException : Exception
    {
        return Assert.Throws<TException>(code) ?? throw new Xunit.Sdk.XunitException($"Expected {typeof(TException).Name}.");
    }

    [Fact]
    public void TestFunctionPointerWithPath()
    {
        var script = """
                     {{ ["", "200",  "400"] | array.any @string.contains '20' }}
                     """;
        var template = CodeBrix.Templating.Template.Parse(script);
        var result = template.Render();
        Assert.Equal("true", result);

        script = """
                 {{ ["", "200",  "400"] | array.any @string.contains '50' }}
                 """;
        template = CodeBrix.Templating.Template.Parse(script);
        result = template.Render();
        Assert.Equal("false", result);
    }

    [Fact]
    public void TestPipeAndNamedArguments()
    {
        var script = """
                     {{func get_values; ret [{name:'A'},{name:'B'},{name:'C'}]; end;}}4) Breaks: {{ get_values '1' two:'2' three: '3' | array.map 'name' }}
                     """;
        var template = CodeBrix.Templating.Template.Parse(script);
        var result = template.Render();

        Assert.Equal("4) Breaks: [\"A\", \"B\", \"C\"]", result);
    }

    [Fact]
    public void TestNullCoallescingWithStringInterpolation()
    {
        var script = """
                     {{ "hello" ?? $"{" "}world" }}
                     """;

        //var lexer = new Lexer(script);
        //foreach (var token in lexer)
        //{
        //    Console.WriteLine(token);
        //}
        var template = CodeBrix.Templating.Template.Parse(script);
        var result = template.Render();

        GetPage(template).PrintTo(new ScriptPrinter(new TextWriterOutput(Console.Out)));

        Assert.Equal("hello", result);
    }

    [Fact]
    public void String_And_Null_Concatenated_Should_Not_Null()
    {
        var context = new TemplateContext()
        {
        };
        var tmplExample1 = Template.Parse("{{'my name is ' + null}}");
        var tmplExample2 = Template.Parse("{{$'my name is {null}'}}");

        var result = tmplExample1.Render(context);
        Assert.Equal("my name is ", result);

        result = tmplExample2.Render(context);
        Assert.Equal("my name is ", result);
    }

    [Fact]
    public void LimitToStringShouldApplyToCumulativeRenderOutput()
    {
        var context = new TemplateContext
        {
            LimitToString = 5
        };
        var template = Template.Parse("{{ 'abc' }}{{ 'def' }}");

        var result = template.Render(context);

        Assert.Equal("abcde...", result);
    }

    [Fact]
    public void ResetShouldClearCumulativeRenderOutputTracking()
    {
        var context = new TemplateContext
        {
            LimitToString = 5
        };
        var largeTemplate = Template.Parse("{{ 'abc' }}{{ 'def' }}");
        var smallTemplate = Template.Parse("{{ 'xy' }}");

        Assert.Equal("abcde...", largeTemplate.Render(context));

        context.Reset();

        Assert.Equal("xy", smallTemplate.Render(context));
    }

    [Fact]
    public void StringMultiplicationShouldRespectLimitToString()
    {
        var context = new TemplateContext
        {
            LimitToString = 5
        };
        var template = Template.Parse("{{ 'ab' * 3 }}");

        var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));

        Assert.Contains("LimitToString", exception.Message);
    }

    [Fact]
    public void BigIntegerShiftShouldRejectOversizedAmounts()
    {
        var template = Template.Parse("{{ 1 << 1048577 }}");

        var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render());

        Assert.Contains("Shift amount", exception.Message);
    }

    [Fact]
    public void RangeExpressionShouldRespectLoopLimit()
    {
        var context = new TemplateContext
        {
            LoopLimit = 5
        };
        var template = Template.Parse("1..6", lexerOptions: new LexerOptions { Mode = ScriptMode.ScriptOnly });

        var exception = Assert.Throws<ScriptRuntimeException>(() => template.Evaluate(context));

        Assert.Contains("LoopLimit", exception.Message);
    }

    [Fact]
    public void ArrayJoinShouldRespectLimitToString()
    {
        var context = new TemplateContext
        {
            LimitToString = 3
        };
        var template = Template.Parse("{{ ['ab', 'cd'] | array.join '' }}");

        var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));

        Assert.Contains("LimitToString", exception.Message);
    }

    [Fact]
    public void ArraySizeShouldRespectLoopLimitForInternalIteration()
    {
        var context = new TemplateContext
        {
            LoopLimit = 5
        };
        context.PushGlobal(new ScriptObject
        {
            { "numbers", Enumerable.Range(0, 10) }
        });

        var template = Template.Parse("{{ numbers | array.size }}");

        var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));

        Assert.Contains("iteration limit `5`", exception.Message);
    }

    [Fact]
    public void ArrayJoinShouldRespectLoopLimitForInternalIteration()
    {
        var context = new TemplateContext
        {
            LoopLimit = 5
        };
        context.PushGlobal(new ScriptObject
        {
            { "numbers", Enumerable.Range(0, 10) }
        });

        var template = Template.Parse("{{ numbers | array.join '' }}");

        var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));

        Assert.Contains("iteration limit `5`", exception.Message);
    }

    [Fact]
    public void ArrayOffsetShouldRespectLoopLimitForInternalIteration()
    {
        var context = new TemplateContext
        {
            LoopLimit = 5
        };
        context.PushGlobal(new ScriptObject
        {
            { "numbers", Enumerable.Range(0, 10) }
        });

        var template = Template.Parse("{{ numbers | array.offset 8 | array.first }}");

        var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));

        Assert.Contains("iteration limit `5`", exception.Message);
    }

    [Fact]
    public void InternalArrayEnumerationShouldCheckCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var context = new TemplateContext
        {
            CancellationToken = cancellation.Token
        };
        context.PushGlobal(new ScriptObject
        {
            { "numbers", Enumerable.Range(0, 10) }
        });

        var template = Template.Parse("{{ numbers | array.size }}");

        Assert.Throws<ScriptAbortException>(() => template.Render(context));
    }

    [Fact]
    public void TestAssignValToDictionary()
    {
        var dict = new Dictionary<string, string>();
        dict["name"] = "bob";
        var model = new ScriptObject();
        model.Add("dict", dict);
        var context = new TemplateContext();
        context.PushGlobal(model);

        var input = "{{dict.location = \"home\"}}";
        var template = Template.Parse(input);
        var results = template.Render(context);

        input = "{{dict[\"location\"] = \"home\"}}";
        template = Template.Parse(input);
        results = template.Render(context);
        // Assert.Equal("", results);
    }

    [Fact]
    public void TestScriptObjectAsDictionary()
    {
        var model = (IDictionary)(new ScriptObject());
        model.Add("name", "John");
        model.Add("age", 20);
        Assert.Equal("John", model["name"]);
        Assert.Equal(20, model["age"]);
    }

    [Fact]
    public void TestLazy()
    {
        var input = @"{{ value }}";
        var template = Template.Parse(input);
        var result = template.Render(new { value = new ScriptLazy<int>(() => 1)});
        Assert.Equal("1", result);
    }

    [Fact]
    public void TestEval()
    {
        var input = @"{{ x = object.eval '1 + 1' }}";
        var template = Template.Parse(input);
        var context = new TemplateContext();
        var result = template.Render(context);
        Assert.Equal("", result);
        Assert.Equal(2, ((ScriptObject)GetCurrentGlobal(context))["x"]);

        input = @"{{ x = object.eval '+' }}";
        template = Template.Parse(input);
        context = new TemplateContext();
        Assert.Throws<ScriptRuntimeException>( () => template.Render(context));
    }

    [Fact]
    public void TestEnumerator()
    {
        var input = @"{{
  queue.add 'a'
  for x in queue.flush
    x
    if x == 'a'; queue.add 'b'; end
  end
}}";
        var template = Template.Parse(input);

        var test = template.Render(new { queue = new QueueBuiltin() });
        Assert.Equal("ab", test);
    }

    class QueueBuiltin : ScriptObject
    {
        static Queue<string> queue = new();

        public static void Add(string x) => queue.Enqueue(x);

        public static IEnumerable<string> Flush()
        {
            while (queue.TryDequeue(out var x))
                yield return x;
        }
    }


    [Fact]
    public void TestDateParse()
    {
        Template template = Template.Parse(
            @"{{date.format='%FT%T.%N%Z'}}{{ date.parse '2018~06~17~13~59~+08:00' '%Y~%m~%d~%H~%M~%Z' }}");
        var result = template.Render();
        Console.WriteLine(result);
    }


    [Fact]
    public void TestLoop()
    {

        var template = Template.Parse(@"{{
my_function(x) = x * i
result = 0
for i in [1,2,3,4]
    result = result + (my_function 10)
end
}}Result: {{ result }}
");
        var result = template.Render();




    }





    [Fact]
    public void TestPars()
    {
        string Dump(params object[] args)
        {
            return "hello";
        }

        ScriptObject model = new ScriptObject();
        ScriptObject debug = new ScriptObject();
        Args dump = Dump;

        debug.Import("dump", dump);
        model["debug"] = debug;

        var input = "{{debug.dump(10, \"hello\", [0, 1, 2])}}";
        var template = Template.Parse(input);
        var result = template.Render(model);

        Assert.Equal("hello", result);
    }

    [Fact]
    public void TestUlong()
    {
        var input = @"{{if value > 0; 1; else; 2; end;}}";

        var template = Template.Parse(input);
        var result = template.Render(new { value = (ulong)1 });
        Assert.Equal("1", result);
    }

    [Fact]
    public void TestUint()
    {
        var input = @"{{ 1 + value }}";

        var template = Template.Parse(input);
        var result = template.Render(new { value = uint.MaxValue });
        Assert.Equal("4294967296", result);
    }

    [Fact]
    public void TestDictionaryInt()
    {
        int MyInt = 1;
        Dictionary<int, string> MyDict = new();
        MyDict.Add(MyInt, "hello");

        string templateTxt = "{{ MyDict[MyInt] }}";

        Template template = Template.Parse(templateTxt);
        var result = template.Render(new { MyDict, MyInt }, member => member.Name);

        Assert.Equal("hello", result);
    }

    [Fact]
    public void TesterFilterEvaluation()
    {
        var result = Template.Parse("{{['', '200', '','400'] | array.filter @string.empty}}").Evaluate(new TemplateContext());
        Assert.IsAssignableFrom<ScriptRange>(result);
        var array = (ScriptRange)(result ?? throw new Xunit.Sdk.XunitException("Expected a ScriptRange."));
        Assert.Equal(2, array.Count);
        Assert.Equal("", array[0]);
        Assert.Equal("", array[1]);
    }

    [Fact]
    public void TestGetTypeName()
    {
        var context = new TemplateContext();

        Assert.Equal("bool", context.GetTypeName(true));
        Assert.Equal("bool", context.GetTypeName(false));
        Assert.Equal("byte", context.GetTypeName((byte)1));
        Assert.Equal("sbyte", context.GetTypeName((sbyte)1));
        Assert.Equal("ushort", context.GetTypeName((ushort)1));
        Assert.Equal("short", context.GetTypeName((short)1));
        Assert.Equal("uint", context.GetTypeName((uint)1));
        Assert.Equal("int", context.GetTypeName((int)1));
        Assert.Equal("ulong", context.GetTypeName((ulong)1));
        Assert.Equal("long", context.GetTypeName((long)1));
        Assert.Equal("float", context.GetTypeName((float)1.5f));
        Assert.Equal("double", context.GetTypeName((double)1.5));
        Assert.Equal("decimal", context.GetTypeName((decimal)1.5m));
        Assert.Equal("bigint", context.GetTypeName(new BigInteger(1)));
        Assert.Equal("string", context.GetTypeName("test"));
        Assert.Equal("range", context.GetTypeName(new ScriptRange()));
        Assert.Equal("array", context.GetTypeName(new ScriptArray()));
        Assert.Equal("array", context.GetTypeName(new ScriptArray<float>()));
        Assert.Equal("object", context.GetTypeName(new ScriptObject()));
        Assert.Equal("function", context.GetTypeName(DelegateCustomAction.Create(() => { })));
        Assert.Equal("enum", context.GetTypeName(ScriptLang.Default));
    }

    [Fact]
    public void TestLocalVariableReturned()
    {
        var text = @"{{
func hello1
 $hello = 'hello1'
 ret $hello
end

func hello2
 $hello = 'hello2'
 ret [ $hello ]
end

func hello3
 $hello = 'hello3'
 ret { hello: $hello }
end

func hello4
 ret { hello: 'hello4' }
end
~}}
hello1: {{ hello1 }}
hello2: {{ hello2 }}
hello3: {{ hello3 }}
hello4: {{ hello4 }}";

        var template = Template.Parse(text);
        var result = template.Render().Replace("\r\n", "\n");
        TextAssert.AreEqual(@"hello1: hello1
hello2: [""hello2""]
hello3: {hello: ""hello3""}
hello4: {hello: ""hello4""}".Replace("\r\n", "\n"), result);
    }

    [Fact]
    public void TestForEach()
    {
        var template = Template.Parse(@"{{ [1,2,3] | array.each do
ret $0 + 4
end
}}");
        var result = template.Render();
        Assert.Equal("[5, 6, 7]", result);
    }

    [Fact]
    public void TestRecursiveLocal()
    {
        var template = Template.Parse("{{ x = {}; with x; func $tester; if $0 == 0; ret; end; $0; $0 - 1 | $tester; end; export = @$tester; end; x.export 5; }}");
        var result = template.Render();
        Assert.Equal("54321", result);
    }

    [Fact]
    public void TestReflectionArguments()
    {
        var context = new TemplateContext();

        // Allocating a zero length object[] should return the same instance
        {
            var arg0_0 = context.GetOrCreateReflectionArguments(0);
            var arg0_1 = context.GetOrCreateReflectionArguments(0);
            Assert.Same(arg0_0, arg0_1);
            context.ReleaseReflectionArguments(arg0_0);
            context.ReleaseReflectionArguments(arg0_1);

            arg0_0 = context.GetOrCreateReflectionArguments(0);
            Assert.Same(arg0_0, arg0_1);
        }

        // Allocating a non-zero length object[] should return the != instance
        const int maxArgument = ScriptFunctionCall.MaximumParameterCount;
        {
            for (int length = 1; length <= maxArgument; length++)
            {
                var arg0_0 = context.GetOrCreateReflectionArguments(length);
                AssertAllNulls(arg0_0);
                var arg0_1 = context.GetOrCreateReflectionArguments(length);
                AssertAllNulls(arg0_1);

                Assert.NotSame(arg0_0, arg0_1);

                Array.Fill(arg0_0, (object)1);
                Array.Fill(arg0_1, (object)1);

                context.ReleaseReflectionArguments(arg0_0);
                context.ReleaseReflectionArguments(arg0_1);

                var arg1_0 = context.GetOrCreateReflectionArguments(length);
                AssertAllNulls(arg1_0);
                var arg1_1 = context.GetOrCreateReflectionArguments(length);
                AssertAllNulls(arg1_1);

                Assert.NotSame(arg1_0, arg1_1);

                Assert.Same(arg0_0, arg1_1);
                Assert.Same(arg0_1, arg1_0);

                context.ReleaseReflectionArguments(arg1_0);
                context.ReleaseReflectionArguments(arg1_1);
            }
        }

        {
            var arg0_0 = context.GetOrCreateReflectionArguments(maxArgument + 1);
            AssertAllNulls(arg0_0);
            Array.Fill(arg0_0, (object)1);
            context.ReleaseReflectionArguments(arg0_0);

            var arg0_1 = context.GetOrCreateReflectionArguments(maxArgument + 1);
            AssertAllNulls(arg0_1);
            Assert.NotSame(arg0_0, arg0_1);

            context.ReleaseReflectionArguments(arg0_1);
        }
    }

    private static void AssertAllNulls(object[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            Assert.Null(array[i]);
        }
    }

    public static class MyPipeFunctions
    {
        public static string A(TemplateContext context, object input, string currencyCode = null)
        {
            return input.ToString() + "A";
        }
        public static string B(object input)
        {
            return input.ToString() + "B";
        }
        public static string T(TemplateContext context, object input, params object[] variables)
        {
            return input + (variables.Length > 0 ? string.Join(",", variables.Select(s => s.ToString()).ToArray()) : string.Empty);
        }
    }

    public static class ImportedPipeOrderFunctions
    {
        public static string Format(DateTime input, string formatString)
        {
            return input.ToString(formatString, CultureInfo.InvariantCulture);
        }
    }

    [Fact]
    public void TestFunctionArrayEachAndFunctionCall()
    {
        var template = Template.Parse(@"{{
func f; ret $0 + 1; end
[1, 2, 3] | array.each @f
}} EOL");

        var result = template.Render();

        TextAssert.AreEqual("[2, 3, 4] EOL", result);
    }

    [Fact]

    public void TestLoopVariable()
    {
        var template = Template.Parse(@"
{{- for x in [1,2,3,4,5]
y = x
end -}}
x and y = {{ x }} and {{ y }}
{{~ for y in [6,7,8,9,0]
z = y
end ~}}
y and z = {{ y }} and {{ z -}}
");
        var expected = @"x and y =  and 5
y and z = 5 and 0";

        var tc = new TemplateContext();
        var result = template.Render(tc);
        TextAssert.AreEqual(expected, result);
    }


    [Fact]
    public void ReturnInTemplate()
    {

        var template = Template.Parse(@"{{ if x }}return{{ ret; end }}not return");

        var tc = new TemplateContext();
        GetCurrentGlobal(tc).SetValue("x", true, false);
        var result = template.Render(tc);
        Assert.Equal("return", result);
        GetCurrentGlobal(tc).SetValue("x", false, false);
        result = template.Render(tc);
        Assert.Equal("not return", result);
    }


    [Fact]
    public void TestFunctionCallWithNoReturn()
    {
        {
            var template = Template.Parse(@"
{{-
func g(x); x ; end;
1 + g(2)
-}}
");
            var tc = new TemplateContext() { ErrorForStatementFunctionAsExpression = true };
            Assert.Throws<ScriptRuntimeException>(() => template.Render(tc));
        }
        {
            var template = Template.Parse(@"
{{-
g(x) = x * 5;
1 + g(2)
-}}
");
            var tc = new TemplateContext() { ErrorForStatementFunctionAsExpression = true };
            var result = template.Render(tc);
            Assert.Equal("11", result);
        }
        {
            var template = Template.Parse(@"
{{-
func g(x); if x < 0; ret x + 1; else; ret x + 2; end; end;
1 + g(2) + g(-1)
-}}
");
            var tc = new TemplateContext() { ErrorForStatementFunctionAsExpression = true };
            var result = template.Render(tc);
            Assert.Equal("5", result);
        }
    }

    [Fact]
    public void TestExplicitFunctionCall()
    {
        {
            var template = Template.Parse(@"
{{-
g(x,y,z) = x + y * 2 + z * 10
1 + g(1,2,3) }} {{ g(5,6,7) * g(1,2,3) + 1
}}");
            var tc = new TemplateContext() { ErrorForStatementFunctionAsExpression = true };
            var result = template.Render(tc);
            Assert.Equal($"{1 + g(1, 2, 3)} {g(5, 6, 7) * g(1, 2, 3) + 1}", result);
        }

        int g(int x, int y, int z) => x + y * 2 + z * 10;
    }


    [Fact]
    public void TestStackOverflow()
    {
        {
            var template = Template.Parse(@"
{{-
f(x) = f(x - 1)
f(1)
-}}
");
            var tc = new TemplateContext();
            Assert.Throws<ScriptRuntimeException>(() => template.Render(tc));
        }
    }

    [Fact]
    public void TestFunctionWithTemplateContextAndObjectParams()
    {
        {
            var parsedTemplate = Template.ParseLiquid("{{ 'yoyo' | t }}");
            Assert.False(parsedTemplate.HasErrors);

            var scriptObject = new ScriptObject();
            scriptObject.Import(typeof(MyPipeFunctions));
            var context = new TemplateContext();
            context.PushGlobal(scriptObject);

            var result = parsedTemplate.Render(context);
            TextAssert.AreEqual("yoyo", result);
        }
        {
            var parsedTemplate = Template.ParseLiquid("{{ 'yoyo' | t 1 2 3}}");
            Assert.False(parsedTemplate.HasErrors);

            var scriptObject = new ScriptObject();
            scriptObject.Import(typeof(MyPipeFunctions));
            var context = new TemplateContext();
            context.PushGlobal(scriptObject);

            var result = parsedTemplate.Render(context);
            TextAssert.AreEqual("yoyo1,2,3", result);
        }
    }

    [Fact]
    public void TestImportedFunctionPipeValueUsesFirstArgument()
    {
        var template = Template.Parse(@"{{ date | format ""dd.MM"" }}");
        Assert.False(template.HasErrors);

        var context = new TemplateContext();
        var scriptObject = new ScriptObject();
        scriptObject.Import(typeof(ImportedPipeOrderFunctions));
        context.PushGlobal(scriptObject);
        context.PushGlobal(new ScriptObject
        {
            ["date"] = new DateTime(2024, 2, 3),
        });

        var result = template.Render(context);

        Assert.Equal("03.02", result);
    }

    [Fact]
    public void TestInvalidConvertToInt()
    {
        var template = Template.ParseLiquid("{{html>0}}");
        var ex = Assert.ThrowsAny<ScriptRuntimeException>(() => template.Render(new { x = 0 })) ?? throw new Xunit.Sdk.XunitException("Expected a ScriptRuntimeException.");
        Assert.Equal("<input>(1,7) : error : Unable to convert type `object` to int", ex.Message);
    }

    [Fact]
    public void TestPipeAndFunction()
    {
        var template = Template.Parse(@"
{{- func format_number
    ret $0 | math.format '0.00' | string.replace '.' ''
end -}}
{{ 123 | format_number -}}
");
        var result = template.Render();
        TextAssert.AreEqual("12300", result);
    }


    [Fact]
    public void TestPipeAndFunctionAndLoop()
    {
        var template = Template.Parse(@"
{{- func format_number
    ret $0 | math.format '0.00' | string.replace '.' ''
end -}}
{{
for $i in 1..3
    temp_variable = $i | format_number
end
-}}
{{ temp_variable -}}
");
        var result = template.Render();
        TextAssert.AreEqual("300", result);
    }

    [Fact]
    public void InvalidPipe()
    {
        var parsedTemplate = Template.ParseLiquid("{{ 22.00 | a | b | string.upcase }}");
        Assert.False(parsedTemplate.HasErrors);

        var scriptObject = new ScriptObject();
        scriptObject.Import(typeof(MyPipeFunctions));
        var context = new TemplateContext();
        context.PushGlobal(scriptObject);

        var result = parsedTemplate.Render(context);
        TextAssert.AreEqual("22AB", result);
    }

    [Fact]
    public async Task TestAsyncAwait()
    {
        var text = @"{{ wait_and_see }}";
        // Tax1: {{ 1 | match_tax }}
        var template = Template.Parse(text);
        var context = new TemplateContext();
        const int MinDelay = 100;
        GetCurrentGlobal(context).Import("wait_and_see", new Func<Task<string>>(async () =>
        {
            await Task.Delay(MinDelay + 10);
            return "yes";
        }));
        var clock = Stopwatch.StartNew();
        var result = await template.RenderAsync(context);
        clock.Stop();
        Console.WriteLine(clock.ElapsedMilliseconds);

        Assert.True(clock.ElapsedMilliseconds >= MinDelay);

        Assert.Equal("yes", result);
    }

    [Fact]
    public void CheckReturnInsideLoop()
    {
        var text = @"
{{-
func match_tax
    taxes = [5,6,7,8,9]
    for s in taxes
        if s == $0
            ret true
        end
    end
    ret false
end
-}}
Tax: {{ 7 | match_tax }}";
        // Tax1: {{ 1 | match_tax }}
        var template = Template.Parse(text);
        var context = new TemplateContext();
        var result = template.Render(context);

        //Task<string> x = Task.FromResult("yo");

        Assert.Equal("Tax: true", result);
    }

    [Fact]
    public void TestOperatorPrecedenceNegate()
    {
        var template = Template.Parse("{{ if -5.32 < 0 }}yo{{ end }}");
        Assert.False(template.HasErrors);
        var text = template.Render();
        Assert.Equal("yo", text);
    }


    [Fact]
    public void TestNullDateTime()
    {
        var template = Template.Parse("{{ null | date.to_string '%g' }}");
        var context = new TemplateContext();
        var result = template.Render(context);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void TestDecimal()
    {
        var template = Template.Parse("{{ if value > 0 }}yes{{end}}");
        decimal x = 5;
        var result = template.Render(new { value = x });
        Assert.Equal("yes", result);
    }

    [Fact]
    public void TestGuidDifferent()
    {
        var template = Template.Parse("{{ if guid1 != guid2 }}OK{{end}}");
        var result = template.Render(new { guid1 = Guid.NewGuid(), guid2 = Guid.NewGuid() });

        Assert.Equal("OK", result);
    }

    [Fact]
    public void TestCulture()
    {
        var number = 11232.123;
        var customCulture = new CultureInfo(CultureInfo.CurrentCulture.Name)
        {
            NumberFormat =
            {
                NumberDecimalSeparator = ",",
                NumberGroupSeparator = "."
            }
        };

        var numberAsStr = number.ToString(customCulture);

        var template = Template.Parse("{{ 11232.123 }}");
        var context = new TemplateContext();
        context.PushCulture(customCulture);
        var result = template.Render(context);
        context.PopCulture();

        Assert.Equal(numberAsStr, result);
    }


    [Fact]
    public void TestEvaluateScriptOnly()
    {
        {
            var lexerOptions = new LexerOptions() { Mode = ScriptMode.ScriptOnly };
            var template = Template.Parse("y = x + 1; y;", lexerOptions: lexerOptions);
            var result = template.Evaluate(new { x = 10 });
            Assert.Equal(11, result);
        }
        {
            var result = Template.Evaluate("y = x + 1; y;", new { x = 10 });
            Assert.Equal(11, result);
        }
    }

    [Fact]
    public void TestEvaluateDefault()
    {
        {
            var template = Template.Parse("{{y = x + 1; y;}} yoyo");
            var result = template.Evaluate(new { x = 10 });
            Assert.Equal(" yoyo", result);
        }
        {
            var template = Template.Parse("{{y = x + 1; y;}} yoyo {{y}}");
            var result = template.Evaluate(new { x = 10 });
            Assert.Equal(11, result);
        }
    }

    [Fact]
    public void TestReadOnly()
    {
        var template = Template.Parse("Test {{ a.b.c = 1 }}");

        var a = new ScriptObject()
        {
            {"b", new ScriptObject() {IsReadOnly = true}}
        };

        var context = new TemplateContext();
        context.PushGlobal(new ScriptObject()
        {
            {"a", a}
        });
        var exception = AssertThrows<ScriptRuntimeException>(() => context.Evaluate(GetPage(template)));
        var result = exception.ToString();
        Assert.True(result.Contains("The object is readonly"), $"The exception string `{result}` does not contain \"The object is readonly\"");
    }

    [Fact]
    public void TestDynamicVariable()
    {
        var context = new TemplateContext
        {
            TryGetVariable = (TemplateContext templateContext, SourceSpan span, ScriptVariable variable, out object value) =>
            {
                value = null;
                if (variable.Name == "myvar")
                {
                    value = "yes";
                    return true;
                }
                return false;
            }
        };

        {
            var template = Template.Parse("Test with a dynamic {{ myvar }}");
            context.Evaluate(GetPage(template));
            var result = context.Output.ToString();

            TextAssert.AreEqual("Test with a dynamic yes", result);
        }

        {
            // Test StrictVariables
            var template = Template.Parse("Test with a dynamic {{ myvar2 }}");
            context.StrictVariables = true;
            var exception = AssertThrows<ScriptRuntimeException>(() => context.Evaluate(GetPage(template)));
            var result = exception.ToString();
            var check = "The variable or function `myvar2` was not found";
            Assert.True(result.Contains(check), $"The exception string `{result}` does not contain the expected value");
        }
    }

    [Fact]
    public void TestDynamicMember()
    {
        var template = Template.Parse("Test with a dynamic {{ a.myvar }}");

        var globalObject = new ScriptObject();
        globalObject.SetValue("a", new ScriptObject(), true);

        var context = new TemplateContext
        {
            TryGetMember = (TemplateContext localContext, SourceSpan span, object target, string member, out object value) =>
            {
                value = null;
                if (member == "myvar")
                {
                    value = "yes";
                    return true;
                }
                return false;
            }
        };

        context.PushGlobal(globalObject);
        context.Evaluate(GetPage(template));
        var result = context.Output.ToString();

        TextAssert.AreEqual("Test with a dynamic yes", result);
    }

    [Fact]
    public void TestJson()
    {
        // issue: https://github.com/lunet-io/scriban/issues/11
        // fixed: https://github.com/lunet-io/scriban/issues/15

        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("Column1");
        dataTable.Columns.Add("Column2");

        DataRow dataRow = dataTable.NewRow();
        dataRow["Column1"] = "Hello";
        dataRow["Column2"] = "World";
        dataTable.Rows.Add(dataRow);

        dataRow = dataTable.NewRow();
        dataRow["Column1"] = "Bonjour";
        dataRow["Column2"] = "le monde";
        dataTable.Rows.Add(dataRow);

        string json = JsonConvert.SerializeObject(dataTable);
        Console.WriteLine("Json: " + json);

        var parsed = JsonConvert.DeserializeObject(json);
        Console.WriteLine("Parsed: " + parsed);

        string myTemplate = @"
[
  { {{~ for tbr in tb }}
    ""N"": {{tbr.Column1}},
    ""M"": {{tbr.Column2}}
    {{~ end ~}}
  },
]
{{tb}}
";

        // Parse the template
        var template = Template.Parse(myTemplate);

        // Render
        var context = new TemplateContext { MemberRenamer = member => member.Name };
        var scriptObject = new ScriptObject();
        scriptObject.Import(new { tb = parsed });
        context.PushGlobal(scriptObject);
        var result = template.Render(context);
        context.PopGlobal();

        var expected =
            @"
[
  {
    ""N"": Hello,
    ""M"": World

    ""N"": Bonjour,
    ""M"": le monde
  },
]
[[[Hello], [World]], [[Bonjour], [le monde]]]
";

        TextAssert.AreEqual(expected, result);
    }

    [Fact]
    public void TestScriptObjectImport()
    {
        {
            var obj = new ScriptObject();
            obj.Import(typeof(MyStaticObject));

            Assert.True(obj.ContainsKey("static_field_a"));
            Assert.Equal("ValueStaticFieldA", obj["static_field_a"]);
            Assert.True(obj.ContainsKey("static_field_b"));
            Assert.Equal("ValueStaticFieldB", obj["static_field_b"]);
            Assert.True(obj.ContainsKey("static_property_a"));
            Assert.Equal("ValueStaticPropertyA", obj["static_property_a"]);
            Assert.True(obj.ContainsKey("static_property_b"));
            Assert.Equal("ValueStaticPropertyB", obj["static_property_b"]);
            Assert.True(obj.ContainsKey("static_yoyo"));
            Assert.False(obj.ContainsKey("invalid"));
        }

        // Check new overrides
        {
            var obj = new ScriptObject();
            obj.Import(typeof(MyStaticObject2));

            Assert.True(obj.ContainsKey("static_yoyo"));
            var function = (IScriptCustomFunction)(obj["static_yoyo"] ?? throw new Xunit.Sdk.XunitException("Expected static_yoyo function."));
            var context = new TemplateContext();
            var result = function.Invoke(context, new ScriptFunctionCall(), new ScriptArray() { "a" }, null);
            Assert.Equal("yoyo2 a", result);
        }

        // Test MemberFilterDelegate
        {
            var obj = new ScriptObject();
            obj.Import(typeof(MyStaticObject), filter: member => member.Name.Contains("Property"));

            Assert.False(obj.ContainsKey("static_field_a"));
            Assert.False(obj.ContainsKey("static_field_b"));
            Assert.True(obj.ContainsKey("static_property_a"));
            Assert.Equal("ValueStaticPropertyA", obj["static_property_a"]);
            Assert.True(obj.ContainsKey("static_property_b"));
            Assert.Equal("ValueStaticPropertyB", obj["static_property_b"]);
            Assert.False(obj.ContainsKey("static_yoyo"));
            Assert.False(obj.ContainsKey("invalid"));
        }

        // Test MemberRenamerDelegate
        {
            var obj = new ScriptObject();
            obj.Import(typeof(MyStaticObject), renamer: member => member.Name);

            Assert.True(obj.ContainsKey(nameof(MyStaticObject.StaticFieldA)));
            Assert.True(obj.ContainsKey(nameof(MyStaticObject.StaticFieldB)));
            Assert.True(obj.ContainsKey(nameof(MyStaticObject.StaticPropertyA)));
            Assert.Equal("ValueStaticPropertyA", obj[nameof(MyStaticObject.StaticPropertyA)]);
            Assert.True(obj.ContainsKey(nameof(MyStaticObject.StaticPropertyB)));
            Assert.Equal("ValueStaticPropertyB", obj[nameof(MyStaticObject.StaticPropertyB)]);
            Assert.True(obj.ContainsKey(nameof(MyStaticObject.StaticYoyo)));
            Assert.False(obj.ContainsKey(nameof(MyStaticObject.Invalid)));
        }

        {
            var obj = new ScriptObject();
            obj.Import(new MyObject2(), renamer: member => member.Name);

            Assert.Equal(9, obj.Count);
            Assert.True(obj.ContainsKey(nameof(MyStaticObject.StaticFieldA)));
            Assert.True(obj.ContainsKey(nameof(MyObject.PropertyA)));
            Assert.True(obj.ContainsKey(nameof(MyObject2.PropertyC)));
        }
    }

    [Fact]
    public void TestScriptObjectImportDelegateOptionalParameter()
    {
        var obj = new ScriptObject();
        OptionalTextDelegate formatter = text => text.ToUpperInvariant();
        obj.Import("formatter", formatter);

        var function = (IScriptCustomFunction)(obj["formatter"] ?? throw new Xunit.Sdk.XunitException("Expected formatter function."));
        Assert.Equal(0, function.RequiredParameterCount);
        Assert.Equal(1, function.ParameterCount);

        var parameterInfo = function.GetParameterInfo(0);
        Assert.True(parameterInfo.HasDefaultValue);
        Assert.Equal("default", parameterInfo.DefaultValue);

        var context = new TemplateContext();
        context.PushGlobal(obj);

        var result = Template.Parse("{{ formatter() }}").Render(context);

        Assert.Equal("DEFAULT", result);
    }


    [Fact]
    public void TestScriptObjectAccessor()
    {
        {
            var context = new TemplateContext();
            var obj = new MyObject();
            var accessor = context.GetMemberAccessor(obj);

            Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "field_a"));
            Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "field_b"));
            Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "property_a"));
            Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "property_b"));

            Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_field_a"));
            Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_field_b"));
            Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_property_a"));
            Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_property_b"));
        }

        // Test Filter
        {
            var context = new TemplateContext { MemberFilter = member => member is PropertyInfo };
            var obj = new MyObject();
            var accessor = context.GetMemberAccessor(obj);

            Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "field_a"));
            Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "field_b"));
            Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "property_a"));
            Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "property_b"));

            Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_field_a"));
            Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_field_b"));
            Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_property_a"));
            Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_property_b"));
        }


        // Test Renamer
        {
            var context = new TemplateContext { MemberRenamer = member => member.Name };
            var obj = new MyObject();
            var accessor = context.GetMemberAccessor(obj);

            Assert.True(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyObject.FieldA)));
            Assert.True(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyObject.FieldB)));
            Assert.True(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyObject.PropertyA)));
            Assert.True(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyObject.PropertyB)));

            Assert.False(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyStaticObject.StaticFieldA)));
            Assert.False(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyStaticObject.StaticFieldB)));
            Assert.False(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyStaticObject.StaticPropertyA)));
            Assert.False(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyStaticObject.StaticPropertyB)));
        }

        // Test Reset clears cached typed accessors that captured the previous MemberFilter
        {
            var context = new TemplateContext
            {
                EnableRelaxedMemberAccess = false,
                MemberFilter = _ => true
            };
            var obj = new MyObject { PropertyA = "allowed", PropertyB = "blocked" };
            var template = Template.Parse("{{ model.property_b }}");
            var accessor = context.GetMemberAccessor(obj);
            var globals = new ScriptObject();
            globals["model"] = obj;
            context.PushGlobal(globals);

            Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "property_b"));
            Assert.Equal("blocked", template.Render(context));

            context.Reset();
            context.MemberFilter = member => member.Name == nameof(MyObject.PropertyA);

            accessor = context.GetMemberAccessor(obj);
            Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "property_b"));

            globals = new ScriptObject();
            globals["model"] = obj;
            context.PushGlobal(globals);

            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            Assert.Contains("Cannot get member", exception.Message);
        }
    }

    [Fact]
    public void TestNullableArgument()
    {
        var template = Template.Parse("{{ tester 'input1' 1 }}");
        var context = new TemplateContext();
        var testerObj = new ScriptObjectWithNullable();
        context.PushGlobal(testerObj);
        var result = template.Render(context);
        TextAssert.AreEqual("input1 Value: 1", result);
    }

    [Fact]
    public void TestPropertyInheritance()
    {
        var scriptObject = new ScriptObject
        {
            {"a", new MyObject {PropertyA = "ClassA"}},
            {"b", new MyObject2 {PropertyA = "ClassB", PropertyC = "ClassB-PropC"}}
        };

        var context = new TemplateContext();
        context.PushGlobal(scriptObject);

        var result = Template.Parse("{{a.property_a}}-{{b.property_a}}-{{b.property_c}}").Render(context);
        TextAssert.AreEqual("ClassA-ClassB-ClassB-PropC", result);
    }

    [Fact]
    public void TestRenderRuntimeException()
    {
        var template = Template.Parse("Test {{ 'error' | unknown }} behind");
        var context = new TemplateContext();
        context.RenderRuntimeException = TemplateContext.RenderRuntimeExceptionDefault;
        context.Evaluate(GetPage(template));
        var result = context.Output.ToString();
        Assert.Matches(@"^Test \[.+\] behind$", result ?? string.Empty);
    }

    [Fact]
    public void TestRenderRuntimeExceptionWithCustomFormat()
    {
        var template = Template.Parse("Test {{ 'error' | unknown }} behind");
        var context = new TemplateContext();
        context.RenderRuntimeException = ex => string.Format("#CodeBrix.Templating-Exception:{0}#", ex.OriginalMessage);
        context.Evaluate(GetPage(template));
        var result = context.Output.ToString();
        Assert.Matches(@"^Test #CodeBrix.Templating-Exception:.+# behind$", result ?? string.Empty);
    }

    [Fact]
    public void TestWithCharProperty()
    {
        var test = new ClassWithChar()
        {
            Char = 'a'
        };

        var template = Template.Parse("{{ model.char }}");
        var context = new TemplateContext();
        var result = template.Render(new { model = test });
        Assert.Equal("a", result);
    }

    private class ClassWithChar
    {
        public char Char { get; set; }
    }

    [Fact]
    public void TestRelaxedMemberAccess()
    {
        var scriptObject = new ScriptObject
        {
            {"a", new MyObject {PropertyA = "A"}}
        };

        // Test unrelaxed member access.
        {
            var context = new TemplateContext()
            {
                EnableRelaxedTargetAccess = false,
                EnableRelaxedMemberAccess = false,
            };
            context.PushGlobal(scriptObject);

            var result = Template.Parse("{{a.property_a").Render(context);
            Assert.Equal("A", result);

            result = Template.Parse("{{null_ref?.property_a").Render(context);
            Assert.Equal(string.Empty, result);

            Assert.ThrowsAny<ScriptRuntimeException>(() =>
               Template.Parse("{{a.property_a.null_ref}}").Render(context));

            Assert.ThrowsAny<ScriptRuntimeException>(() =>
               Template.Parse("{{null_ref.null_ref}}").Render(context));
        }

        // Test relaxed member access.
        {
            var context = new TemplateContext
            {
                EnableRelaxedTargetAccess = true,
                EnableRelaxedMemberAccess = true
            };
            context.PushGlobal(scriptObject);

            var result = Template.Parse("{{a.property_a").Render(context);
            Assert.Equal("A", result);

            result = Template.Parse("{{a.property_a.null_ref}}").Render(context);
            Assert.Equal(string.Empty, result);

            result = Template.Parse("{{null_ref.null_ref}}").Render(context);
            Assert.Equal(string.Empty, result);
        }
    }

    [Fact]
    public void TestRelaxedListIndexerAccess()
    {
        var scriptObject = new ScriptObject
        {
            {"list", new List<string> {"value" } }
        };

        // Test unrelaxed indexer access.
        {
            var context = new TemplateContext()
            {
                EnableRelaxedMemberAccess = false,
            };
            context.PushGlobal(scriptObject);

            var result = Template.Parse("{{list[0]").Render(context);
            Assert.Equal("value", result);

            Assert.ThrowsAny<ScriptRuntimeException>(() =>
               Template.Parse("{{list[0].null_ref.null_ref}}").Render(context));

            Assert.ThrowsAny<ScriptRuntimeException>(() =>
               Template.Parse("{{list[-1].null_ref}}").Render(context));

            Assert.ThrowsAny<ScriptRuntimeException>(() =>
               Template.Parse("{{null_ref[-1].null_ref}}").Render(context));
        }

        // Test relaxed member access.
        {
            var context = new TemplateContext
            {
                EnableNullIndexer = false,
                EnableRelaxedTargetAccess = true,
                EnableRelaxedMemberAccess = true
            };
            context.PushGlobal(scriptObject);

            var result = Template.Parse("{{list[0]").Render(context);
            Assert.Equal("value", result);

            result = Template.Parse("{{list[0].null_ref.null_ref}}").Render(context);
            Assert.Equal(string.Empty, result);

            result = Template.Parse("{{list[-1].null_ref}}").Render(context);
            Assert.Equal(string.Empty, result);

            result = Template.Parse("{{null_ref[-1].null_ref}}").Render(context);
            Assert.Equal(string.Empty, result);
        }
    }

    [Fact]
    public void TestRelaxedDictionaryIndexerAccess()
    {
        var scriptObject = new ScriptObject
        {
            {"dictionary", new Dictionary<string, string> { { "key", "value" } } }
        };

        // Test unrelaxed indexer access.
        {
            var context = new TemplateContext();
            context.PushGlobal(scriptObject);

            var result = Template.Parse("{{dictionary['key']").Render(context);
            Assert.Equal("value", result);

            Assert.ThrowsAny<ScriptRuntimeException>(() =>
               Template.Parse("{{dictionary['key'].null_ref.null_ref}}").Render(context));

            Assert.ThrowsAny<ScriptRuntimeException>(() =>
               Template.Parse("{{dictionary['null_ref'].null_ref}}").Render(context));

            Assert.ThrowsAny<ScriptRuntimeException>(() =>
               Template.Parse("{{null_ref['null_ref'].null_ref}}").Render(context));
        }

        // Test relaxed member access.
        {
            var context = new TemplateContext
            {
                EnableRelaxedTargetAccess = true,
                EnableRelaxedMemberAccess = true
            };
            context.PushGlobal(scriptObject);

            var result = Template.Parse("{{dictionary['key']").Render(context);
            Assert.Equal("value", result);

            result = Template.Parse("{{dictionary['key'].null_ref.null_ref}}").Render(context);
            Assert.Equal(string.Empty, result);

            result = Template.Parse("{{dictionary['null_ref'].null_ref}}").Render(context);
            Assert.Equal(string.Empty, result);

            result = Template.Parse("{{null_ref['null_ref'].null_ref}}").Render(context);
            Assert.Equal(string.Empty, result);
        }
    }

    [Fact]
    public void TestIndexerOnNET()
    {
        var myobject = new MyObject() { FieldA = "yo" };
        var result = Template.Parse("{{obj['field_a']}}").Render(new ScriptObject() { { "obj", myobject } });
        Assert.Equal("yo", result);
    }

    [Fact]
    public void TestItemIndexerOnNET_String_Getter()
    {
        var expected = "One";
        var key = "alpha";
        var myobject = new ClassWithItemIndexerString
        {
            [key] = expected
        };
        var result = Template.Parse($"{{{{obj['{key}']}}}}").Render(new ScriptObject() { { "obj", myobject } });
        Assert.Equal(expected, result);
    }
    [Fact]
    public void TestItemIndexerOnNET_String_Setter()
    {
        var expected = "One";
        var key = "alpha";
        var myobject = new ClassWithItemIndexerString
        {
            [key] = "Initial"
        };
        _ = Template.Parse($"{{{{obj['{key}'] = '{expected}'}}}}").Render(new ScriptObject() { { "obj", myobject } });
        Assert.Equal(expected, myobject[key]);
    }
    [Fact]
    public void TestItemIndexerOnNET_Integer_Getter()
    {
        var expected = "One";
        var key = 5;
        var myobject = new ClassWithItemIndexerInteger()
        {
            [key] = expected
        };
        var result = Template.Parse($"{{{{obj[{key}]}}}}").Render(new ScriptObject() { { "obj", myobject } });
        Assert.Equal(expected, result);
    }
    [Fact]
    public void TestItemIndexerOnNET_Integer_Setter()
    {
        var expected = "One";
        var key = 5;
        var myobject = new ClassWithItemIndexerInteger
        {
            [key] = "Initial"
        };
        _ = Template.Parse($"{{{{obj[{key}] = '{expected}'}}}}").Render(new ScriptObject() { { "obj", myobject } });
        Assert.Equal(expected, myobject[key]);
    }

    [Fact]
    public void TestCaseInsensitiveLookupOnScriptObject()
    {
        var obj = new ScriptObject(StringComparer.OrdinalIgnoreCase);
        obj["Name"] = "world";
        var context = new TemplateContext();
        context.PushGlobal(obj);
        var template = Template.Parse("Hello {{ name }}!");
        var result = template.Render(context);
        Assert.Equal("Hello world!", result);
    }

    [Fact]
    public void TestCaseInsensitiveLookupOnHierarchy()
    {
        var obj = new ScriptObject(StringComparer.OrdinalIgnoreCase);
        obj.Import(new { UPPERCASED = new { lowercased = 42 } }, renamer: mi => mi.Name);

        var context = new TemplateContext(StringComparer.OrdinalIgnoreCase);
        context.PushGlobal(obj);

        var result = Template.Parse("{{UPPERCASED.lowercased}}-{{uppercased.LOWERCASED}}-{{UPPERCASED.LOWERCASED}}-{{uppercased.lowercased}}").Render(context);
        TextAssert.AreEqual("42-42-42-42", result);
    }
    [Fact]
    public void TestNestedLoopLimit()
    {
        // Test that nested loops properly enforce LoopLimit per loop level
        var context = new TemplateContext
        {
            LoopLimit = 2
        };

        // Create a template with nested loops where inner loop exceeds the limit
        var template = Template.Parse(@"{{
for i in 1..2
  for j in 1..3
    i + j
  end
end
}}");

        // This should throw because the inner loop has 3 iterations, exceeding limit of 2
        var exception = AssertThrows<ScriptRuntimeException>(() => template.Render(context));
        Assert.Contains("LoopLimit `2`", exception.Message);
    }

    [Fact]
    public void TestNestedLoopLimitSimple()
    {
        // Simple test to verify the fix works
        var context = new TemplateContext
        {
            LoopLimit = 3
        };

        // Create a template where inner loop exceeds limit
        var template = Template.Parse(@"{{
for i in 1..4
  for j in 1..2
    i + j
  end
end
}}");

        // This should throw because inner loop has 4 iterations > limit of 3
        var exception = AssertThrows<ScriptRuntimeException>(() => template.Render(context));
        Assert.Contains("LoopLimit `3`", exception.Message);
    }

    [Fact]
    public void TestNestedLoopLimitInnerLoopExceeds()
    {
        // Test that inner loop properly enforces LoopLimit
        var context = new TemplateContext
        {
            LoopLimit = 8
        };

        // Create a template where the inner loop alone exceeds the limit
        var template = Template.Parse(@"{{
for i in 1..2
  for j in 1..6
    i + j
  end
end
}}");

        // This should throw because the inner loop has 6 iterations, exceeding limit of 4
        var exception = AssertThrows<ScriptRuntimeException>(() => template.Render(context));
        Assert.Contains("Exceeding number of iteration limit `8` for loop statement", exception.Message);
    }

    [Fact]
    public void TestNestedLoopLimitWithinBounds()
    {
        // Test that nested loops work correctly when within limits
        var context = new TemplateContext
        {
            LoopLimit = 14
        };

        // Create a template with nested loops that should NOT exceed the limit
        var template = Template.Parse(@"{{
for i in 1..2
  for j in 1..6
    i + j
  end
end
}}");

        var result = template.Render(context);
        Assert.Contains("2", result); // Should contain some output
    }

    [Fact]
    public void TestTripleNestedLoopLimit()
    {
        // Test that triple nested loops properly enforce LoopLimit per loop level
        var context = new TemplateContext
        {
            LoopLimit = 8
        };

        // Create a template with triple nested loops where innermost loop exceeds the limit
        var template = Template.Parse(@"{{
for i in 1..2
  for j in 1..2
    for k in 1..3
      i + j + k
    end
  end
end
}}");

        // This should throw because the innermost loop has 3 iterations, exceeding limit of 8
        var exception = AssertThrows<ScriptRuntimeException>(() => template.Render(context));
        Assert.Contains("Exceeding number of iteration limit `8` for loop statement", exception.Message);
    }

    [Fact]
    public void TestNestedLoopLimitIndependentCounters()
    {
        // Test that each loop level has independent counters
        var context = new TemplateContext
        {
            LoopLimit = 3
        };

        // Create a template where outer loop is within limit but inner loop exceeds
        var template = Template.Parse(@"{{
for i in 1..2
  for j in 1..5
    i + j
  end
end
}}");

        // This should throw on the inner loop (5 iterations > 3 limit)
        var exception = AssertThrows<ScriptRuntimeException>(() => template.Render(context));
        Assert.Contains("LoopLimit `3`", exception.Message);
    }

    [Fact]
    public void TestObjectToStringEscapesCorrectlyWithLazyEvaluation()
    {
        var context = new TemplateContext();
        var template = Template.Parse("""{{ [["a", "b"]] | array.each @(do; ret $0 | array.join("#"); end) }}""");
        var result = template.Render(context);
        TextAssert.AreEqual("""["a#b"]""", context.ObjectToString(result));
    }

    [Fact]
    public void RecursiveLimitShouldThrowAtConfiguredDepth()
    {
        // Recursive function that counts down - should be stopped by RecursiveLimit
        var template = Template.Parse("{{ func f(n); if n > 0; f(n - 1); end; end; f(10) }}");
        var context = new TemplateContext { RecursiveLimit = 5 };

        var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
        Assert.Contains("recursive", exception.Message);
    }

    [Fact]
    public void WhileLoopShouldRespectLoopLimit()
    {
        var template = Template.Parse("{{ x = 0; while x < 100; x = x + 1; end }}");
        var context = new TemplateContext { LoopLimit = 10 };

        var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
        Assert.Contains("iteration limit", exception.Message);
    }

    [Fact]
    public void CircularIncludeShouldHitRecursiveLimit()
    {
        // Template that includes itself - caught by RecursiveLimit
        var template = Template.Parse("{{ include 'self' }}");
        var context = new TemplateContext
        {
            RecursiveLimit = 5,
            TemplateLoader = new CircularIncludeLoader()
        };

        var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
        Assert.Contains("recursive", exception.Message);
    }

    [Fact]
    public void ObjectEvalShouldRespectLoopLimit()
    {
        // Eval creates a template that loops - should inherit the existing LoopLimit
        var template = Template.Parse(@"{{ ""for i in 1..100; i; end"" | object.eval }}");
        var context = new TemplateContext { LoopLimit = 5 };

        var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
        Assert.Contains("LoopLimit", exception.Message);
    }

    [Fact]
    public void NullConditionalShouldShortCircuitFollowingIndexers()
    {
        var template = Template.Parse("{{ a?.b[0][1] }}");

        var nullResult = template.Render(new { a = (object)null });
        Assert.Equal(string.Empty, nullResult);

        var valueResult = template.Render(new { a = new { b = new[] { new[] { "skip", "ok" } } } });
        Assert.Equal("ok", valueResult);
    }

    private class CircularIncludeLoader : ITemplateLoader
    {
        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            return templateName;
        }

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            return "{{ include 'self' }}";
        }

        public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            return ValueTask.FromResult(Load(context, callerSpan, templatePath));
        }
    }

    private class MyObject : MyStaticObject
    {
        public string FieldA;

#pragma warning disable 649
        public string FieldB;
#pragma warning restore 649

        public string PropertyA { get; set; }

        public string PropertyB { get; set; }

    }

    private class MyObject2 : MyObject
    {
        public string PropertyC { get; set; }
    }

    private class MyStaticObject
    {
        static MyStaticObject()
        {
            StaticPropertyA = "ValueStaticPropertyA";
            StaticPropertyB = "ValueStaticPropertyB";
        }

        public static string StaticFieldA = "ValueStaticFieldA";

        public static string StaticFieldB = "ValueStaticFieldB";

        public static string StaticPropertyA { get; set; }

        public static string StaticPropertyB { get; set; }

        public string Invalid()
        {
            return null;
        }

        public static string StaticYoyo(string text)
        {
            return "yoyo " + text;
        }
    }

    private class MyStaticObject2 : MyStaticObject
    {
        public static new string StaticYoyo(string text)
        {
            return "yoyo2 " + text;
        }
    }

    public class ScriptObjectWithNullable : ScriptObject
    {
        public static string Tester(string text, int? value = null)
        {
            return value.HasValue ? text + " Value: " + value.Value : text;
        }
    }

    public class ClassWithItemIndexerString
    {
        private readonly Dictionary<string, string> _dictionary = new Dictionary<string, string>();
        public string this[string key]
        {
            get => _dictionary.GetValueOrDefault(key) ?? string.Empty;
            set
            {
                if (this._dictionary.ContainsKey(key))
                {
                    this._dictionary[key] = value;
                }
                else
                {
                    this._dictionary.Add(key, value);
                }
            }
        }
    }
    public class ClassWithItemIndexerInteger
    {
        private readonly Dictionary<int, string> _dictionary = new Dictionary<int, string>();
        public string this[int key]
        {
            get => _dictionary.GetValueOrDefault(key) ?? string.Empty;
            set
            {
                if (this._dictionary.ContainsKey(key))
                {
                    this._dictionary[key] = value;
                }
                else
                {
                    this._dictionary.Add(key, value);
                }
            }
        }
    }
}
