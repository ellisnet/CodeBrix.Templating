================================================================================
MAINTAINER-README: CodeBrix.Templating
Notes for people and agents MAINTAINING this repository — not for package
consumers
================================================================================

If you are CONSUMING the NuGet package, read AGENT-README.txt instead. This
file is only about working on the repository itself.


PURPOSE AND SCOPE
=================

The repository produces exactly one NuGet package:

    PackageId    CodeBrix.Templating.BsdLicenseForever
    Project      src/CodeBrix.Templating/CodeBrix.Templating.csproj
    Assembly     CodeBrix.Templating
    Namespace    CodeBrix.Templating
    License      BSD-2-Clause
    Docs         AGENT-README.txt (repository root)

There are no sibling packages, no native assets and no RID-specific outputs.
The library is a text-templating and scripting engine for the Scriban and
Liquid template languages.


REPOSITORY LAYOUT
=================

    /
      AGENT-README.txt            consumer documentation (shipped in the nupkg)
      MAINTAINER-README.txt       this file
      EXTRAS-README.txt           non-package content in the repository
      README-INDEX.txt            map of the README files
      README.md                   human-facing overview (shipped in the nupkg)
      LICENSE                     BSD-2-Clause
      THIRD-PARTY-NOTICES.txt     upstream attributions (shipped in the nupkg)
      icon-codebrix-128.png       package icon (shipped in the nupkg)
      CodeBrix.Templating.slnx    solution
      .gitignore
      AGENTS.md, CLAUDE.md, .clinerules, .cursorrules, .windsurfrules,
      .cursor/rules/agent-readme.mdc, .github/copilot-instructions.md,
      .junie/guidelines.md        AI-agent pointer stubs (do not hand-edit;
                                  they are maintained centrally)

      src/CodeBrix.Templating/
        CodeBrix.Templating.csproj
        Template.cs               the Template entry-point type
        Templating.cs             partial Template/TemplateContext: the async
                                  surface (EvaluateAsync / RenderAsync)
        TemplateContext.cs        evaluation state
        TemplateContext.Helpers.cs      partial: conversions and helpers
        TemplateContext.Variables.cs    partial: variable/scope stacks
        TemplatingFunctions.cs    partial IncludeFunction / IncludeJoinFunction:
                                  their async InvokeAsync implementations
        TemplatingRuntime.cs      partial DynamicCustomFunction and
                                  ScriptOutputExtensions async members
        TemplatingSyntax.cs       partial AST nodes: EvaluateAsync overrides
        TemplatingVisitors.cs     partial AST nodes: ChildrenCount,
                                  GetChildrenImpl and Accept plumbing
        LogMessageBag.cs          parse-message collector
        ScriptPrinter.cs          AST -> text
        ScriptPrinterOptions.cs
        InternalsVisibleTo.cs     grants internals to CodeBrix.Templating.Tests
        Functions/                built-in function tables (Array, String,
                                  Math, DateTime, TimeSpan, Object, Html,
                                  Regex, Include, IncludeJoin, Builtin,
                                  LiquidBuiltins)
        Helpers/                  low-level utilities. BoxHelper, FastStack,
                                  InlineList and ThrowHelper are internal;
                                  CharHelper and ReflectionHelper are public
                                  (part of the shipped surface -- changing
                                  them is a breaking change)
        Parsing/                  Lexer, Parser (+ .Expressions, .Statements,
                                  .Statements.Liquid, .Statements.Templating,
                                  .Terminals partials), options, tokens,
                                  SourceSpan/TextPosition, LogMessage
        Runtime/                  ScriptObject and friends, accessors, custom
                                  functions, output sinks, loaders
        Runtime/Accessors/        ArrayAccessor, DictionaryAccessor,
                                  ListAccessor, NullAccessor,
                                  PrimitiveAccessor, ScriptObjectAccessor,
                                  StringAccessor, TypedObjectAccessor
        Syntax/                   AST base types, visitors, rewriter,
                                  formatter, exceptions, trivia
        Syntax/Expressions/       expression nodes
        Syntax/Statements/        statement nodes

      tests/CodeBrix.Templating.Tests/
        CodeBrix.Templating.Tests.csproj
        TestParser.cs, TestLexer.cs, TestRuntime.cs, TestFunctions.cs,
        TestIncludes.cs, TestAsync.cs, TestFormatter.cs, TestQueryable.cs,
        TestStringSlice.cs, TestAot.cs, ScriptRewriterTests.cs,
        ScriptRuntimeExceptionTests.cs
        TestFilesHelper.cs, TextAssert.cs, CustomTemplateLoader.cs,
        MathObject.cs, SpecialFunctionProvider.cs
        TestFiles/                template/expected-output pairs
        TestJsonSupport/          System.Text.Json bridge tests
        LiquidTests/              Liquid compatibility tests adapted from
                                  DotLiquid, plus readme.md and
                                  dotliquid-license.txt

