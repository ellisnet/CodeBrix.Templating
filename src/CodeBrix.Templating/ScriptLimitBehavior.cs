// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

namespace CodeBrix.Templating; //was previously: Scriban;

/// <summary>
/// Controls how string conversion and rendered output limits are reported.
/// </summary>
public
enum ScriptLimitBehavior
{
    /// <summary>
    /// Truncates the text and appends an ellipsis (<c>...</c>).
    /// </summary>
    Truncate,

    /// <summary>
    /// Throws a <see cref="Syntax.ScriptRuntimeException"/> instead of truncating the text.
    /// </summary>
    Throw,
}
