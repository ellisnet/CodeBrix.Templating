// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using CodeBrix.Templating.Functions;
using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Runtime;
using CodeBrix.Templating.Syntax;

namespace CodeBrix.Templating.Tests; //was previously: Scriban.Tests;

public class TestFunctions
{
    public class Arrays
    {
        [Fact]
        public void TestOffset()
        {
            Assert.Null(ArrayFunctions.Offset(null, 0));
        }

        [Fact]
        public void TestLimit()
        {
            Assert.Null(ArrayFunctions.Limit(null, 0));
        }

        [Fact]
        public void TestSortNoError()
        {
            TestParser.AssertTemplate("true", "{{ [1,2] || array.sort }}");
        }

        [Fact]
        public void TestSortIsStableAcrossChainedSorts()
        {
            var context = new TemplateContext();
            var items = new ScriptArray
            {
                new ScriptObject { { "name", "00" }, { "key", 1 } },
                new ScriptObject { { "name", "01" }, { "key", 2 } },
                new ScriptObject { { "name", "02" }, { "key", 3 } },
                new ScriptObject { { "name", "03" }, { "key", 2 } },
                new ScriptObject { { "name", "04" }, { "key", 1 } },
                new ScriptObject { { "name", "05" }, { "key", 0 } },
                new ScriptObject { { "name", "06" }, { "key", 3 } },
                new ScriptObject { { "name", "07" }, { "key", 1 } },
                new ScriptObject { { "name", "08" }, { "key", 2 } },
                new ScriptObject { { "name", "09" }, { "key", 2 } },
                new ScriptObject { { "name", "10" }, { "key", 1 } },
                new ScriptObject { { "name", "11" }, { "key", 0 } },
                new ScriptObject { { "name", "12" }, { "key", 0 } },
                new ScriptObject { { "name", "13" }, { "key", 0 } },
                new ScriptObject { { "name", "14" }, { "key", 2 } },
                new ScriptObject { { "name", "15" }, { "key", 3 } },
                new ScriptObject { { "name", "16" }, { "key", 1 } },
            };

            var sortedByName = ArrayFunctions.Sort(context, new SourceSpan(), items, "name") ?? throw new Xunit.Sdk.XunitException("Expected a sorted array.");
            var sortedByKey = ArrayFunctions.Sort(context, new SourceSpan(), sortedByName, "key") ?? throw new Xunit.Sdk.XunitException("Expected a sorted array.");
            var orderedNames = sortedByKey.Cast<ScriptObject>().Select(item => item["name"]?.ToString() ?? throw new Xunit.Sdk.XunitException("Expected a name value.")).ToArray();

            Assert.Equal(new[]
            {
                "05", "11", "12", "13",
                "00", "04", "07", "10", "16",
                "01", "03", "08", "09", "14",
                "02", "06", "15",
            }, orderedNames);
        }

        [Fact]
        public void TestSortFallsBackToNestedMemberPath()
        {
            var context = new TemplateContext();
            var lines = new ScriptArray
            {
                new ScriptObject
                {
                    { "department", "food" },
                    { "product", new ScriptObject { { "name", "celery" } } },
                },
                new ScriptObject
                {
                    { "department", "pets" },
                    { "product", new ScriptObject { { "name", "dog" } } },
                },
                new ScriptObject
                {
                    { "department", "bakery" },
                    { "product", new ScriptObject { { "name", "bread" } } },
                },
            };

            var sorted = ArrayFunctions.Sort(context, new SourceSpan(), lines, "product.name") ?? throw new Xunit.Sdk.XunitException("Expected a sorted array.");
            var orderedDepartments = sorted.Cast<ScriptObject>().Select(item => item["department"]?.ToString() ?? throw new Xunit.Sdk.XunitException("Expected a department value.")).ToArray();

            Assert.Equal(new[] { "bakery", "food", "pets" }, orderedDepartments);
        }

