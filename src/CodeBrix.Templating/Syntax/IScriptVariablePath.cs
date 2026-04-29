// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Threading.Tasks;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>IScriptVariablePath</c>.</summary>
public
interface IScriptVariablePath
{
    /// <summary><c>GetValue</c>.</summary>
    object GetValue(TemplateContext context);
    /// <summary><c>SetValue</c>.</summary>
    void SetValue(TemplateContext context, object valueToSet);
    /// <summary><c>GetFirstPath</c>.</summary>
    string GetFirstPath();
    /// <summary><c>GetValueAsync</c>.</summary>
    ValueTask<object> GetValueAsync(TemplateContext context);
    /// <summary><c>SetValueAsync</c>.</summary>
    ValueTask SetValueAsync(TemplateContext context, object valueToSet);
}
