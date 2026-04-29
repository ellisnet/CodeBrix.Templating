// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using CodeBrix.Templating.Parsing;

namespace CodeBrix.Templating.Runtime; //was previously: Scriban.Runtime;

/// <summary>
/// Can apply a transform to each element (e.g ScriptArray.Transform(...))
/// </summary>
public
interface IScriptTransformable
{
    /// <summary><c>ElementType</c>.</summary>
    Type ElementType { get; }
    /// <summary><c>CanTransform</c>.</summary>
    bool CanTransform(Type transformType);
    /// <summary><c>Visit</c>.</summary>
    bool Visit(TemplateContext context, SourceSpan span, Func<object, bool> visit);
    /// <summary><c>Transform</c>.</summary>
    object Transform(TemplateContext context, SourceSpan span, Func<object, object> apply, Type destType);
}
