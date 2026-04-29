// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using CodeBrix.Templating.Runtime;
using CodeBrix.Templating.Syntax;
using Xunit;

namespace CodeBrix.Templating.Tests.TestJsonSupport; //was previously: Scriban.Tests.TestJsonSupport;

public class TestObjectFunctions {
    [Fact]
    public void Can_parse_json()
    {
        var template = Template.Parse("""
            {{
                json = `{ "foo": { "bar": [{ "baz": 1 }, { "baz": 2 }, { "baz": 3 }] } }`
                obj = json | object.from_json
                obj.foo.bar[1].baz
            }}
            """
        );

        var result = template.Render();

        Assert.Equal("2", result);
    }

    [InlineData("""
        null
        """, """
        null
        """)]
    [InlineData("""
        true
        """, """
        true
        """)]
    [InlineData("""
        false
        """, """
        false
        """)]
    [InlineData("""
        "string"
        """, """
        "string"
        """)]
    [InlineData("""
        123
        """, """
        123
        """)]
    [InlineData("""
        123.45
        """, """
        123.45
        """)]
    [InlineData("""
        [1, 2, 3, {foo: "bar"}, { "baz": 123 }]
        """, """
        [1,2,3,{"foo":"bar"},{"baz":123}]
        """)]
    [InlineData("""
        { foo: { bar: [{ baz: 1 }, { baz: 2 }, { baz: 3 }] } }
        """, """
        {"foo":{"bar":[{"baz":1},{"baz":2},{"baz":3}]}}
        """)]
    [Theory]
    public void Can_convert_ScribanValue_to_json(string scriban, string json)
    {
        var template = Template.Parse($$$"""
            {{ {{{scriban}}} | object.to_json }}
            """
        );

        var result = template.Render();

        Assert.Equal(json, result);
    }

    [Fact]
    public void Can_convert_TypedModel_to_json()
    {
        var template = Template.Parse("""
            {{ model | object.to_json }}
            """
        );

        var result = template.Render(new {
            Model = new {
                Foo = "bar",
                Baz = new[] { 1, 2, 3 }
            }
        });

        Assert.Equal("""
            {"foo":"bar","baz":[1,2,3]}
            """, result);
    }

    [Fact]
    public void Can_handle_MemberRenamer_when_writing_json()
    {
        var template = Template.Parse("""
            {{ Model | object.to_json }}
            """
        );

        var model = new {
            Model = new {
                Foo = "bar",
                Baz = new[] { 1, 2, 3 }
            }
        };

        var result = template.Render(model, member => member.Name);

        Assert.Equal("""
            {"Foo":"bar","Baz":[1,2,3]}
            """, result);
    }

    [Fact]
    public void Throws_when_serializing_function_to_json()
    {
        var template = Template.Parse("""
            {{
                func myFunc()
                    ret 1
                end

                object.to_json @myFunc
            }}
            """
        );

        var ex = Assert.Throws<ScriptRuntimeException>(() => {
            var result = template.Render();
        });

        Assert.Equal("<input>(6,20) : error : Can not serialize functions to JSON. (Parameter 'value')", ex.Message);
    }

    [Fact]
    public void Throws_when_serializing_cyclic_object_graph_to_json()
    {
        var node = new JsonNode();
        node.Next = node;

        var template = Template.Parse("""
            {{ model | object.to_json }}
            """
        );

        var ex = Assert.Throws<ScriptRuntimeException>(() => template.Render(new { Model = node }));
        Assert.Contains("Structure is too deeply nested or contains reference loops.", ex.Message);
    }

    [Fact]
    public void Throws_when_serializing_object_graph_beyond_recursion_limit_to_json()
    {
        var root = new JsonNode();
        root.Next = new JsonNode
        {
            Next = new JsonNode
            {
                Next = new JsonNode()
            }
        };

        var template = Template.Parse("""
            {{ model | object.to_json }}
            """
        );
        var context = new TemplateContext { ObjectRecursionLimit = 3 };
        var globals = new ScriptObject();
        globals.Import(new { Model = root });
        context.PushGlobal(globals);

        var ex = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
        Assert.Contains("Structure is too deeply nested or contains reference loops.", ex.Message);
    }

    private sealed class JsonNode
    {
        public JsonNode Next { get; set; }
    }
}
