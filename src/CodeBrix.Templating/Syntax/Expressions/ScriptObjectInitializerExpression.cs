// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using CodeBrix.Templating.Runtime;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptObjectInitializerExpression</c>.</summary>
[ScriptSyntax("object initializer expression", "{ member1: <expression>, member2: ... }")]
public
partial class ScriptObjectInitializerExpression : ScriptExpression
{
    private ScriptToken _openBrace;
    private ScriptList<ScriptObjectMember> _members;
    private ScriptToken _closeBrace;
    /// <summary><c>ScriptObjectInitializerExpression</c>.</summary>
    public ScriptObjectInitializerExpression()
    {
        _openBrace = ScriptToken.OpenBrace();
        _openBrace.Parent = this;
        _members = new ScriptList<ScriptObjectMember>();
        _members.Parent = this;
        _closeBrace = ScriptToken.CloseBrace();
        _closeBrace.Parent = this;
    }
    /// <summary><c>OpenBrace</c>.</summary>
    public ScriptToken OpenBrace
    {
        get => _openBrace;
        set => ParentToThis(ref _openBrace, value);
    }
    /// <summary><c>Members</c>.</summary>
    public ScriptList<ScriptObjectMember> Members
    {
        get => _members;
        set => ParentToThis(ref _members, value);
    }
    /// <summary><c>CloseBrace</c>.</summary>
    public ScriptToken CloseBrace
    {
        get => _closeBrace;
        set => ParentToThis(ref _closeBrace, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        var obj = new ScriptObject();
        context.PushGlobalOnly(obj);
        try
        {
            foreach (var member in Members)
            {
                member.Evaluate(context);
            }
        }
        finally
        {
            context.PopGlobalOnly();
        }
        return obj;
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.Write(OpenBrace);
        printer.WriteListWithCommas(Members);
        printer.Write(CloseBrace);
    }
}