THERE ARE NO GENERATED FILES IN THIS REPOSITORY. The only file whose name
contains ".Generated." is Runtime/CustomFunction.Generated.cs; nothing in this
repository regenerates it. Treat the name as historical and edit it by hand,
the same as every other source file. (Earlier revisions of the consumer
documentation described TemplatingAsync.generated.cs and
TemplatingVisitors.generated.cs -- no such files exist; the async and visitor
partials are Templating.cs, TemplatingSyntax.cs and TemplatingVisitors.cs.)


BUILDING
========

    dotnet restore CodeBrix.Templating.slnx
    dotnet build CodeBrix.Templating.slnx

The library targets net10.0 only. GenerateDocumentationFile is enabled, so a
missing XML doc comment on a public or protected-on-unsealed member is a
CS1591 warning -- fix it at the source rather than suppressing it.

GeneratePackageOnBuild is true for the library project, so every successful
build of src/CodeBrix.Templating also produces a .nupkg in its bin folder.


TESTING
=======

    dotnet test CodeBrix.Templating.slnx

The test project uses xunit.v3 with the Visual Studio runner, coverlet for
coverage, SilverAssertions for fluent assertions, and Newtonsoft.Json for a
few JSON comparison helpers. It has a ProjectReference to the library (not a
PackageReference) and sees the library internals through
src/CodeBrix.Templating/InternalsVisibleTo.cs.

No environment variables, opt-in flags or external services are required.

TestFilesHelper resolves tests/CodeBrix.Templating.Tests/TestFiles relative to
the built test assembly, so the .txt fixtures are read from the source tree
and do not need to be copied to the output directory. Each `<name>.txt` is a
template and `<name>.out.txt` is the byte-exact expected output; adding a pair
adds a test case automatically for the folders listed in
TestFilesHelper.ListAllTestFiles.

TestAot.AotPublishProducesNoWarnings looks for a sibling project at
tests/CodeBrix.Templating.Tests.Aot/CodeBrix.Templating.Tests.Aot.csproj and
calls Assert.Skip when it is absent. That project is NOT part of this
repository today, so the test always skips. If you add it, the test runs
`dotnet publish -c Release` on it and fails on any IL/AOT warning.


PACKAGING AND PUBLISHING
========================

Packing is driven entirely by src/CodeBrix.Templating/CodeBrix.Templating.csproj
-- there is no separate pack script or nuspec.

Version scheme (date-stamped, computed from System.DateTime.UtcNow at build
time, documented in a comment block in the csproj):

    1.<x>.<y>.<z>
      1  major     always 1 for this library
      x  minor     whole years since the _VersionBaseYear property
      y  build     UTC day of year, 1-based
      z  revision  UTC minute of day, 0..1439

The value is strictly increasing over time. Two builds within the same UTC
minute produce the same version, so never publish two packages from within one
minute. This is not SemVer: major/minor carry no API-compatibility meaning.
Re-baseline by changing _VersionBaseYear.

Files packed into the nupkg (from the ItemGroup at the end of the csproj):

    icon-codebrix-128.png     PackageIcon
    README.md                 PackageReadmeFile
    AGENT-README.txt          consumer documentation
    THIRD-PARTY-NOTICES.txt   upstream attributions

MAINTAINER-README.txt, EXTRAS-README.txt and README-INDEX.txt are repository
documentation and are deliberately NOT packed.

Other package metadata worth knowing: PackageLicenseExpression is BSD-2-Clause,
PackageRequireLicenseAcceptance is true, and the project/repository URLs point
at https://github.com/ellisnet/CodeBrix.Templating.


PROVENANCE AND VENDORED SOURCES
===============================

