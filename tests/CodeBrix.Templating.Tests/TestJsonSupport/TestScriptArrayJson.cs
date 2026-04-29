// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Runtime;
using Xunit;

namespace CodeBrix.Templating.Tests.TestJsonSupport; //was previously: Scriban.Tests.TestJsonSupport;

public class TestScriptArrayJson {
    [InlineData("""null""", "{{ array }}", "[]")]
    [InlineData("""[1, 2, 3]""", "{{ array | object.typeof }}", "array")]
    [InlineData("""[1, 2, 3]""", "{{ array[0] }}", "1")]
    [InlineData("""[{ "baz": 1 }, { "baz": 2 }, { "baz": 3 }]""", "{{ array[1].baz }}", "2")]
    [Theory]
    public void ScriptArray_can_import_json_array(string json, string script, string expected)
    {
        // Test Import(JsonElement) extension
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptArray();
            ScriptObjectExtensions.Import(model, json: obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: ScriptObject.From(new { array = model })
            );

            Assert.Equal(expected, result);
        }

        // Test Import(object) extension
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptArray();
            ScriptObjectExtensions.Import(model, obj: (object) obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: ScriptObject.From(new { array = model })
            );

            Assert.Equal(expected, result);
        }
    }


    [InlineData("""[1, 2, 3]""", "{{ array | object.typeof }}", "array")]
    [InlineData("""[1, 2, 3]""", "{{ array[0] }}", "1")]
    [InlineData("""[{ "baz": 1 }, { "baz": 2 }, { "baz": 3 }]""", "{{ array[1].baz }}", "2")]
    [Theory]
    public void ScriptArray_From_json_array(string json, string script, string expected)
    {
        var obj = JsonSerializer.Deserialize<JsonElement>(json);
        var model = ScriptArray.From(obj);

        var result = RenderHelper.Render(
            script: script,
            scriptObject: ScriptObject.From(new { array = model })
        );

        Assert.Equal(expected, result);
    }

    [InlineData("""true""")]
    [InlineData("""false""")]
    [InlineData("\"bar\"")]
    [InlineData("""123.45""")]
    [InlineData("""{ "foo": "bar" }""")]
    [Theory]
    public void ScriptArray_From_json_non_array_throws(string json)
    {
        var obj = JsonSerializer.Deserialize<JsonElement>(json);
        Assert.Throws<ArgumentException>(() => ScriptArray.From(obj));
    }

    [Fact]
    public void ScriptArray_From_enumerable()
    {
        var list = new List<object> { 1, "two", 3.0 };
        var model = ScriptArray.From(list);

        Assert.Equal(3, model.Count);
        Assert.Equal(1, model[0]);
        Assert.Equal("two", model[1]);
        Assert.Equal(3.0, model[2]);
    }

    [Fact]
    public void ScriptArray_From_null_throws()
    {
        IEnumerable source = null;
        Assert.Throws<ArgumentNullException>(() => ScriptArray.From(source));
    }

    [InlineData("""true""", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `True`. Expecting Json Array.')")]
    [InlineData("""false""", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `False`. Expecting Json Array.')")]
    [InlineData("\"bar\"", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `String`. Expecting Json Array.')")]
    [InlineData("""123.45""", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `Number`. Expecting Json Array.')")]
    [InlineData("""{ "foo": "bar" }""", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `Object`. Expecting Json Array.')")]
    [Theory]
    public void ScriptArray_can_not_import_json_non_array(string json, string expected)
    {
        var obj = JsonSerializer.Deserialize<JsonElement>(json);

        // Test Import(JsonElement) extension
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => {
                var model = new ScriptArray();
                ScriptObjectExtensions.Import(model, json: obj);
            });
            Assert.Equal(expected, ex.Message);
        }

        // Test Import(object) extension
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => {
                var model = new ScriptArray();
                ScriptObjectExtensions.Import(model, obj: (object) obj);
            });
            Assert.Equal(expected, ex.Message);
        }
    }
}
