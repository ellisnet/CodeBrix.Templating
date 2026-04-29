// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using CodeBrix.Templating.Parsing;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>IScriptConvertibleFrom</c>.</summary>
public
interface IScriptConvertibleFrom
{
    /// <summary><c>TryConvertFrom</c>.</summary>
    bool TryConvertFrom(TemplateContext context, SourceSpan span, object value);
}
