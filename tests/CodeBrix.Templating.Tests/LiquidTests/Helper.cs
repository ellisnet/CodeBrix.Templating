// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections;
using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Runtime;
using CodeBrix.Templating.Runtime.Accessors;
using CodeBrix.Templating.Tests;

namespace DotLiquid.Tests.Tags; //was previously: DotLiquid.Tests.Tags;

public class Helper
{
    public static void AssertTemplateResult(string result, string input, object model = null, bool hasForVariableNotSupportedByScriban = false, bool supportRoundTrip = true)
    {
        TestParser.AssertTemplate(result, input, ScriptLang.Liquid, false, false, model, true, expectParsingErrorForRountrip: hasForVariableNotSupportedByScriban, supportRoundTrip: supportRoundTrip);
    }
}

public class Hash : ScriptObject
{
    public static Hash FromAnonymousObject(object obj)
    {
        var hash = new Hash();
        hash.Import(obj);
        return hash;
    }

    public static Hash FromDictionary(object obj)
    {
        var hash = new Hash();
        hash.Import(obj);

        return hash;
    }
}
