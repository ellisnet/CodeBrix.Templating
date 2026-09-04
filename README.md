# CodeBrix.Templating

A fully managed, cross-platform text-templating and scripting language
library for .NET. CodeBrix.Templating parses and renders templates in the
Scriban and Liquid template languages, suitable for code generation, HTML
pages, reports, configuration files, and any other text produced from a
model. It is provided as a .NET 10 library and the
`CodeBrix.Templating.BsdLicenseForever` NuGet package.

CodeBrix.Templating supports applications and assemblies that target Microsoft .NET version 10.0 and later.
Microsoft .NET version 10.0 is a Long-Term Supported (LTS) version of .NET, and was released on Nov 11, 2025; and will be actively supported by Microsoft until Nov 14, 2028.
Please update your C#/.NET code and projects to the latest LTS version of Microsoft .NET.

## Installation

```
dotnet add package CodeBrix.Templating.BsdLicenseForever
```

Note that the NuGet package ID and the namespace are different - there is no package named plain `CodeBrix.Templating`:

* NuGet package ID: `CodeBrix.Templating.BsdLicenseForever`
* Assembly and primary namespace: `CodeBrix.Templating` - i.e. `using CodeBrix.Templating;`

XML documentation (IntelliSense) ships alongside the assembly. The package has no NuGet dependencies of its own.

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

## Documentation

The NuGet package includes `AGENT-README.txt`, a complete API reference and usage guide written for AI coding agents - point your agent at that file when it is writing code against this library. It also documents the template-language syntax the engine accepts.

Additional sample code and usage examples are available in the `CodeBrix.Templating.Tests` project:
https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests

## License

CodeBrix.Templating is licensed under the BSD 2-Clause License - see the
[LICENSE](https://github.com/ellisnet/CodeBrix.Templating/blob/main/LICENSE) file.

For licensing and provenance information about the open source code included in
this package, see [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Templating/blob/main/THIRD-PARTY-NOTICES.txt).
