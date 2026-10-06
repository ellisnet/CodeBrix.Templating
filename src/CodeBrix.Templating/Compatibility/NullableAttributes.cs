// netstandard2.0 compatibility shim - compiled ONLY for the netstandard2.0
// target (see the Compile Remove item in CodeBrix.Templating.csproj).
//
// These attributes describe nullability to callers and to the compiler's flow
// analysis. They live in the BCL from netstandard2.1 / .NET Core 3.0 onward,
// but netstandard2.0's reference assemblies predate them. (System.Text.Json
// carries its own copies, but those are internal to that assembly and cannot
// be used from here.)
//
// They are declared as internal shims rather than pulled in from a package
// (PolySharp, ...) so the netstandard2.0 build takes no third-party dependency.

namespace System.Diagnostics.CodeAnalysis; //netstandard2.0 shim; not a CodeBrix namespace

/// <summary>
/// Specifies that <c>null</c> is allowed as an input even if the corresponding
/// type disallows it.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
internal sealed class AllowNullAttribute : Attribute
{
}

/// <summary>
/// Specifies that an output may be <c>null</c> even if the corresponding type
/// disallows it.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
internal sealed class MaybeNullAttribute : Attribute
{
}

/// <summary>
/// Specifies that when a method returns <see cref="ReturnValue" />, the
/// associated parameter will not be <c>null</c> even if the type allows it.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class NotNullWhenAttribute : Attribute
{
    /// <summary>
    /// Initializes the attribute with the specified return-value condition.
    /// </summary>
    /// <param name="returnValue">
    /// The return value condition. If the method returns this value, the
    /// associated parameter will not be <c>null</c>.
    /// </param>
    public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

    /// <summary>
    /// Gets the return value condition.
    /// </summary>
    public bool ReturnValue { get; }
}
