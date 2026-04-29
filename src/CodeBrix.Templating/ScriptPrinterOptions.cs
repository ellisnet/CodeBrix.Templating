// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Syntax;

namespace CodeBrix.Templating; //was previously: Scriban;

/// <summary>
/// Defines the options used for rendering back an AST/<see cref="ScriptNode"/> to a text.
/// </summary>
public
struct ScriptPrinterOptions
{
    /// <summary>
    /// The mode used to render back an AST
    /// </summary>
    public ScriptMode Mode;
}
