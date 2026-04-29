// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Text.Json;
using CodeBrix.Templating.Runtime;
using Xunit;

namespace CodeBrix.Templating.Tests.TestJsonSupport; //was previously: Scriban.Tests.TestJsonSupport;

public class TestTemplateRenderJson {
    [Fact]
    public void Template_render_accepts_json()
    {
        var json = JsonSerializer.Deserialize<JsonElement>("""{ "foo": { "bar": [{ "baz": 1 }, { "baz": 2 }, { "baz": 3 }] } }""");

        var template = Template.Parse("{{ foo.bar[1].baz }}");
        var result = template.Render(json);

        Assert.Equal("2", result);
    }

    [Fact]
    public void Template_render_accepts_script_object_created_from_json_element()
    {
        var json = JsonSerializer.Deserialize<JsonElement>("""{ "foo": "bar" }""");
        var model = ScriptObject.From(json);

        var template = Template.Parse("foo: `{{ foo }}`");
        var result = template.Render(model);

        Assert.Equal("foo: `bar`", result);
    }
}
