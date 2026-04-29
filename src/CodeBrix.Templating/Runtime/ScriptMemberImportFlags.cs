// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CodeBrix.Templating.Runtime; //was previously: Scriban.Runtime;
/// <summary><c>ScriptMemberImportFlags</c>.</summary>
[Flags]
public
enum ScriptMemberImportFlags
{
    /// <summary><c>Field</c>.</summary>
    Field = 1,
    /// <summary><c>Property</c>.</summary>
    Property = 2,
    /// <summary><c>Method</c>.</summary>
    Method = 4,
    /// <summary><c>MethodInstance</c>.</summary>
    [Obsolete("Importing Method Instance is actually not supported - This flag will be removed in a future release")]
    MethodInstance = 8,
    /// <summary><c>All</c>.</summary>
    All = Field | Property | Method
}
