// netstandard2.0 compatibility shim - compiled ONLY for the netstandard2.0
// target (see the Compile Remove item in CodeBrix.Templating.csproj).
//
// The C# compiler requires System.Runtime.CompilerServices.IsExternalInit to
// exist before it will emit an "init"-only property setter (LexerOptions and
// ParserOptions use them). netstandard2.0's reference assemblies predate the
// type, so it is declared here.
//
// It is declared as an internal shim rather than pulled in from a package
// (PolySharp, IsExternalInit, ...) so the netstandard2.0 build takes no
// third-party dependency.
//
// Nothing references this type at runtime - it exists purely so the compiler
// has a type to emit a modreq against.

namespace System.Runtime.CompilerServices; //netstandard2.0 shim; not a CodeBrix namespace

/// <summary>
/// Marker type the C# compiler requires in order to emit <c>init</c>-only
/// property setters.
/// </summary>
internal static class IsExternalInit
{
}