        [Fact]
        public void TestSortPrefersExactDottedMemberOverNestedPath()
        {
            var context = new TemplateContext();
            var lines = new ScriptArray
            {
                new ScriptObject
                {
                    { "department", "food" },
                    { "product.name", "celery" },
                    { "product", new ScriptObject { { "name", "sedano" } } },
                },
                new ScriptObject
                {
                    { "department", "pets" },
                    { "product.name", "dog" },
                    { "product", new ScriptObject { { "name", "cane" } } },
                },
            };

            var sorted = ArrayFunctions.Sort(context, new SourceSpan(), lines, "product.name") ?? throw new Xunit.Sdk.XunitException("Expected a sorted array.");
            var orderedDepartments = sorted.Cast<ScriptObject>().Select(item => item["department"]?.ToString() ?? throw new Xunit.Sdk.XunitException("Expected a department value.")).ToArray();

            Assert.Equal(new[] { "food", "pets" }, orderedDepartments);
        }

        [Fact]
        public void TestContains()
        {
            var mixed = new object[] { "hi", 1, TestEnum.First };
            Assert.True(ArrayFunctions.Contains(mixed, "hi"));
            Assert.True(ArrayFunctions.Contains(mixed, 1));
            Assert.True(ArrayFunctions.Contains(mixed, TestEnum.First));
            Assert.True(ArrayFunctions.Contains(mixed, "First"));
            Assert.True(ArrayFunctions.Contains(mixed, 100));
            Assert.False(ArrayFunctions.Contains(mixed, TestEnum.Second));
            Assert.False(ArrayFunctions.Contains(mixed, 101));
            Assert.False(ArrayFunctions.Contains(mixed, "Third"));
            Assert.False(ArrayFunctions.Contains(null, 1));
            TestParser.AssertTemplate("true", "{{ value | array.contains 'First' }}", model: new ObjectModel { Value = mixed });
            TestParser.AssertTemplate("true", "{{ value | array.contains 100 }}", model: new ObjectModel { Value = mixed });
            TestParser.AssertTemplate("false", "{{ value | array.contains 'Second' }}", model: new ObjectModel { Value = mixed });
            TestParser.AssertTemplate("false", "{{ value | array.contains 101 }}", model: new ObjectModel { Value = mixed });
            TestParser.AssertTemplate("false", "{{ value | array.contains 'Third' }}", model: new ObjectModel { Value = mixed });
            TestParser.AssertTemplate("false", "{{ null | array.contains 100 }}");
        }
        class ObjectModel
        {
            public object[] Value { get; set; }
        }

        enum TestEnum : int
        {
            First = 100,
            Second = 101
        }
    }

    public class Strings
    {
        [Fact]
        public void TestSliceError()
        {
            TestParser.AssertTemplate("text(1,16) : error : Invalid number of arguments `0` passed to `string.slice` while expecting `2` arguments", "{{ string.slice }}");
        }

        [Fact]
        public void TestSliceAtError()
        {
            TestParser.AssertTemplate("text(1,17) : error : Invalid number of arguments `0` passed to `string.slice1` while expecting `2` arguments", "{{ string.slice1 }}");
        }

        [Fact]
        public void TestNullFriendlyStringFunctionsDoNotRejectNullArguments()
        {
            Assert.Equal(StringFunctions.Capitalize(null), Render("{{ null | string.capitalize }}"));
            Assert.Equal(ToTemplateBoolean(StringFunctions.Empty(null)), Render("{{ null | string.empty }}"));
            Assert.Equal(ToTemplateBoolean(StringFunctions.Whitespace(null)), Render("{{ null | string.whitespace }}"));
            Assert.Equal(ToTemplateBoolean(StringFunctions.EqualsIgnoreCase(null, null)), Render("{{ null | string.equals_ignore_case null }}"));
            Assert.Equal(StringFunctions.Md5(null), Render("{{ null | string.md5 }}"));
            Assert.Equal(StringFunctions.Sha1(null), Render("{{ null | string.sha1 }}"));
            Assert.Equal(StringFunctions.Sha256(null), Render("{{ null | string.sha256 }}"));
            Assert.Equal(StringFunctions.Sha512(null), Render("{{ null | string.sha512 }}"));
            Assert.Equal(StringFunctions.HmacSha1(null, null), Render("{{ null | string.hmac_sha1 null }}"));
            Assert.Equal(StringFunctions.HmacSha256(null, null), Render("{{ null | string.hmac_sha256 null }}"));
            Assert.Equal(StringFunctions.HmacSha512(null, null), Render("{{ null | string.hmac_sha512 null }}"));
            Assert.Equal(StringFunctions.PadLeft(null, 3), Render("{{ null | string.pad_left 3 }}"));
            Assert.Equal(StringFunctions.PadRight(null, 3), Render("{{ null | string.pad_right 3 }}"));
            Assert.Equal(StringFunctions.Base64Encode(null), Render("{{ null | string.base64_encode }}"));
            Assert.Equal(StringFunctions.Base64Decode(null), Render("{{ null | string.base64_decode }}"));
        }

