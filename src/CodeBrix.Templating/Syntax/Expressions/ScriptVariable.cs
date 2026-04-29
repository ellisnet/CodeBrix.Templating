// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CodeBrix.Templating.Syntax; //was previously: Scriban.Syntax;

/// <summary>
/// A script variable
/// </summary>
/// <remarks>This class is immutable as all variable object are being shared across all templates</remarks>
[ScriptSyntax("variable", "<variable_name>")]
public
abstract partial class ScriptVariable : ScriptExpression, IScriptVariablePath, IEquatable<ScriptVariable>, IScriptTerminal
{
    private int _hashCode;
    /// <summary><c>Arguments</c>.</summary>
    public static readonly ScriptVariableLocal Arguments = new ScriptVariableLocal(string.Empty);
    /// <summary><c>BlockDelegate</c>.</summary>
    public static readonly ScriptVariableLocal BlockDelegate = new ScriptVariableLocal("$");
    /// <summary><c>Continue</c>.</summary>
    public static readonly ScriptVariableLocal Continue = new ScriptVariableLocal("continue"); // Used by liquid offset:continue
    /// <summary><c>ForObject</c>.</summary>
    public static readonly ScriptVariableGlobal ForObject = new ScriptVariableGlobal("for");
    /// <summary><c>TablerowObject</c>.</summary>
    public static readonly ScriptVariableGlobal TablerowObject = new ScriptVariableGlobal("tablerow");
    /// <summary><c>WhileObject</c>.</summary>
    public static readonly ScriptVariableGlobal WhileObject = new ScriptVariableGlobal("while");
    /// <summary><c>ScriptVariable</c>.</summary>
    protected ScriptVariable(string name, ScriptVariableScope scope)
    {
        BaseName = name;
        Scope = scope;
        Trivias = new ScriptTrivias();
        switch (scope)
        {
            case ScriptVariableScope.Global:
                Name = name;
                break;
            case ScriptVariableScope.Local:
                Name = $"${name}";
                break;
            default:
                throw new InvalidOperationException($"Scope `{scope}` is not supported");
        }
        unchecked
        {
            _hashCode = (BaseName.GetHashCode() * 397) ^ (int)Scope;
        }
    }
    /// <summary><c>Trivias</c>.</summary>
    public ScriptTrivias Trivias { get; set; }

    /// <summary>
    /// Creates a <see cref="ScriptVariable"/> according to the specified name and <see cref="ScriptVariableScope"/>
    /// </summary>
    /// <param name="name">Name of the variable</param>
    /// <param name="scope">Scope of the variable</param>
    /// <returns>The script variable</returns>
    public static ScriptVariable Create(string name, ScriptVariableScope scope)
    {
        switch (scope)
        {
            case ScriptVariableScope.Global:
                return new ScriptVariableGlobal(name);
            case ScriptVariableScope.Local:
                return new ScriptVariableLocal(name);
            default:
                throw new InvalidOperationException($"Scope `{scope}` is not supported");
        }
    }
    /// <summary><c>BaseName</c>.</summary>
    public string BaseName { get; }

    /// <summary>
    /// Gets or sets the name of the variable (without the $ sign for local variable)
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets a boolean indicating whether this variable is a local variable (starting with $ in the template ) or global.
    /// </summary>
    public ScriptVariableScope Scope { get; }
    /// <summary><c>GetFirstPath</c>.</summary>
    public string GetFirstPath()
    {
        return Name;
    }

#if !SCRIBAN_NO_ASYNC
    /// <summary><c>SetValueAsync</c>.</summary>
    public ValueTask SetValueAsync(TemplateContext context, object valueToSet)
    {
        return context.SetValueAsync(this, valueToSet);
    }
#endif
    /// <summary><c>Equals</c>.</summary>
    public virtual bool Equals(ScriptVariable other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(Name, other.Name) && Scope == other.Scope;
    }
    /// <summary><c>Equals</c>.</summary>
    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj is ScriptVariable && Equals((ScriptVariable) obj);
    }
    /// <summary><c>GetHashCode</c>.</summary>
    public override int GetHashCode()
    {
        return _hashCode;
    }
    /// <summary><c>operator ==</c>.</summary>
    public static bool operator ==(ScriptVariable left, ScriptVariable right)
    {
        return Equals(left, right);
    }
    /// <summary><c>operator !=</c>.</summary>
    public static bool operator !=(ScriptVariable left, ScriptVariable right)
    {
        return !Equals(left, right);
    }
    /// <summary><c>Evaluate</c>.</summary>
    public override object Evaluate(TemplateContext context)
    {
        return context.GetValue((ScriptExpression)this);
    }
    /// <summary><c>GetValue</c>.</summary>
    public virtual object GetValue(TemplateContext context)
    {
        return context.GetValue(this);
    }
    /// <summary><c>SetValue</c>.</summary>
    public void SetValue(TemplateContext context, object valueToSet)
    {
        context.SetValue(this, valueToSet);
    }
    /// <summary><c>PrintTo</c>.</summary>
    public override void PrintTo(ScriptPrinter printer)
    {
        printer.Write(Name);
    }
}
/// <summary><c>ScriptVariableGlobal</c>.</summary>
public
partial class ScriptVariableGlobal : ScriptVariable
{
    /// <summary><c>ScriptVariableGlobal</c>.</summary>
    public ScriptVariableGlobal(string name) : base(name, ScriptVariableScope.Global)
    {
    }
    /// <summary><c>GetValue</c>.</summary>
    public override object GetValue(TemplateContext context)
    {
        // Used a specialized overrides on contxet for ScriptVariableGlobal
        return context.GetValue(this);
    }
}
/// <summary><c>ScriptVariableLocal</c>.</summary>
public
partial class ScriptVariableLocal : ScriptVariable
{
    /// <summary><c>ScriptVariableLocal</c>.</summary>
    public ScriptVariableLocal(string name) : base(name, ScriptVariableScope.Local)
    {
    }
}
