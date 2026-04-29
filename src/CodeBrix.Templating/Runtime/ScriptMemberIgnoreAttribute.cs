// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CodeBrix.Templating.Runtime; //was previously: Scriban.Runtime;
/// <summary><c>ScriptMemberIgnoreAttribute</c>.</summary>
[AttributeUsage(AttributeTargets.Field| AttributeTargets.Property|AttributeTargets.Method)]
public
class ScriptMemberIgnoreAttribute : Attribute
{

}
