// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>IScriptCustomType</c>.</summary>
public
interface IScriptCustomType : IScriptCustomTypeInfo, IScriptCustomBinaryOperation, IScriptCustomUnaryOperation, IScriptConvertibleTo
{
}
