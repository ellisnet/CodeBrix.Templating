================================================================================
AGENT-README: CodeBrix.Templating
A Comprehensive Guide for AI Coding Agents
================================================================================

OVERVIEW
--------
CodeBrix.Templating is a text-templating and scripting-language library for
.NET 10.0+. It parses and renders templates written in the Scriban and Liquid
template languages, suitable for code generation, HTML pages, reports,
configuration files, and any other text produced from a model.

CodeBrix.Templating is a fork of the Scriban project (v7.1.0). All namespaces
use "CodeBrix.Templating" instead of "Scriban". Do NOT use Scriban
namespaces -- they do not exist in this library.

Every ported .cs file carries its upstream Scriban BSD-2-Clause copyright
header verbatim, and the namespace line of every ported file carries a
trailing provenance comment of the form:

    namespace CodeBrix.Templating.<Sub>; //was previously: Scriban.<Sub>;

so the port history remains auditable against the upstream Scriban 7.1.0
source.


INSTALLATION
------------
NuGet Package: CodeBrix.Templating.BsdLicenseForever

    dotnet add package CodeBrix.Templating.BsdLicenseForever

IMPORTANT: The NuGet package name is CodeBrix.Templating.BsdLicenseForever
(NOT CodeBrix.Templating). The primary namespace and assembly name are
both CodeBrix.Templating.

Requirements: .NET 10.0 or higher
License: BSD 2-Clause License


KEY NAMESPACES
--------------
    using CodeBrix.Templating;                    // Template, TemplateContext
    using CodeBrix.Templating.Runtime;            // ScriptObject, ScriptArray,
                                                   // ITemplateLoader, etc.
    using CodeBrix.Templating.Parsing;            // ParserOptions, LexerOptions,
                                                   // ScriptLang, ScriptMode
    using CodeBrix.Templating.Syntax;             // AST nodes, ScriptVisitor,
                                                   // ScriptFormatter,
                                                   // ScriptRuntimeException
    using CodeBrix.Templating.Functions;          // Built-in function tables


================================================================================

CORE API REFERENCE
==================

Template CLASS -- MAIN ENTRY POINT
----------------------------------
The Template class is the primary public API for parsing and rendering
templates. It is immutable once parsed and is safe to cache across renders.

Static factory methods:
    Template.Parse(string text, string sourceFilePath = null,
                   ParserOptions parserOptions = null,
                   LexerOptions lexerOptions = null)
        Parse a Scriban template (the default language).

    Template.ParseLiquid(string text, string sourceFilePath = null,
                        ParserOptions parserOptions = null,
                        LexerOptions lexerOptions = null)
        Parse a Liquid template.

Instance members:
    bool HasErrors                                // true if parsing failed
    LogMessageBag Messages                        // parse errors/warnings
    ScriptPage Page                               // parsed AST root
    ParserOptions ParserOptions                   // options used at parse time
    LexerOptions LexerOptions                     // options used at parse time
    string SourceFilePath                         // originating file path

    string Render(object model = null,
                  MemberFilterDelegate memberFilter = null,
                  MemberRenamerDelegate memberRenamer = null)
    string Render(TemplateContext context)
    ValueTask<string> RenderAsync(object model = null,
                                   MemberFilterDelegate memberFilter = null,
                                   MemberRenamerDelegate memberRenamer = null)
    ValueTask<string> RenderAsync(TemplateContext context)
    object Evaluate(TemplateContext context)      // return raw value, no render
    ValueTask<object> EvaluateAsync(TemplateContext context)


TemplateContext CLASS -- EVALUATION STATE
-----------------------------------------
TemplateContext holds the state of a single render pass: the output sink,
the global variable stack, imported functions, limits, culture, and the
template loader used by `include`. Cache contexts across renders when the
globals don't change, to avoid rebuilding import tables.

