// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>IScriptVisitorContext</c>.</summary>
public
interface IScriptVisitorContext
{
    /// <summary><c>Current</c>.</summary>
    ScriptNode Current { get; }
    /// <summary><c>Parent</c>.</summary>
    ScriptNode Parent { get; }
    /// <summary><c>Ancestors</c>.</summary>
    IEnumerable<ScriptNode> Ancestors { get; }
}
