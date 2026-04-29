// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptFlowState</c>.</summary>
public
enum ScriptFlowState
{
    /// <summary><c>None</c>.</summary>
    None,
    /// <summary><c>Break</c>.</summary>
    Break,
    /// <summary><c>Continue</c>.</summary>
    Continue,
    /// <summary><c>Return</c>.</summary>
    Return
}
