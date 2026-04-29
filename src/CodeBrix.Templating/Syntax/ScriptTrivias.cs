// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptTrivias</c>.</summary>
public
class ScriptTrivias
{
    /// <summary><c>ScriptTrivias</c>.</summary>
    public ScriptTrivias()
    {
        Before = new List<ScriptTrivia>();
        After = new List<ScriptTrivia>();
    }
    /// <summary><c>Before</c>.</summary>
    public List<ScriptTrivia> Before { get; }
    /// <summary><c>After</c>.</summary>
    public List<ScriptTrivia> After { get; }
}