Key members:
    void PushGlobal(IScriptObject globals)
    IScriptObject PopGlobal()
    IScriptObject CurrentGlobal { get; }

    ITemplateLoader TemplateLoader { get; set; }  // resolves `include`
    IScriptOutput Output { get; set; }            // text sink (default:
                                                   // StringBuilderOutput)
    ScriptLang Language { get; set; }             // Default/Liquid/Scientific

    int LoopLimit { get; set; }                   // default: 1000
    int RecursiveLimit { get; set; }              // default: 100
    int ObjectRecursionLimit { get; set; }        // default: 20
    TimeSpan RegexTimeOut { get; set; }           // default: 10 seconds

    CultureInfo CurrentCulture { get; set; }      // affects date/number formatting
    bool StrictVariables { get; set; }            // throw on missing globals
    bool EnableRelaxedMemberAccess { get; set; }
    bool EnableRelaxedTargetAccess { get; set; }
    bool EnableRelaxedFunctionAccess { get; set; }


ScriptObject CLASS -- MODEL / GLOBAL CONTAINER
----------------------------------------------
ScriptObject is a dictionary-like container that implements IScriptObject
and is the canonical way to expose values to templates.

    void Import(object obj,
                ScriptMemberImportFlags flags = ScriptMemberImportFlags.All,
                MemberFilterDelegate filter = null,
                MemberRenamerDelegate renamer = null)
    void Import(string memberName, Delegate function)
    void Import(Type type, ...)                    // static members of a type
    void Add(string key, object value)
    void SetValue(string member, object value, bool readOnly)
    bool TryGetValue(string member, out object value)
    bool Remove(string member)

Built-in subclasses live in CodeBrix.Templating.Functions:
    BuiltinFunctions           - the aggregate object registered as the
                                  default global set (array, string, object,
                                  math, regex, date, timespan, html, etc.)
    ArrayFunctions, StringFunctions, ObjectFunctions, MathFunctions,
    DateTimeFunctions, TimeSpanFunctions, HtmlFunctions, RegexFunctions,
    IncludeFunction, IncludeJoinFunction, LiquidBuiltinsFunctions


ITemplateLoader INTERFACE -- RESOLVING `include`
------------------------------------------------
Implement this interface to resolve `{{ include "name" }}` at render time.

    string GetPath(TemplateContext context, SourceSpan callerSpan,
                   string templateName)
    string Load(TemplateContext context, SourceSpan callerSpan,
                string templatePath)
    ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan,
                                 string templatePath)

Register the loader via:

    context.TemplateLoader = new MyLoader();


================================================================================

QUICK-START EXAMPLES
====================

Render a Scriban template against an anonymous model:

    using CodeBrix.Templating;

    var template = Template.Parse("Hello {{ name }}!");
    string result = template.Render(new { name = "World" });

Render asynchronously:

    var template = Template.Parse("{{ for item in items }}{{ item }}\n{{ end }}");
    string result = await template.RenderAsync(new { items = new[] { "a", "b" } });

Render a Liquid template:

    var template = Template.ParseLiquid("Hello {{ name }}!");
    string result = template.Render(new { name = "World" });

Reuse a TemplateContext across renders:

    var globals = new ScriptObject();
    globals.Import(new { project = "CodeBrix.Templating", year = 2026 });

    var context = new TemplateContext();
    context.PushGlobal(globals);

    var template = Template.Parse("Welcome to {{ project }} ({{ year }}).");
    string result = template.Render(context);

Import a static class as a function library:

    var scriptObject = new ScriptObject();
    scriptObject.Import(typeof(Math));
    var context = new TemplateContext();
    context.PushGlobal(scriptObject);

    string result = Template.Parse("{{ sqrt 16 }}").Render(context);

Enable strict variable mode (throws on undefined variables):

    var context = new TemplateContext { StrictVariables = true };


================================================================================

COMMON PITFALLS
===============

DO NOT reach for `Scriban.*` namespaces. They do not exist here. Every
ported type lives under CodeBrix.Templating.*.

