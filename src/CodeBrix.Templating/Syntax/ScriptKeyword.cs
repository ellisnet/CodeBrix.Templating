// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;

/// <summary>
/// A verbatim node (use for custom parsing).
/// </summary>
public
partial class ScriptKeyword : ScriptVerbatim
{
    /// <summary><c>This</c>.</summary>
    public static ScriptKeyword This() => new ScriptKeyword("this");
    /// <summary><c>Func</c>.</summary>
    public static ScriptKeyword Func() => new ScriptKeyword("func");
    /// <summary><c>Do</c>.</summary>
    public static ScriptKeyword Do() => new ScriptKeyword("do");
    /// <summary><c>Break</c>.</summary>
    public static ScriptKeyword Break() => new ScriptKeyword("break");
    /// <summary><c>Capture</c>.</summary>
    public static ScriptKeyword Capture() => new ScriptKeyword("capture");
    /// <summary><c>Case</c>.</summary>
    public static ScriptKeyword Case() => new ScriptKeyword("case");
    /// <summary><c>Continue</c>.</summary>
    public static ScriptKeyword Continue() => new ScriptKeyword("continue");
    /// <summary><c>Else</c>.</summary>
    public static ScriptKeyword Else() => new ScriptKeyword("else");
    /// <summary><c>End</c>.</summary>
    public static ScriptKeyword End() => new ScriptKeyword("end");
    /// <summary><c>If</c>.</summary>
    public static ScriptKeyword If() => new ScriptKeyword("if");
    /// <summary><c>In</c>.</summary>
    public static ScriptKeyword In() => new ScriptKeyword("in");
    /// <summary><c>For</c>.</summary>
    public static ScriptKeyword For() => new ScriptKeyword("for");
    /// <summary><c>Import</c>.</summary>
    public static ScriptKeyword Import() => new ScriptKeyword("import");
    /// <summary><c>ReadOnly</c>.</summary>
    public static ScriptKeyword ReadOnly() => new ScriptKeyword("readonly");
    /// <summary><c>Ret</c>.</summary>
    public static ScriptKeyword Ret() => new ScriptKeyword("ret");
    /// <summary><c>TableRow</c>.</summary>
    public static ScriptKeyword TableRow() => new ScriptKeyword("tablerow");
    /// <summary><c>When</c>.</summary>
    public static ScriptKeyword When() => new ScriptKeyword("when");
    /// <summary><c>While</c>.</summary>
    public static ScriptKeyword While() => new ScriptKeyword("while");
    /// <summary><c>With</c>.</summary>
    public static ScriptKeyword With() => new ScriptKeyword("with");
    /// <summary><c>Wrap</c>.</summary>
    public static ScriptKeyword Wrap() => new ScriptKeyword("wrap");
    /// <summary><c>ScriptKeyword</c>.</summary>
    public ScriptKeyword()
    {
    }
    /// <summary><c>ScriptKeyword</c>.</summary>
    public ScriptKeyword(string value) : base(value)
    {
        Value = value;
    }
}
