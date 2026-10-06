// netstandard2.0 compatibility shim - compiled ONLY for the netstandard2.0
// target (see the Compile Remove item in CodeBrix.Templating.csproj).
//
// These attributes annotate code for the IL trimmer and Native AOT. They live
// in the BCL from .NET 5 / .NET 7 onward, but netstandard2.0's reference
// assemblies predate them. A netstandard2.0 build is never trimmed or AOT
// compiled, so on that target they are inert metadata; they are declared only
// so the shared source compiles unchanged for both targets. (System.Text.Json
// carries its own copies, but those are internal to that assembly and cannot
// be used from here.)
//
// They are declared as internal shims rather than pulled in from a package
// (PolySharp, ...) so the netstandard2.0 build takes no third-party dependency.

namespace System.Diagnostics.CodeAnalysis; //netstandard2.0 shim; not a CodeBrix namespace

/// <summary>
/// Indicates that the annotated member requires dynamic access to code that is
/// not referenced statically, so it may break when the application is trimmed.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false)]
internal sealed class RequiresUnreferencedCodeAttribute : Attribute
{
    /// <summary>
    /// Initializes the attribute with the specified message.
    /// </summary>
    /// <param name="message">Why the member is not trim-compatible.</param>
    public RequiresUnreferencedCodeAttribute(string message) => Message = message;

    /// <summary>
    /// Gets why the member is not trim-compatible.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets or sets an optional URL with more information.
    /// </summary>
    public string Url { get; set; }
}

/// <summary>
/// Indicates that the annotated member requires the ability to generate new
/// code at runtime, so it may not work under Native AOT.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false)]
internal sealed class RequiresDynamicCodeAttribute : Attribute
{
    /// <summary>
    /// Initializes the attribute with the specified message.
    /// </summary>
    /// <param name="message">Why the member is not AOT-compatible.</param>
    public RequiresDynamicCodeAttribute(string message) => Message = message;

    /// <summary>
    /// Gets why the member is not AOT-compatible.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets or sets an optional URL with more information.
    /// </summary>
    public string Url { get; set; }
}

/// <summary>
/// Suppresses reporting of a specific code-analysis rule violation, including
/// in the IL trimmer (unlike <see cref="SuppressMessageAttribute" />).
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
internal sealed class UnconditionalSuppressMessageAttribute : Attribute
{
    /// <summary>
    /// Initializes the attribute with the rule category and identifier.
    /// </summary>
    /// <param name="category">The category of the suppressed rule.</param>
    /// <param name="checkId">The identifier of the suppressed rule.</param>
    public UnconditionalSuppressMessageAttribute(string category, string checkId)
    {
        Category = category;
        CheckId = checkId;
    }

    /// <summary>
    /// Gets the category of the suppressed rule.
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// Gets the identifier of the suppressed rule.
    /// </summary>
    public string CheckId { get; }

    /// <summary>
    /// Gets or sets the scope of the suppression.
    /// </summary>
    public string Scope { get; set; }

    /// <summary>
    /// Gets or sets the target of the suppression.
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    /// Gets or sets an optional argument expanding on the exclusion criteria.
    /// </summary>
    public string MessageId { get; set; }

    /// <summary>
    /// Gets or sets the justification for the suppression.
    /// </summary>
    public string Justification { get; set; }
}

/// <summary>
/// Indicates that certain members of a <see cref="Type" /> are accessed
/// dynamically, so the trimmer must keep them.
/// </summary>
[AttributeUsage(
    AttributeTargets.Field | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter |
    AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Method |
    AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
    Inherited = false)]
internal sealed class DynamicallyAccessedMembersAttribute : Attribute
{
    /// <summary>
    /// Initializes the attribute with the member types that are accessed.
    /// </summary>
    /// <param name="memberTypes">The member types that are dynamically accessed.</param>
    public DynamicallyAccessedMembersAttribute(DynamicallyAccessedMemberTypes memberTypes) => MemberTypes = memberTypes;

    /// <summary>
    /// Gets the member types that are dynamically accessed.
    /// </summary>
    public DynamicallyAccessedMemberTypes MemberTypes { get; }
}

/// <summary>
/// Specifies the types of members that are dynamically accessed.
/// </summary>
[Flags]
internal enum DynamicallyAccessedMemberTypes
{
    /// <summary>No members.</summary>
    None = 0,
    /// <summary>The default, parameterless public constructor.</summary>
    PublicParameterlessConstructor = 0x0001,
    /// <summary>All public constructors.</summary>
    PublicConstructors = 0x0002 | PublicParameterlessConstructor,
    /// <summary>All non-public constructors.</summary>
    NonPublicConstructors = 0x0004,
    /// <summary>All public methods.</summary>
    PublicMethods = 0x0008,
    /// <summary>All non-public methods.</summary>
    NonPublicMethods = 0x0010,
    /// <summary>All public fields.</summary>
    PublicFields = 0x0020,
    /// <summary>All non-public fields.</summary>
    NonPublicFields = 0x0040,
    /// <summary>All public nested types.</summary>
    PublicNestedTypes = 0x0080,
    /// <summary>All non-public nested types.</summary>
    NonPublicNestedTypes = 0x0100,
    /// <summary>All public properties.</summary>
    PublicProperties = 0x0200,
    /// <summary>All non-public properties.</summary>
    NonPublicProperties = 0x0400,
    /// <summary>All public events.</summary>
    PublicEvents = 0x0800,
    /// <summary>All non-public events.</summary>
    NonPublicEvents = 0x1000,
    /// <summary>All interfaces implemented by the type.</summary>
    Interfaces = 0x2000,
    /// <summary>All members.</summary>
    All = ~None,
}