        [Fact]
        public void TestNullFriendlyBuiltinStringArgumentsDoNotRejectNull()
        {
            TestParser.AssertTemplate("true", "{{ (null | html.url_escape) == null }}");
            TestParser.AssertTemplate("true", "{{ (null | date.parse) == null }}");
            TestParser.AssertTemplate("true", "{{ (null | date.parse_to_string) == null }}");
            TestParser.AssertTemplate("123", "{{ [1, 2, 3] | array.join null }}");
            TestParser.AssertTemplate("42", "{{ 42 | object.format null }}");
            TestParser.AssertTemplate("42", "{{ 42 | math.format null }}");
            TestParser.AssertTemplate("false", "{{ null | object.has_key null }}");
            TestParser.AssertTemplate("false", "{{ null | object.has_value null }}");
        }

        public record IndexOfTestCase(
            string Text,
            string Search,
            int Expected,
            int? StartIndex = null,
            int? Count = null,
            StringComparison? StringComparison = null
        );

        public static readonly IReadOnlyList<IndexOfTestCase> IndexOfTestCases = new IndexOfTestCase[]
        {
            new ("The quick brown fox", "quick", 4),
            new ("The the the the", "the", 0, null, null, StringComparison.OrdinalIgnoreCase),
            new ("The quick brown fox", "quick", -1, null, 2),
            new ("The the the the", "the", 8, 6),
        };

        public static IEnumerable<object[]> IndexOfTestCasesData => IndexOfTestCases.Select(tc => new object[] { tc });

        [Theory]
        [MemberData(nameof(IndexOfTestCasesData))]
        public void TestIndexOf(IndexOfTestCase testCase)
        {
            testCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
            var args = new[]
            {
                (Name: "text", Value: MakeString(testCase.Text)),
                (Name: "search", Value: MakeString(testCase.Search)),
                (Name: "start_index", Value: MakeInt(testCase.StartIndex)),
                (Name: "count", Value: MakeInt(testCase.Count)),
                (Name: "string_comparison", Value: MakeString(testCase.StringComparison?.ToString())),
            }.Select(x => $"{x.Name}: {x.Value}");
            var script = $@"{{{{ string.index_of {string.Join(" ", args)} }}}}";
            Template template = null;
            Record.Exception(() => template = Template.Parse(script));
            if (template is null)
            {
                throw new Xunit.Sdk.XunitException("Expected template parsing to succeed.");
            }
            Assert.False(template.HasErrors);
            Assert.NotNull(template.Messages);
            Assert.Empty(template.Messages);
            var result = template.Render();
            Assert.Equal(testCase.Expected.ToString(), result);

            static string MakeString(string value) => value is null ? "null" : $"'{value}'";
            static string MakeInt(int? value) => value is null ? "null" : value.Value.ToString();
        }

        [Theory]
        [MemberData(nameof(TestReplaceFirstArguments))]
        public void TestReplaceFirst(string source, string match, string replace, bool fromend, string expected)
        {
            var script = @"{{source | string.replace_first  match replace fromend}}";
            var template = Template.Parse(script);
            var result = template.Render(new
            {
                Source = source,
                Match = match,
                Replace = replace,
                Fromend = fromend,
            });
            Assert.Equal(result, expected);
        }

