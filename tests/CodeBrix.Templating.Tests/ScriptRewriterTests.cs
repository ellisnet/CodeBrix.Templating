// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Text;
using Xunit;
using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Runtime;
using CodeBrix.Templating.Syntax;

namespace CodeBrix.Templating.Tests; //was previously: Scriban.Tests;

public class ScriptRewriterTests
{
    [Theory]
    [MemberData(nameof(TestFilesHelper.ListAllTestFiles), MemberType = typeof(TestFilesHelper))]
    public void ScriptRewriter_Returns_Original_Script(string inputFileName)
    {
        var template = LoadTemplate(inputFileName);

        var rewriter = new TestCloneScriptRewriter();
        var result = rewriter.Visit(template.Page);

        // The base ScriptRewriter never changes any node, so we should end up with the same instance
        Assert.NotSame(template.Page, result);
    }

    [Theory]
    [MemberData(nameof(TestFilesHelper.ListAllTestFiles), MemberType = typeof(TestFilesHelper))]
    public void LeafCopyScriptRewriter_Returns_Identical_Script(string inputFileName)
    {
        var template = LoadTemplate(inputFileName);

        var rewriter = new TestCloneScriptRewriter();
        var result = rewriter.Visit(template.Page);

        // This rewriter makes copies of leaf nodes instead of returning the original nodes,
        // so we should end up with another instance identical to the original.
        Assert.NotSame(template.Page, result);
        Assert.Equal(ToText(template.Page), ToText(result));
    }

    private string ToText(ScriptNode node)
    {
        var checkedNode = node ?? throw new Xunit.Sdk.XunitException("Expected a script node.");
        var output = new StringBuilder();
        var context = new ScriptPrinter(new StringBuilderOutput(output));
        context.Write(checkedNode);
        return output.ToString();
    }

    private Template LoadTemplate(string inputName)
    {
        var templateSource = TestFilesHelper.LoadTestFile(inputName) ?? throw new Xunit.Sdk.XunitException($"Unable to load test file `{inputName}`.");
        var parser =
            inputName.Contains("500-liquid")
                ? (Func<string, string, ParserOptions, LexerOptions, Template>) Template.ParseLiquid
                : Template.Parse;

        LexerOptions options = LexerOptions.Default;
        if (inputName.Contains("liquid"))
        {
            options = options with { Lang = ScriptLang.Liquid };
        }
        else if (inputName.Contains("scientific"))
        {
            options = options with { Lang = ScriptLang.Scientific };
        }

        var template = parser(templateSource, inputName, default, options);
        if (template.HasErrors || template.Page is null)
        {
            if (inputName.Contains("error"))
            {
                Assert.Skip("Template has errors and didn't parse correctly. This is expected for an `error` test.");
            }
            else
            {
                Console.WriteLine(template.Messages);
                Assert.Fail("Template has errors and didn't parse correctly. This is not expected.");
            }
        }

        return template;
    }

    private class TestCloneScriptRewriter : ScriptRewriter
    {
    }
}
