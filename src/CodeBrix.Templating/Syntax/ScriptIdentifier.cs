// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;

/// <summary>
/// An identifier (which is not a <see cref="ScriptVariable"/>)
/// </summary>
public
partial class ScriptIdentifier : ScriptVerbatim
{
    /// <summary><c>ScriptIdentifier</c>.</summary>
    public ScriptIdentifier()
    {
    }
    /// <summary><c>ScriptIdentifier</c>.</summary>
    public ScriptIdentifier(string value) : base(value)
    {
    }
}
