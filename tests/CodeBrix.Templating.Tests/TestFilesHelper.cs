// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeBrix.Templating.Tests; //was previously: Scriban.Tests;

static class TestFilesHelper
{
    public const string InputFilePattern = "*.txt";
    public const string OutputEndFileExtension = ".out.txt";

    public static string BaseDirectory => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestFiles"));

    public static IEnumerable<object[]> ListTestFilesInFolder(string folder)
    {
        var baseDir = BaseDirectory;
        foreach (var file in
            Directory.GetFiles(Path.Combine(baseDir, folder), InputFilePattern, SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(OutputEndFileExtension))
                .Select(f => f.StartsWith(baseDir) ? f.Substring(baseDir.Length + 1) : f)
                .OrderBy(f => f))
        {
            yield return new object[] { file };
        }
    }

    public static IEnumerable<object[]> ListAllTestFiles()
    {
        var folders = new[]
        {
            "000-basic",
            "010-literals",
            "020-interpolation",
            "100-expressions",
            "200-statements",
            "300-functions",
            "400-builtins",
            "500-liquid"
        };

        foreach (var folder in folders)
        {
            foreach (var testCaseData in ListTestFilesInFolder(folder))
            {
                yield return testCaseData;
            }
        }
    }

    public static string LoadTestFile(string inputName)
    {
        var baseDir = BaseDirectory;
        var inputFile = Path.Combine(baseDir, inputName);
        if (!File.Exists(inputFile))
            return null;
        var templateSource = File.ReadAllText(inputFile);
        // The test data files are authored with LF line endings (matching the upstream
        // Scriban test corpus and the line endings the CodeBrix.Templating library
        // produces in its rendered output and error messages). Git checkouts on
        // Windows with `core.autocrlf=true` rewrite them to CRLF on disk, which
        // would otherwise leak CRLF into both the input templates (affecting parser
        // column counts and error message wording) and the expected `.out.txt`
        // contents. Normalize on load so tests behave the same regardless of how
        // the working tree was checked out.
        return templateSource.Replace("\r\n", "\n");
    }

    // Helper methods for MemberData - wrapping ListTestFilesInFolder with specific folder
    public static IEnumerable<object[]> ListTestFiles_000_basic() => ListTestFilesInFolder("000-basic");
    public static IEnumerable<object[]> ListTestFiles_010_literals() => ListTestFilesInFolder("010-literals");
    public static IEnumerable<object[]> ListTestFiles_020_interpolation() => ListTestFilesInFolder("020-interpolation");
    public static IEnumerable<object[]> ListTestFiles_100_expressions() => ListTestFilesInFolder("100-expressions");
    public static IEnumerable<object[]> ListTestFiles_200_statements() => ListTestFilesInFolder("200-statements");
    public static IEnumerable<object[]> ListTestFiles_300_functions() => ListTestFilesInFolder("300-functions");
    public static IEnumerable<object[]> ListTestFiles_400_builtins() => ListTestFilesInFolder("400-builtins");
    public static IEnumerable<object[]> ListTestFiles_500_liquid() => ListTestFilesInFolder("500-liquid");
    public static IEnumerable<object[]> ListTestFiles_600_ast() => ListTestFilesInFolder("600-ast");

    public static IEnumerable<object[]> ListBuiltinFunctionTests(string name) => ListTestFilesInFolder(Path.Combine("400-builtins", name));
    public static IEnumerable<object[]> ListBuiltinFunctionTests_array() => ListBuiltinFunctionTests("array");
    public static IEnumerable<object[]> ListBuiltinFunctionTests_date() => ListBuiltinFunctionTests("date");
    public static IEnumerable<object[]> ListBuiltinFunctionTests_html() => ListBuiltinFunctionTests("html");
    public static IEnumerable<object[]> ListBuiltinFunctionTests_math() => ListBuiltinFunctionTests("math");
    public static IEnumerable<object[]> ListBuiltinFunctionTests_object() => ListBuiltinFunctionTests("object");
    public static IEnumerable<object[]> ListBuiltinFunctionTests_regex() => ListBuiltinFunctionTests("regex");
    public static IEnumerable<object[]> ListBuiltinFunctionTests_string() => ListBuiltinFunctionTests("string");
    public static IEnumerable<object[]> ListBuiltinFunctionTests_timespan() => ListBuiltinFunctionTests("timespan");
}