        public static readonly object[][] TestReplaceFirstArguments =
        {
            // Replace from start
            new object [] {
                "Hello, world. Goodbye, world.",    // source
                "world",                            // match
                "buddy",                            // replace
                false,                              // fromEnd
                "Hello, buddy. Goodbye, world.",    // expected
            },
            // Replace from end
            new object [] {
                "Hello, world. Goodbye, world.",    // source
                "world",                            // match
                "buddy",                            // replace
                true,                               // fromEnd
                "Hello, world. Goodbye, buddy.",    // expected
            },
            // nothing to replace
            new object [] {
                "Hello, world. Goodbye, world.",    // source
                "xxxx",                             // match
                "buddy",                            // replace
                false,                              // fromEnd
                "Hello, world. Goodbye, world.",    // expected
            },
        };

        [Fact]
        public void TestPadLeft()
        {
            var template = Template.Parse("{{ 'world' | string.pad_left 10 }}");
            var result = template.Render();
            Assert.Equal("     world", result);
        }

        [Fact]
        public void TestPadRight()
        {
            var template = Template.Parse("{{ 'hello' | string.pad_right 10 }}");
            var result = template.Render();
            Assert.Equal("hello     ", result);
        }

        [Fact]
        public void TestPadLeftRespectsLimitToString()
        {
            var template = Template.Parse("{{ 'x' | string.pad_left 4 }}");
            var context = new TemplateContext { LimitToString = 3 };

            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            Assert.Contains("LimitToString", exception.Message);
        }

        [Fact]
        public void TestPadRightRespectsLimitToString()
        {
            var template = Template.Parse("{{ 'x' | string.pad_right 4 }}");
            var context = new TemplateContext { LimitToString = 3 };

            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            Assert.Contains("LimitToString", exception.Message);
        }

        [Fact]
        public void TestAppendRespectsLimitToString()
        {
            var template = Template.Parse("{{ 'abc' | string.append 'def' }}");
            var context = new TemplateContext { LimitToString = 5 };

            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            Assert.Contains("LimitToString", exception.Message);
        }

        [Fact]
        public void TestPrependRespectsLimitToString()
        {
            var template = Template.Parse("{{ 'abc' | string.prepend 'def' }}");
            var context = new TemplateContext { LimitToString = 5 };

            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            Assert.Contains("LimitToString", exception.Message);
        }

        [Fact]
        public void TestReplaceRespectsLimitToString()
        {
            // "aa" replacing each "a" with "bbb" produces "bbbbbb" (length 6)
            var template = Template.Parse("{{ 'aa' | string.replace 'a' 'bbb' }}");
            var context = new TemplateContext { LimitToString = 5 };

            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            Assert.Contains("LimitToString", exception.Message);
        }

        [Fact]
        public void TestReplaceFirstRespectsLimitToString()
        {
            // "ab" replacing "a" with "ccccc" produces "cccccb" (length 6)
            var template = Template.Parse("{{ 'ab' | string.replace_first 'a' 'ccccc' }}");
            var context = new TemplateContext { LimitToString = 5 };

            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            Assert.Contains("LimitToString", exception.Message);
        }

        [Fact]
        public void TestAppendDoublingLoopRespectsLimitToString()
        {
            // Doubling loop: x grows exponentially, should be caught by LimitToString
            var template = Template.Parse("{{ x = 'a'; for i in 1..30; x = x | string.append x; end; x }}");
            var context = new TemplateContext { LimitToString = 1000 };

            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            Assert.Contains("LimitToString", exception.Message);
        }

        private static string Render(string script)
        {
            var template = Template.Parse(script);
            Assert.False(template.HasErrors);
            return template.Render();
        }

        private static string ToTemplateBoolean(bool value) => value ? "true" : "false";
    }

    public class Math
    {
        [Fact]
        public void TestMathUuid()
        {
            var script = @"{{math.uuid}}";
            var template = Template.Parse(script);
            var result = template.Render();
            Assert.True(Guid.TryParse(result, out var _));
        }

        [Fact]
        public void TestMathRandom()
        {
            var script = @"{{math.random 1 10}}";
            var template = Template.Parse(script);
            var result = template.Render();
            Assert.True(int.TryParse(result, out var number));
            Assert.True(number < 10 && number >= 1);
        }

        [Fact]
        public void TestMathRandomError()
        {
            TestParser.AssertTemplate("text(1,4) : error : minValue must be greater than maxValue", "{{ math.random 11 10 }}");
        }
    }
}
