// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.Json;
using CodeBrix.Templating.Runtime;
using Xunit;

namespace CodeBrix.Templating.Tests.TestJsonSupport; //was previously: Scriban.Tests.TestJsonSupport;

public class TestModelWithJsonElement {
    [InlineData("""null""", "")]
    [InlineData("""true""", "true")]
    [InlineData("""false""", "false")]
    [InlineData("""123.45""", "123.45")]
    [InlineData("\"bar\"", "bar")]
    [InlineData("""[1, 2, 3]""", "[1, 2, 3]")]
    [InlineData("""{ "foo": "bar" }""", "{foo: \"bar\"}")]
    [Theory]
    public void Can_import_JsonElement_property(string json, string expected)
    {
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        var model = new {
            foo = jsonElement,
        };

        var result = RenderHelper.Render(
            script: "{{ foo }}",
            scriptObject: ScriptObject.From(model)
        );

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Can_import_boxed_jsonElement()
    {
        var model = JsonSerializer.Deserialize<Dictionary<string, object>>("""{ "model": { "foo": "bar" } }""") ?? throw new Xunit.Sdk.XunitException("Expected JSON model.");

        // ensure we have a boxed JsonElement:
        Assert.Equal(typeof(string), model.GetType().GetGenericArguments()[0]);
        Assert.Equal(typeof(object), model.GetType().GetGenericArguments()[1]);
        Assert.Equal(typeof(JsonElement), model["model"].GetType());

        var result = RenderHelper.Render(
            script: "{{ model.foo }}",
            scriptObject: ScriptObject.From(model)
        );

        Assert.Equal("bar", result);
    }

    [Fact]
    public void Can_import_jsonElement_in_typed_class()
    {
        var data = JsonSerializer.Deserialize<JsonElement>("""{ "foo": "bar" }""");

        var model = new MyClass("name", data);

        var result = RenderHelper.Render(
            script: """
            Name: {{ name }}
            Data.Foo: {{ data.foo }}
            """,
            scriptObject: ScriptObject.From(model)
        ).ReplaceLineEndings("\n");

        Assert.Equal("Name: name\nData.Foo: bar", result);
    }

    [Fact]
    public void Can_import_jsonElement_in_typed_struct()
    {
        var data = JsonSerializer.Deserialize<JsonElement>("""{ "foo": "bar" }""");

        var model = new MyStruct("name", data);

        var result = RenderHelper.Render(
            script: """
            Name: {{ name }}
            Data.Foo: {{ data.foo }}
            """,
            scriptObject: ScriptObject.From(model)
        ).ReplaceLineEndings("\n");

        Assert.Equal("Name: name\nData.Foo: bar", result);
    }


    private record MyClass(
        string Name,
        JsonElement Data
    );

    private record struct MyStruct(
        string Name,
        JsonElement Data
    );
}
