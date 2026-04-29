// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Runtime;
using Xunit;

namespace CodeBrix.Templating.Tests.TestJsonSupport; //was previously: Scriban.Tests.TestJsonSupport;

public class TestScriptObjectJson {
    [InlineData("""null""", "{{ foo }}", "")]
    [InlineData("""{ "foo": null }""", "{{ foo }}", "")]
    [InlineData("""{ "foo": true }""", "{{ foo }}", "true")]
    [InlineData("""{ "foo": false }""", "{{ foo }}", "false")]
    [InlineData("""{ "foo": "bar" }""", "{{ foo }}", "bar")]
    [InlineData("""{ "foo": 123.45 }""", "{{ foo }}", "123.45")]
    [InlineData("""{ "foo": [1, 2, 3] }""", "{{ foo | object.typeof }}", "array")]
    [InlineData("""{ "foo": [1, 2, 3] }""", "{{ foo[0] }}", "1")]
    [InlineData("""{ "foo": { "bar": "baz" } }""", "{{ foo }}", """{bar: "baz"}""")]
    [InlineData("""{ "foo": { "bar": "baz" } }""", "{{ foo.bar }}", "baz")]
    [InlineData("""{ "foo": { "bar": [1, 2, 3] } }""", "{{ foo.bar[0] }}", "1")]
    [InlineData("""{ "foo": { "bar": [{ "baz": 1 }, { "baz": 2 }, { "baz": 3 }] } }""", "{{ foo.bar[1].baz }}", "2")]
    [Theory]
    public void ScriptObject_can_import_json_object(string json, string script, string expected)
    {
        // Test ScriptObject.From(object)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = ScriptObject.From((object) obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: model
            );

            Assert.Equal(expected, result);
        }

        // Test Import(JsonElement) extension
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptObject();
            ScriptObjectExtensions.Import(model, json: obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: model
            );

            Assert.Equal(expected, result);
        }

        // Test Import(object) extension
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptObject();
            ScriptObjectExtensions.Import(model, obj: (object) obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: model
            );

            Assert.Equal(expected, result);
        }
    }


    [InlineData("""true""", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `True`. Expecting Json Object.')")]
    [InlineData("""false""", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `False`. Expecting Json Object.')")]
    [InlineData("\"bar\"", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `String`. Expecting Json Object.')")]
    [InlineData("""123.45""", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `Number`. Expecting Json Object.')")]
    [InlineData("""[1, 2, 3]""", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `Array`. Expecting Json Object.')")]
    [Theory]
    public void ScriptObject_can_not_import_json_non_object(string json, string expected)
    {
        var obj = JsonSerializer.Deserialize<JsonElement>(json);

        // Test ScriptObject.From(object)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => {
                var model = ScriptObject.From((object) obj);
            });
            Assert.Equal(expected, ex.Message);
        }

        // Test Import(JsonElement) extension
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => {
                var model = new ScriptObject();
                ScriptObjectExtensions.Import(model, json: obj);
            });
            Assert.Equal(expected, ex.Message);
        }

        // Test Import(object) extension
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => {
                var model = new ScriptObject();
                ScriptObjectExtensions.Import(model, obj: (object) obj);
            });
            Assert.Equal(expected, ex.Message);
        }
    }
}

public class RenderHelper {
    /// <summary>
    /// Runs the specified script with the specified scriptObject in global scope
    /// with an <b>invariant culture</b> to ensure predictable formatting
    /// </summary>
    public static string Render(string script, ScriptObject scriptObject)
    {
        var context = new TemplateContext();
        context.PushGlobal(scriptObject);
        context.PushCulture(CultureInfo.InvariantCulture);

        var template = Template.Parse(script);
        return template.Render(context);
    }
}
