================================================================================
EXTRAS-README: CodeBrix.Templating
Samples, tools and other content in this repository that is not part of a NuGet
package
================================================================================

This repository ships no sample applications, demo apps, benchmarks or build
tools. Everything under src/ goes into the single NuGet package
(CodeBrix.Templating.BsdLicenseForever); the only non-package content is the
test project and its data files, described below.

For consumer documentation see AGENT-README.txt; for build, test and packaging
notes see MAINTAINER-README.txt.


TEST PROJECT
============

    Path   tests/CodeBrix.Templating.Tests/
    What   The xunit.v3 test suite for the library. It has a ProjectReference
           to src/CodeBrix.Templating and sees the library internals through
           src/CodeBrix.Templating/InternalsVisibleTo.cs.
    Run    dotnet test CodeBrix.Templating.slnx
           (no environment variables, opt-in flags or external services)
    Shows  Every public feature of the library end to end -- parsing, the
           lexer, the runtime and ScriptObject imports, the built-in
           functions, async rendering, template loaders and `include`, the
           visitor/rewriter, the formatter, exception behaviour, the JSON
           bridge and IQueryable models. It is the best worked example set for
           the package; AGENT-README.txt links the individual files under
           "WORKING EXAMPLES ON GITHUB".

    Support files inside the project that are examples rather than tests:
        CustomTemplateLoader.cs    two ITemplateLoader implementations (a
                                   Scriban one and a Liquid one) that serve
                                   template text from a switch statement
        MathObject.cs              a static class imported into a template as
                                   a function table
        SpecialFunctionProvider.cs static methods with optional and `params`
                                   arguments, used to exercise argument
                                   binding
        TestFilesHelper.cs         discovers the TestFiles pairs
        TextAssert.cs              a diffing string comparison helper


TEMPLATE FIXTURE FILES
======================

    Path   tests/CodeBrix.Templating.Tests/TestFiles/
    What   Template / expected-output pairs. Each `<name>.txt` is a template
           and `<name>.out.txt` is exactly what it must render to, byte for
           byte. Files whose name contains "error" capture the expected error
           message instead.
    Run    They execute as part of `dotnet test`; TestFilesHelper enumerates
           them, so dropping a new pair into one of the listed folders adds a
           test case with no code change.
    Shows  The authoritative specification of the template language:

        000-basic/          raw text, comments, whitespace control, indentation
        010-literals/       every literal form (strings, verbatim strings,
                            integers, hex, binary, floats, booleans, null)
        020-interpolation/  interpolated strings, including nested ones
        100-expressions/    operators, ranges, member and indexer access,
                            array and object initializers, assignment forms
        200-statements/     if/else, case/when, for, tablerow, while, capture,
                            import, readonly, with, ret
        300-functions/      func declarations, pipes, named/optional/variadic
                            arguments, wrap
        400-builtins/       one file per builtin table, plus 400-builtins.txt
                            which prints the complete list of registered
                            builtin names
        500-liquid/         Liquid tags, filters, raw and comment blocks
        600-ast/            AST round-tripping

    Note   The TestFiles folder is read from the source tree at test time (the
           helper walks up from the test assembly), so the files are not
           copied to the build output.


LIQUID COMPATIBILITY TESTS
==========================

    Path   tests/CodeBrix.Templating.Tests/LiquidTests/
    What   Liquid behaviour tests adapted from the DotLiquid project
           (IfElseTests, IncludeTagTests, RawTests, StandardTagTests,
           UnlessElseTests, an Html fixture folder and a Helper).
    Run    Part of `dotnet test`.
    Legal  These files carry DotLiquid's dual Apache-2.0 / Ms-PL licence.
           readme.md in that folder explains the origin and embeds the licence
           text, and dotliquid-license.txt holds the authoritative copy. Keep
           both with the tests, and keep the matching entry in
           THIRD-PARTY-NOTICES.txt at the repository root.


JSON BRIDGE TESTS
=================

    Path   tests/CodeBrix.Templating.Tests/TestJsonSupport/
    What   Tests for the System.Text.Json integration: importing a JsonElement
           into a ScriptObject or ScriptArray, object.from_json /
           object.to_json, models that expose JsonElement members, and type
           round-tripping.
    Run    Part of `dotnet test`.


REFERENCED BUT NOT PRESENT
==========================

    tests/CodeBrix.Templating.Tests.Aot/
        TestAot.AotPublishProducesNoWarnings expects a NativeAOT publish
        project at this path and calls Assert.Skip when it is missing. The
        project is not in this repository, so that test always skips. Adding
        it would turn on an AOT-warning gate over the library.


END OF EXTRAS-README
