// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using CodeBrix.Templating.Parsing;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;
/// <summary><c>ScriptParameter</c>.</summary>
public
partial class ScriptParameter : ScriptNode
{
    private ScriptVariable _name;
    private ScriptToken _equalOrTripleDotToken;
    private ScriptLiteral _defaultValue;
    /// <summary><c>Name</c>.</summary>
    public ScriptVariable Name
    {
        get => _name;
        set => ParentToThisNullable(ref _name, value);
    }
    /// <summary><c>EqualOrTripleDotToken</c>.</summary>
    public ScriptToken EqualOrTripleDotToken
    {
        get => _equalOrTripleDotToken;
        set => ParentToThisNullable(ref _equalOrTripleDotToken, value);
    }
    /// <summary><c>DefaultValue</c>.</summary>
    public ScriptLiteral DefaultValue
    {
        get => _defaultValue;
        set => ParentToThisNullable(ref _defaultValue, value);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        throw new InvalidOperationException("A parameter should not be evaluated directly");
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.Write(Name);
        if (EqualOrTripleDotToken is not null)
        {
            printer.Write(EqualOrTripleDotToken);
            if (EqualOrTripleDotToken.TokenType == TokenType.Equal && DefaultValue is not null)
            {
                printer.Write(DefaultValue);
            }
        }
    }
}
