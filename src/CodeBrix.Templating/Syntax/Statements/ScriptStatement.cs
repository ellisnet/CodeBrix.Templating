// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using CodeBrix.Templating.Parsing;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;

/// <summary>
/// Base class for all statements.
/// </summary>
/// <seealso cref="ScriptNode" />
public
abstract partial class ScriptStatement : ScriptNode
{
    /// <summary><c>ScriptStatement</c>.</summary>
    protected ScriptStatement()
    {
        CanOutput = true;
    }
    /// <summary><c>CanSkipEvaluation</c>.</summary>
    public bool CanSkipEvaluation { get; protected set; }
    /// <summary><c>CanOutput</c>.</summary>
    public bool CanOutput { get; protected set; }
}
