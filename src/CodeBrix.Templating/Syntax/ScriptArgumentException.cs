// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptArgumentException</c>.</summary>
public
class ScriptArgumentException : Exception
{
    /// <summary><c>ScriptArgumentException</c>.</summary>
    public ScriptArgumentException(int argumentIndex, string message) : base(message)
    {
        ArgumentIndex = argumentIndex;
    }
    /// <summary><c>ArgumentIndex</c>.</summary>
    public int ArgumentIndex { get; }
}
