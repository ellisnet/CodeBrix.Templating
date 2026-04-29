# CodeBrix.Templating

A fully managed, cross-platform text-templating and scripting language
library for .NET. CodeBrix.Templating parses and renders templates in the
Scriban and Liquid template languages, suitable for code generation, HTML
pages, reports, configuration files, and any other text produced from a
model. It is provided as a .NET 10 library and the
`CodeBrix.Templating.BsdLicenseForever` NuGet package.

CodeBrix.Templating supports applications and assemblies that target
Microsoft .NET version 10.0 and later. Microsoft .NET version 10.0 is a
Long-Term Supported (LTS) version of .NET, released on Nov 11, 2025 and
supported by Microsoft until Nov 14, 2028. Please update your C#/.NET
code and projects to the latest LTS version of Microsoft .NET.

CodeBrix.Templating is a fork of the open-source
[Scriban](https://github.com/scriban/scriban) library (v7.1.0) -- see
below for licensing details.

## CodeBrix.Templating supports:

* Scriban template language (default) with a rich built-in function library
* Liquid template language (compatible subset)
* Synchronous and asynchronous rendering
* Safe evaluation with configurable member access, recursion, and
  execution limits
* Pluggable template loaders for `include`
* JSON bridge via `System.Text.Json` (`object.from_json`, `object.to_json`)
* Queryable model objects via reflection, dictionaries, and
  `IScriptObject`
* Template parsing, rewriting, formatting, and AST visitation
* AOT-friendly operation (no runtime code generation required for the
  core engine)

## Installation

```
dotnet add package CodeBrix.Templating.BsdLicenseForever
```

The NuGet package name is `CodeBrix.Templating.BsdLicenseForever`. The
root namespace and assembly name are both `CodeBrix.Templating`.

## Sample Code

### Render a Scriban template

```csharp
using CodeBrix.Templating;

var template = Template.Parse("Hello {{ name }}!");
var result = template.Render(new { name = "World" });
// result == "Hello World!"
```

### Render asynchronously

```csharp
using CodeBrix.Templating;

var template = Template.Parse("Items: {{ for item in items }}{{ item }} {{ end }}");
var result = await template.RenderAsync(new { items = new[] { "a", "b", "c" } });
// result == "Items: a b c "
```

### Use the Liquid template language

```csharp
using CodeBrix.Templating;
using CodeBrix.Templating.Parsing;

var template = Template.ParseLiquid("Hello {{ name }}!");
var result = template.Render(new { name = "World" });
```

### Share state across renders via TemplateContext

```csharp
using CodeBrix.Templating;
using CodeBrix.Templating.Runtime;

var globals = new ScriptObject();
globals.Import(new { project = "CodeBrix.Templating" });

var context = new TemplateContext();
context.PushGlobal(globals);

var template = Template.Parse("Welcome to {{ project }}.");
var result = template.Render(context);
```

Additional sample code and usage examples are in the
`CodeBrix.Templating.Tests` project. Scriban language documentation is at
[scriban/doc](https://github.com/scriban/scriban/tree/master/doc).

## License

The project is licensed under the BSD 2-Clause License. See
[the BSD-2-Clause Wikipedia article](https://en.wikipedia.org/wiki/BSD_licenses#2-clause_license_%28%22Simplified_BSD_License%22_or_%22FreeBSD_License%22%29)
for a brief overview.

All code originating from Scriban v7.1.0 was included under the BSD
2-Clause License, the same license used by this project; per-file
upstream copyright notices are preserved verbatim. This project
(CodeBrix.Templating) complies with all provisions of the source code
license of Scriban v7.1.0 (BSD 2-Clause License).

Parts of the Liquid test suite were adapted from the
[DotLiquid](https://github.com/dotliquid/dotliquid) project, which is
dual-licensed under Apache-2.0 / MS-PL; see `THIRD-PARTY-NOTICES.txt`
for the full attribution.
