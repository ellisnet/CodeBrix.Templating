// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using CodeBrix.Templating.Parsing;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>IScriptConvertibleTo</c>.</summary>
public
interface IScriptConvertibleTo
{
    /// <summary><c>TryConvertTo</c>.</summary>
    bool TryConvertTo(TemplateContext context, SourceSpan span, Type type, out object value);
}