DO NOT assume NRT (nullable reference types) is on. NRT is OFF
family-wide for CodeBrix libraries. Do NOT add `?` annotations to
reference types (`string?`, `MyClass?`, `List<int>?`), do NOT use the
null-forgiveness operator (`!` as a postfix), and do NOT add `#nullable
enable/disable/restore` directives anywhere. Value-type `?` (`int?`,
`bool?`, `DateTime?`, enum `?`, etc.) is fine.

DO NOT add project-wide `<NoWarn>` suppressions to the csproj. Warnings
are fixed at the source. The library csproj has
`<GenerateDocumentationFile>true</GenerateDocumentationFile>` enabled, so
every public and protected-on-unsealed member must carry an XML doc
comment (CS1591 fires on missing ones).

USE the SilverAssertions fluent form in new test assertions
(`value.Should().Be(...)`), not the xUnit built-in
(`Assert.Equal(expected, actual)`). Legacy tests imported from upstream
Scriban still use the xUnit form and are being converted incrementally.

USE file-scoped namespaces everywhere. Block-scoped `namespace X { ... }`
form is forbidden.

USE xUnit v3 for every new test. NUnit is forbidden; remove any
`using NUnit.Framework[.Internal];` directives encountered in ported
LiquidTests files.

`include` requires a loader. Calling `{{ include "name" }}` without
assigning `TemplateContext.TemplateLoader` throws at render time.


================================================================================

ARCHITECTURAL NOTES
===================

Layout of src/CodeBrix.Templating/:
    Template.cs                   - entry-point Template class
    TemplateContext.cs            - evaluation state
    TemplateContext.Helpers.cs    - partial (helper methods)
    TemplateContext.Variables.cs  - partial (variable stack handling)
    LogMessageBag.cs              - parse-error collector
    ScriptPrinter.cs,
    ScriptPrinterOptions.cs       - re-emits an AST back to text
    InternalsVisibleTo.cs         - exposes internals to .Tests project
    Templating*.cs                - large partial classes carrying the
                                    async surface and the visitor/rewriter
                                    infrastructure (e.g.
                                    TemplatingAsync.generated.cs,
                                    TemplatingVisitors.generated.cs).
                                    The ".generated." portion of the
                                    filename is historical -- nothing in
                                    this repository regenerates them; they
                                    are checked-in source files maintained
                                    by hand.
    Functions/                    - built-in function tables
    Helpers/                      - internal utilities
    Parsing/                      - Lexer + Parser (+ Liquid/Templating
                                    partials) + token types
    Runtime/                      - ScriptObject, ScriptArray,
                                    ITemplateLoader, accessors, output sinks
    Syntax/                       - AST nodes (Expressions/, Statements/),
                                    ScriptFormatter, ScriptRewriter,
                                    ScriptVisitor, ScriptRuntimeException

The async surface (RenderAsync, EvaluateAsync, and the visitor/rewriter
infrastructure) lives in partial classes named Templating*.cs at the
project root. Although two of those files end in ".generated.cs"
(TemplatingAsync.generated.cs and TemplatingVisitors.generated.cs), no
generator in this repository emits them. They are checked-in source
files -- treat the ".generated." suffix as historical naming only and
edit them by hand when the synchronous surface changes.

AOT: the core engine does not require runtime code generation. Reflection
is used at render time to read model members; avoid requiring runtime
IL-emission when embedding CodeBrix.Templating in a trimmed or AOT build.


================================================================================

LICENSE AND ATTRIBUTION
=======================

CodeBrix.Templating is distributed under the BSD 2-Clause License (see
the LICENSE file at the repository root). Every ported .cs file preserves
its upstream Scriban BSD-2-Clause copyright header verbatim.

Upstream attributions and third-party license notices are in
THIRD-PARTY-NOTICES.txt at the repository root.


END OF AGENT-README