CodeBrix.Templating is a port of Scriban 7.1.0 (BSD-2-Clause), the same
license this project uses.

    * Every ported .cs file keeps its upstream copyright header verbatim:
        // Copyright (c) Alexandre Mutel. All rights reserved.
        // Licensed under the BSD-Clause 2 license.
        // See license.txt file in the project root for full license information.
    * Every ported file's namespace line carries a provenance comment:
        namespace CodeBrix.Templating.<Sub>; //was previously: Scriban.<Sub>;
      Keep these comments so the port stays auditable against the upstream
      source. The same convention is used in the test project
      (//was previously: Scriban.Tests;).
    * Conditional compilation symbols inherited from upstream (for example
      SCRIBAN_NO_SYSTEM_TEXT_JSON) are left in place but are not defined by
      this build; System.Text.Json support is always compiled in.
    * Parts of the Liquid test suite were adapted from DotLiquid
      (Apache-2.0 / Ms-PL dual licence). See THIRD-PARTY-NOTICES.txt and
      tests/CodeBrix.Templating.Tests/LiquidTests/dotliquid-license.txt.

Upstream terminology such as "scriban" survives in XML doc comments that were
ported verbatim; that is intentional and is part of the audit trail.


CODING CONVENTIONS
==================

These are repository conventions for contributors (they are not consumer
guidance and must not be repeated in AGENT-README.txt).

    * NULLABLE REFERENCE TYPES ARE OFF, family-wide. Do not annotate reference
      types with `?` (no `string?`, `MyClass?`, `List<int>?`), do not use the
      null-forgiving postfix `!`, and do not add `#nullable enable/disable/
      restore` directives. Value-type `?` (`int?`, `bool?`, `DateTime?`, enum
      `?`) is fine.
    * NO PROJECT-WIDE <NoWarn>. Warnings are fixed at the source. With
      GenerateDocumentationFile enabled that includes writing an XML doc
      comment for every public and protected-on-unsealed member.
    * FILE-SCOPED NAMESPACES everywhere. Block-scoped `namespace X { ... }` is
      forbidden.
    * THE PORT PUTS `public` ON ITS OWN LINE above the type declaration, e.g.

          public
          partial class ArrayFunctions : ScriptObject

      This is a port artifact that appears throughout the codebase. Preserve
      it when editing ported files, and remember that line-anchored greps such
      as `grep -n '^public class'` will MISS these types -- use a
      newline-tolerant search when counting or auditing the public surface.
    * XUNIT V3 FOR EVERY NEW TEST. NUnit is forbidden; if a ported file ever
      reintroduces `using NUnit.Framework;` or
      `using NUnit.Framework.Internal;`, remove it.
    * USE SILVERASSERTIONS in new test assertions (`value.Should().Be(...)`)
      rather than the xUnit built-ins (`Assert.Equal(expected, actual)`).
      Tests carried over from upstream still use the xUnit form and are being
      converted incrementally; do not mass-rewrite them in an unrelated change.
    * TEST FILE NAMING follows the family convention `<Class>Tests.cs` with
      snake_case test method names and //Arrange //Act //Assert comment
      blocks for new tests. The upstream-derived files (TestParser.cs,
      TestRuntime.cs, ...) keep their original names.
    * WHEN ADDING A BUILT-IN FUNCTION, add it as a `public static` member of
      the relevant ScriptObject-derived table in Functions/ -- the table's
      constructor auto-imports the derived type's statics, so no registration
      code is needed. Mark helper overloads that must not be exposed with
      [ScriptMemberIgnore], and add the new name to the expected output of
      tests/CodeBrix.Templating.Tests/TestFiles/400-builtins/400-builtins.out.txt,
      which enumerates every registered builtin name.


NOTES
=====

    * The AST is large (roughly 100 public types under Syntax/). A change
      to a node usually needs matching edits in three places: the node file
      itself, the async EvaluateAsync partial in TemplatingSyntax.cs, and the
      children/visitor plumbing in TemplatingVisitors.cs.
    * Keep the synchronous and asynchronous surfaces in step. Anything added
      to Template/TemplateContext/an AST node synchronously almost always
      needs an async twin in Templating.cs or TemplatingSyntax.cs.
    * Liquid support has two halves that must stay in sync:
      LiquidBuiltinsFunctions.TryLiquidToScriban (the name map used when
      ParserOptions.LiquidFunctionsToScriban is set) and the DefaultBuiltins
      constructor in the same file (the aliases actually registered). They are
      NOT identical today -- `escape_once` is in the map but its registration
      is commented out because HtmlFunctions has no escape_once member. Fix
      the two together if you add the function.
    * tests/CodeBrix.Templating.Tests/LiquidTests/readme.md records that those
      tests came from DotLiquid and embeds the DotLiquid dual licence; keep it
      with those files and keep the matching entry in THIRD-PARTY-NOTICES.txt
      if the folder is moved or renamed.
    * The engine performs no runtime code generation, which is what makes it
      trim- and AOT-safe. Reflection-based entry points are annotated with
      [RequiresUnreferencedCode]; keep that annotation on any new
      reflection-based API rather than suppressing the warning.


END OF MAINTAINER-README
