================================================================================
AGENT-README: CodeBrix.Templating
A Guide for AI Coding Agents — CONSUMING the
CodeBrix.Templating.BsdLicenseForever NuGet package
================================================================================


OVERVIEW
========

CodeBrix.Templating is a fully managed text-templating and scripting-language
library. It parses templates written in the Scriban template language (the
default) or the Liquid template language, binds them to a .NET object model,
and renders them to text -- code generation, HTML pages, e-mail bodies,
reports, configuration files, SQL, anything produced from a model.

Target framework: .NET 10 or later.

What you get:

    * A template language with expressions, statements (if/case/for/while),
      user-defined functions, pipes, string interpolation and whitespace
      control.
    * A library of built-in functions covering arrays, strings, math, dates,
      timespans, objects, regular expressions and HTML.
    * Synchronous and asynchronous rendering (Render / RenderAsync).
    * Safe evaluation: loop, recursion, object-recursion, string-length and
      regex-timeout limits, plus opt-in strict-variable mode.
    * Pluggable template loading for the `include` function.
    * A JSON bridge over System.Text.Json (object.from_json / object.to_json,
      and importing a JsonElement into a ScriptObject).
    * A full public AST with visitors, a rewriter, a formatter and a printer,
      so templates can be analysed, transformed and written back to text.

Provenance: CodeBrix.Templating is a port of Scriban 7.1.0. Every type lives
under the CodeBrix.Templating.* namespaces. Do NOT use Scriban.* namespaces --
they do not exist in this library. The template language itself is unchanged,
so Scriban/Liquid template text written for the upstream project parses here.


INSTALLATION
============

PackageId: CodeBrix.Templating.BsdLicenseForever

    dotnet add package CodeBrix.Templating.BsdLicenseForever

IMPORTANT: the package id is CodeBrix.Templating.BsdLicenseForever, NOT
CodeBrix.Templating. The assembly name and the root namespace are both
CodeBrix.Templating.

NuGet dependencies: none. The library uses only the .NET base class library
(System.Text.Json is part of the shared framework on .NET 10).

Native libraries / OS restrictions: none. The library is pure managed code and
runs on every platform .NET 10 supports.

License: BSD-2-Clause.

Third-party attribution for the ported upstream code ships in the package as
THIRD-PARTY-NOTICES.txt.


KEY NAMESPACES / USINGS
=======================

    using CodeBrix.Templating;
        // Template, TemplateContext, LiquidTemplateContext, LogMessageBag,
        // ScriptPrinter, ScriptPrinterOptions

    using CodeBrix.Templating.Runtime;
        // ScriptObject, IScriptObject, ScriptObjectExtensions (the Import
        // extension methods), ScriptArray, ScriptRange, ScriptLazy,
        // EmptyScriptObject, ITemplateLoader, IScriptOutput,
        // StringBuilderOutput, TextWriterOutput, IScriptCustomFunction,
        // DelegateCustomFunction, DelegateCustomAction, DynamicCustomFunction,
        // MemberRenamerDelegate, MemberFilterDelegate, StandardMemberRenamer,
        // ScriptMemberIgnoreAttribute, ScriptMemberImportFlags,
        // IObjectAccessor, IListAccessor and the built-in accessors

    using CodeBrix.Templating.Parsing;
        // ParserOptions, LexerOptions, ScriptLang, ScriptMode, Lexer, Parser,
        // Token, TokenType, SourceSpan, TextPosition, LogMessage,
        // ParserMessageType

    using CodeBrix.Templating.Syntax;
        // ScriptNode and every AST node type, ScriptVisitor, ScriptRewriter,
        // ScriptFormatter, ScriptFormatterOptions, ScriptFormatterFlags,
        // ScriptRuntimeException, ScriptParserRuntimeException,
        // ScriptAbortException, ScriptArgumentException

    using CodeBrix.Templating.Functions;
        // BuiltinFunctions, ArrayFunctions, StringFunctions, MathFunctions,
        // DateTimeFunctions, TimeSpanFunctions, ObjectFunctions,
        // HtmlFunctions, RegexFunctions, IncludeFunction,
        // IncludeJoinFunction, LiquidBuiltinsFunctions

Most consuming code needs only the first two.


CORE API REFERENCE
==================

Template -- THE ENTRY POINT
---------------------------
A parsed Template is immutable and safe to cache and reuse across renders.

Static factory methods:

    static Template Parse(string text,
                          string sourceFilePath = null,
                          ParserOptions parserOptions = null,
                          LexerOptions lexerOptions = null)

    static Template ParseLiquid(string text,
                                string sourceFilePath = null,
                                ParserOptions parserOptions = null,
                                LexerOptions lexerOptions = null)

    static object Evaluate(string expression, TemplateContext context)
    static object Evaluate(string expression, object model,
                           MemberRenamerDelegate memberRenamer = null,
                           MemberFilterDelegate memberFilter = null)
    static ValueTask<object> EvaluateAsync(string expression,
                                           TemplateContext context)
    static ValueTask<object> EvaluateAsync(string expression, object model,
                                           MemberRenamerDelegate memberRenamer = null,
                                           MemberFilterDelegate memberFilter = null)

Instance properties:

    string         SourceFilePath { get; }   // path passed to Parse, or null
    ScriptPage     Page           { get; }   // parsed AST root
    bool           HasErrors      { get; }   // true if parsing failed
    LogMessageBag  Messages       { get; }   // parse errors and warnings
    ParserOptions  ParserOptions  { get; }
    LexerOptions   LexerOptions   { get; }

Instance methods:

    string Render(TemplateContext context)
    string Render(object model = null,
                  MemberRenamerDelegate memberRenamer = null,
                  MemberFilterDelegate memberFilter = null)

    ValueTask<string> RenderAsync(TemplateContext context)
    ValueTask<string> RenderAsync(object model = null,
                                  MemberRenamerDelegate memberRenamer = null,
                                  MemberFilterDelegate memberFilter = null)

    object Evaluate(TemplateContext context)
    object Evaluate(object model = null,
                    MemberRenamerDelegate memberRenamer = null,
                    MemberFilterDelegate memberFilter = null)
    ValueTask<object> EvaluateAsync(TemplateContext context)
    ValueTask<object> EvaluateAsync(object model = null,
                                    MemberRenamerDelegate memberRenamer = null,
                                    MemberFilterDelegate memberFilter = null)

    string ToText(ScriptPrinterOptions options = default)  // AST back to text

PARAMETER ORDER: on Render / RenderAsync / Evaluate / EvaluateAsync the
member RENAMER comes BEFORE the member FILTER. Positional calls that pass a
filter second will not compile. Prefer named arguments:

    template.Render(model, memberRenamer: MyRenamer, memberFilter: MyFilter);

Evaluate returns the value of the last expression instead of the rendered
text; Render writes to the context output and returns the accumulated string.

The overloads that take an `object model` import it by reflection and are
annotated [RequiresUnreferencedCode]; in trimmed or NativeAOT builds use the
TemplateContext overloads with an explicitly populated ScriptObject.


TemplateContext -- EVALUATION STATE
-----------------------------------
TemplateContext owns the output sink, the global/local variable stacks, the
culture stack, the loaded-template cache and every execution limit. Reuse one
context across renders when the globals do not change.

Construction:

    TemplateContext()
    TemplateContext(ScriptObject builtin)
    TemplateContext(IEqualityComparer<string> keyComparer)
    TemplateContext(ScriptObject builtin, IEqualityComparer<string> keyComparer)

    LiquidTemplateContext()      // derived; installs the Liquid builtins

Variables and scopes:

    void          PushGlobal(IScriptObject scriptObject)
    IScriptObject PopGlobal()
    IScriptObject CurrentGlobal { get; }
    int           GlobalCount   { get; }
    void          PushLocal()
    void          PopLocal()
    ScriptObject  BuiltinObject { get; }        // the builtin function table
    void          SetValue(ScriptVariable variable, object value,
                           bool asReadOnly = false)
    object        GetValue(ScriptVariable variable)
    object        GetValue(ScriptExpression target)
    void          SetValue(ScriptExpression target, object value)
    void          DeleteValue(ScriptVariable variable)
    void          SetReadOnly(ScriptVariable variable, bool isReadOnly = true)
    void          Import(SourceSpan span, object objectToImport)
    static ScriptObject GetDefaultBuiltinObject()

Output:

    IScriptOutput  Output { get; }              // READ-ONLY property
    void           PushOutput()                 // pushes a StringBuilderOutput
    void           PushOutput(IScriptOutput output)
    IScriptOutput  PopOutput()
    int            OutputCount { get; }
    bool           EnableOutput { get; set; }   // default true
    TemplateContext Write(string text)
    TemplateContext Write(SourceSpan span, object textAsObject)
    TemplateContext Write(ScriptStringSlice slice)
    TemplateContext Write(string text, int startIndex, int count)
    TemplateContext WriteLine()
    string          NewLine { get; set; }       // default Environment.NewLine

To render into a TextWriter, push a TextWriterOutput -- do not assign Output.

Culture:

    CultureInfo CurrentCulture { get; }         // READ-ONLY; default is
                                                // CultureInfo.InvariantCulture
    void        PushCulture(CultureInfo culture)
    CultureInfo PopCulture()
    int         CultureCount { get; }

Limits and safety (defaults as shipped):

    int      LoopLimit            { get; set; }   // 1000 iterations per loop
    int?     LoopLimitQueryable   { get; set; }   // null = use LoopLimit
    int      RecursiveLimit       { get; set; }   // 100 nested evaluations
    int      ObjectRecursionLimit { get; set; }   // 20
    int      LimitToString        { get; set; }   // 1048576 characters
    TimeSpan RegexTimeOut         { get; set; }   // 10 seconds
    CancellationToken CancellationToken { get; set; }
    void     CheckAbort()                         // throws ScriptAbortException

Behaviour switches (defaults as shipped):

    bool StrictVariables               { get; set; }  // false
    bool EnableRelaxedMemberAccess     { get; set; }  // true
    bool EnableRelaxedTargetAccess     { get; set; }  // false
    bool EnableRelaxedFunctionAccess   { get; set; }  // false
    bool EnableRelaxedIndexerAccess    { get; set; }  // true
    bool EnableNullIndexer             { get; set; }  // false
    bool EnableBreakAndContinueAsReturnOutsideLoop { get; set; }  // false
    bool ErrorForStatementFunctionAsExpression     { get; set; }
    bool AutoIndent                    { get; set; }  // true
    bool IndentOnEmptyLines            { get; set; }  // true
    bool IndentWithInclude             { get; set; }  // [Obsolete] alias
                                                      // for AutoIndent
    string CurrentIndent               { get; set; }
    bool UseScientific                 { get; set; }
    bool IsLiquid                      { get; protected set; }
    ScriptLang Language                { get; set; }  // Default

Member mapping, loading and hooks:

    MemberRenamerDelegate MemberRenamer { get; set; } // StandardMemberRenamer.Default
    MemberFilterDelegate  MemberFilter  { get; set; }
    ITemplateLoader       TemplateLoader { get; set; }
    ParserOptions TemplateLoaderParserOptions { get; set; }
    LexerOptions  TemplateLoaderLexerOptions  { get; set; }
    Dictionary<string, Template> CachedTemplates { get; }
    Dictionary<object, object>   Tags { get; }
    TryGetVariableDelegate TryGetVariable { get; set; }
    TryGetMemberDelegate   TryGetMember   { get; set; }
    RenderRuntimeExceptionDelegate RenderRuntimeException { get; set; }
    static RenderRuntimeExceptionDelegate RenderRuntimeExceptionDefault

    delegate bool TryGetMemberDelegate(TemplateContext context, SourceSpan span,
                                       object target, string member,
                                       out object value);
    delegate bool TryGetVariableDelegate(TemplateContext context, SourceSpan span,
                                         ScriptVariable variable,
                                         out object value);
    delegate string RenderRuntimeExceptionDelegate(ScriptRuntimeException ex);

Conversions and helpers used when writing custom functions:

    string  ObjectToString(object value, bool nested = false)
    bool    ToBool(SourceSpan span, object value)
    int     ToInt(SourceSpan span, object value)
    IList   ToList(SourceSpan span, object value)
    T       ToObject<T>(SourceSpan span, object value)
    object  ToObject(SourceSpan span, object value, Type destinationType)
    string  GetTypeName(object value)
    object  IsEmpty(SourceSpan span, object against)
    IObjectAccessor GetMemberAccessor(object target)
    IListAccessor   GetListAccessor(object target)
    void    Reset()

Template resolution used by `include`:

    string   GetTemplatePathFromName(string templateName, ScriptNode caller)
    Template GetOrCreateTemplate(string templatePath, ScriptNode caller)
    string   RenderTemplate(Template template, ScriptArray arguments,
                            ScriptNode callerContext)


ScriptObject -- THE MODEL CONTAINER
-----------------------------------
ScriptObject is an ordered, case-sensitive string/object dictionary that
implements IScriptObject. It is the canonical way to hand values to a
template without reflection.

    ScriptObject()
    ScriptObject(int capacity)
    ScriptObject(IEqualityComparer<string> keyComparer)
    ScriptObject(int capacity, IEqualityComparer<string> keyComparer)
    ScriptObject(int capacity, bool? autoImportStaticsFromThisType)
    ScriptObject(int capacity, bool? autoImportStaticsFromThisType,
                 IEqualityComparer<string> keyComparer)

    object this[string key] { get; set; }
    int    Count { get; }
    ICollection<string> Keys   { get; }
    ICollection<object> Values { get; }
    IEnumerable<string> GetMembers()
    bool   Contains(string member)
    bool   ContainsKey(string key)
    void   Add(string key, object value)
    void   SetValue(string member, object value, bool readOnly)
    bool   TryGetValue(TemplateContext context, SourceSpan span,
                       string member, out object value)
    T      GetSafeValue<T>(string name)
    T      GetSafeValue<T>(string name, T defaultValue)
    bool   CanWrite(string member)
    bool   TrySetValue(TemplateContext context, SourceSpan span,
                       string member, object value, bool readOnly)
    bool   Remove(string member)
    void   SetReadOnly(string member, bool readOnly)
    bool   IsReadOnly { get; set; }
    void   Clear()
    void   CopyTo(ScriptObject dest)
    IScriptObject Clone(bool deep)
    IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    static ScriptObject From(object obj)
    static bool IsImportable(object obj)

A class deriving from ScriptObject automatically imports its own public
STATIC members as template functions (that is exactly how the built-in
function tables such as StringFunctions are built). Pass
autoImportStaticsFromThisType: false to suppress that.

IScriptObject is the interface the engine actually consumes -- Count,
GetMembers(), Contains(member), IsReadOnly, TryGetValue(context, span, member,
out value), CanWrite(member), TrySetValue(context, span, member, value,
readOnly), Remove(member), SetReadOnly(member, readOnly), Clone(deep).
Implement it directly for a fully custom model object.


ScriptObjectExtensions -- IMPORTING .NET OBJECTS
------------------------------------------------
The Import methods are EXTENSION METHODS on IScriptObject declared in
CodeBrix.Templating.Runtime.ScriptObjectExtensions -- they are not instance
methods of ScriptObject. `using CodeBrix.Templating.Runtime;` is required for
them to resolve.

    static void Import(this IScriptObject script, object obj,
                       MemberFilterDelegate filter = null,
                       MemberRenamerDelegate renamer = null)

    static void Import(this IScriptObject script, object obj,
                       ScriptMemberImportFlags flags,
                       MemberFilterDelegate filter = null,
                       MemberRenamerDelegate renamer = null)

    static void Import(this IScriptObject script, string member,
                       Delegate function)

    static void Import(this IScriptObject @this, IScriptObject other)

    static void Import(this IScriptObject script, JsonElement json)

    static void ImportMember(this IScriptObject script, object obj,
                             string memberName, string exportName = null)

    static bool TryGetValue(this IScriptObject @this, string key,
                            out object value)
    static void SetValue(this IScriptObject @this, string member,
                         object value, bool readOnly)
    static ScriptObject GetScriptObject(this IScriptObject @this)
    static void AssertNotReadOnly(this IScriptObject scriptObject)

NOTE the argument order difference: on Import the FILTER comes before the
RENAMER; on Template.Render the RENAMER comes before the FILTER. Use named
arguments on both.

Pass a Type as `obj` to import the static members of that type
(scriptObject.Import(typeof(Math))). Pass an instance to import its instance
fields and properties.

    [Flags] enum ScriptMemberImportFlags { Field = 1, Property = 2,
                                           Method = 4, All = Field|Property|Method }

    delegate bool   MemberFilterDelegate(MemberInfo member);
    delegate string MemberRenamerDelegate(MemberInfo member);

    sealed class StandardMemberRenamer
        static readonly MemberRenamerDelegate Default
        static string Rename(MemberInfo member)
        static string Rename(string name)

StandardMemberRenamer turns PascalCase into snake_case: `ThisIsAnExample`
becomes `this_is_an_example`. It is the default renamer for both
TemplateContext.MemberRenamer and Template.Render. To keep .NET names as-is,
pass `member => member.Name`.

    [AttributeUsage(Field|Property|Method)]
    class ScriptMemberIgnoreAttribute : Attribute

Apply [ScriptMemberIgnore] to a field, property or method to keep Import from
exposing it to templates.


OUTPUT SINKS
------------
    interface IScriptOutput
        void      Write(string text, int offset, int count);
        ValueTask WriteAsync(string text, int offset, int count,
                             CancellationToken cancellationToken);

    class StringBuilderOutput : IScriptOutput
        StringBuilderOutput()
        StringBuilderOutput(StringBuilder builder)
        StringBuilder Builder { get; }
        static StringBuilderOutput GetThreadInstance()

    class TextWriterOutput : IScriptOutput
        TextWriterOutput()                 // wraps a new StringWriter
        TextWriterOutput(TextWriter writer)
        TextWriter Writer { get; }

    static class ScriptOutputExtensions
        static void      Write(this IScriptOutput o, string text)
        static void      Write(this IScriptOutput o, ScriptStringSlice text)
        static ValueTask WriteAsync(this IScriptOutput o, string text,
                                    CancellationToken cancellationToken)
        static ValueTask WriteAsync(this IScriptOutput o, ScriptStringSlice text,
                                    CancellationToken cancellationToken)

A fresh TemplateContext starts with a StringBuilderOutput.


TEMPLATE LANGUAGE REFERENCE (SCRIBAN MODE)
==========================================

CODE BLOCKS, RAW TEXT AND ESCAPING
----------------------------------
Everything outside `{{ ... }}` is raw text and is copied to the output
verbatim. Inside `{{ ... }}` the content is a sequence of script statements;
the value of a statement that is a bare expression is written to the output.

    Hello {{ name }}!                       <- one expression
    {{ x = 1; y = 2; x + y }}               <- statements separated by `;`
    {{
      x = 1
      y = 2
      x + y                                 <- newline also ends a statement
    }}

To emit literal `{{` and `}}`, wrap the region in an escape block. Add one `%`
per nesting level:

    {%{ this {{ is }} not interpreted }%}
    {%%{ and this can contain a literal }%} }%%}

COMMENTS
--------
    {{ # a single-line comment, runs to the end of the line or to }} }}
    {{ ## a multi-line
       comment ## }}

WHITESPACE CONTROL
------------------
    {{-  ...  -}}    greedy: removes ALL whitespace (newlines included) before
                     the `{{-` / after the `-}}`
    {{~  ...  ~}}    non-greedy: `{{~` removes whitespace before but stops at
                     the first newline without consuming it; `~}}` removes
                     whitespace after INCLUDING the first newline, then stops

The corresponding AST value is ScriptWhitespaceMode.Greedy / NonGreedy / None.
By default TemplateContext.AutoIndent is true, so the indentation in front of
an interpolation is re-applied to every line of a multi-line value (including
the text produced by `include`).

LITERALS
--------
    "double quoted"      escapes: \" \n \t \\ \uXXXX
    'single quoted'      escapes: \'
    `verbatim string`    no escapes; double a backtick to embed one
    1  100000000000  1_2_3            integers (underscores allowed)
    0xff  0x1_ff                      hexadecimal
    0b11111111  0b_1111_1111          binary
    0xFFFFFFFFu / 0xFFFFFFFFU         unsigned suffix
    2.5  1e3  1e40  1f  1F            floating point
    true  false  null
    $"interpolated {1 + 2} string"    interpolated string ($'...' also works)

In an interpolated string `\{` emits a literal `{`.

VARIABLES, MEMBERS AND SCOPES
-----------------------------
    name              global variable
    $name             local variable (scoped to the enclosing func / include)
    obj.member        member access
    obj?.member       null-conditional member access (null instead of error)
    list[0]           indexer
    obj["member"]     indexer with a string key
    this              the current global object
    x.empty?          true for an empty list, string or script object;
                      false when it has content; null when x is null.
                      Read-only -- assigning to it raises a runtime error

Assignment: `=`, and the compound forms `+=`, `-=`, `*=`, `/=`, `//=`, `%=`.
Increment / decrement: `++` and `--`, prefix or postfix.

OPERATORS
---------
    arithmetic   +  -  *  /  //  %      (`//` divides and floors)
    string       +  (concatenation)   *  (repeat: "a" * 2)
    comparison   ==  !=  <  >  <=  >=   (also compares strings)
    logical      &&  ||  !
    bitwise      &  |  ^  <<  >>        (on arrays: set/append operations)
    coalescing   a ?? b                 b when a is null
                 a ?! b                 b when a is NOT null, else null
    ternary      cond ? then : else
    ranges       1..5                   inclusive  -> [1, 2, 3, 4, 5]
                 1..<5                  exclusive  -> [1, 2, 3, 4]
    function     @name                  reference a function without calling it
                 ^array                 expand an array as call arguments

ARRAYS AND OBJECTS
------------------
    x = [1, 2, 3]
    x[0]
    y = { member1: "value", member2: 150 }
    y.member1
    y["member2"]

STATEMENTS
----------
Each block statement is closed with `end`.

    if <expression> ... else if <expression> ... else ... end
    case <expression> when <v1> ... when <v2> ... else ... end
    for <var> in <expression> [offset: n] [limit: n] [reversed] ... else ... end
    tablerow <var> in <expression> [cols: n] ... end
    while <expression> ... end
    break
    continue
    capture <variable> ... end        captures the rendered block into a variable
    with <object> ... end             assignments inside target that object
    import <object>                   copies an object's members into scope
    readonly <variable>               makes a variable read-only
    func <name> ... end               declares a function
    ret <expression>                  returns from a function (`return` also works)
    wrap <function_call> ... end      calls the function with the block as `$$`

`for` example, with the loop-state object:

    {{ for item in items offset:1 limit:3 reversed }}
      {{ for.index }} {{ item }}
    {{ end }}

Inside a `for` the object `for` exposes: index (0-based; 1-based in Liquid),
rindex, first, last, even, odd, length, changed. `while` exposes the same
members on the object `while`, and `tablerow` adds col, col0, col_first and
col_last on the object `tablerow` (index0 and rindex0 exist in Liquid mode).
A `for` may carry an `else` block that runs when the sequence is empty.

FUNCTIONS AND PIPES
-------------------
    {{ func inc
         ret $0 + 1
       end
       inc 1            }}      -> 2

    {{ func inc3
         ret $0 + $1[0] + $2.member1
       end
       inc3 (1) [1] {member1: 1} }}   -> 3

Inside a function:
    $0, $1, $2 ...   positional arguments
    $                the whole arguments object; $.name / $name for named ones
    $$               the block passed by a `wrap` statement

Functions can also declare named parameters, optional parameters (`name = value`)
and a variadic last parameter (`name...`).

Calls take arguments separated by spaces, not parentheses:

    {{ string.truncate text 10 "..." }}
    {{ date.parse '2016/01/05' }}
    {{ myfunc arg1: 1, arg2: "x" }}      named arguments

The pipe operator feeds the left value in as the FIRST argument of the right
call, so pipelines read left to right:

    {{ "hello" | string.upcase | string.append " world" }}
    {{ [3, 1, 2] | array.sort | array.join ", " }}
    {{ ((2 | inc | inc) + 5) | inc }}

`|>` is accepted as a pipe as well and is the only pipe form in Scientific
mode (ScriptLang.Scientific), where `|` keeps its bitwise-or meaning.

INCLUDE
-------
    {{ include 'header' }}
    {{ include 'arguments' 1 2 }}                 -> $1 and $2 in the include
    {{ include 'partial' name: 'value' }}         -> $.name / $name

Positional arguments inside an included template start at $1, not $0 -- $0 is
the template name. (Inside a `func` the first argument IS $0.)
    {{ include_join ['a', 'b', 'c'] ', ' }}
    {{ include_join names ', ' 'begin ' ' end' }}

`include` resolves the name through TemplateContext.TemplateLoader; without a
loader assigned it throws at render time. Loaded templates are cached in
TemplateContext.CachedTemplates for the lifetime of the context.
`include_join` takes a list of template names, a separator, and optional
start and end components; a component prefixed with `tpl:` is itself rendered
as a template name.

FRONT MATTER
------------
With LexerOptions.Mode set to ScriptMode.FrontMatterAndContent (or
FrontMatterOnly), a template may open with a `+++` delimited script block:

    +++
    variable = 1
    name = 'yes'
    +++
    This is after the front matter: {{ name }}

The parsed block is available as template.Page.FrontMatter and can be
evaluated on its own with context.Evaluate(frontMatter). The marker text is
LexerOptions.FrontMatterMarker (default "+++").

LIQUID MODE
-----------
Parse with Template.ParseLiquid and render with a LiquidTemplateContext:

    var template = Template.ParseLiquid(
        "{% assign x = 'abcdef' %}{{ x | truncate: 5 }}");
    var context = new LiquidTemplateContext();
    var result = template.Render(context);

Liquid differences enforced by the parser and the Liquid context:

    * statements live in `{% ... %}` tags; `{{ ... }}` only outputs a value
    * closing tags are explicit: endif, endunless, endfor, endcase,
      endcapture, endtablerow, endifchanged
    * additional tags: assign, capture, case/when, cycle, decrement,
      increment, if/elsif/else, ifchanged, include, raw, comment, tablerow,
      unless, for, break, continue
    * `{% raw %} ... {% endraw %}` and `{% comment %} ... {% endcomment %}`
    * filter arguments use a colon and commas: `{{ x | truncate: 5 }}`
    * the loop-state object is written `forloop` (and `tablerowloop`); the
      Scriban names `for` and `tablerow` are accepted as well
    * `forloop.index` is 1-based, and index0 / rindex0 are available
    * only the `-` whitespace modifier exists (`{%-`, `-%}`, `{{-`, `-}}`);
      `~` is a Scriban-only modifier
    * break/continue outside a loop return from the template instead of
      raising an error (EnableBreakAndContinueAsReturnOutsideLoop is on)

LiquidBuiltinsFunctions installs the Liquid filter names as aliases of the
Scriban builtins, and exposes the mapping so tooling can translate:

    static bool LiquidBuiltinsFunctions.TryLiquidToScriban(string liquidBuiltin,
                                                           out string target,
                                                           out string member)

The registered Liquid aliases are: abs, append, capitalize, ceil, compact,
concat, contains, cycle, date, default, divided_by, downcase, escape, first,
floor, join, last, lstrip, map, minus, modulo, newline_to_br, plus, prepend,
remove, remove_first, replace, replace_first, reverse, round, rstrip, size,
slice, sort, split, strip, strip_html, strip_newlines, times, truncate,
truncatewords, uniq, upcase.

ParserOptions.LiquidFunctionsToScriban = true rewrites Liquid filter calls
into their Scriban equivalents at parse time (this is what a
LiquidTemplateContext uses for the templates it loads through `include`).


BUILT-IN FUNCTIONS
==================

A new TemplateContext installs BuiltinFunctions as its bottom-most global
scope. That object holds the function tables `array`, `string`, `math`,
`date`, `timespan`, `object`, `regex`, `html`, the functions `include` and
`include_join`, and the two aliases `empty` and `blank`
(EmptyScriptObject.Default). Below, `<x>` denotes the first argument, which is
what a pipe supplies.

array (CodeBrix.Templating.Functions.ArrayFunctions)
---------------------------------------------------
    array.add <list> <value>            new list with value appended
    array.add_range <list1> <list2>     new list with all of list2 appended
    array.any <list> <fn> [args...]     true if any item satisfies fn
    array.compact <list>                removes null entries
    array.concat <list1> <list2>        concatenates two lists
    array.contains <list> <item>        true if the list contains item
    array.cycle <list> [group]          returns the next item each time it is
                                        called, cycling through the list
    array.each <list> <fn>              applies fn to every element
    array.filter <list> <fn>            keeps the elements fn accepts
    array.first <list>                  first element
    array.insert_at <list> <i> <value>  inserts value at index i
    array.join <list> <sep> [fn]        joins into a string with a delimiter
    array.last <list>                   last element
    array.limit <list> <count>          first count elements
    array.map <list> <member>           projects one member of each element
    array.offset <list> <index>         everything after index
    array.remove_at <list> <index>      removes the element at index
    array.reverse <list>                reverses the list
    array.size <list>                   number of elements
    array.sort <list> [member]          sorts by value or by a member
    array.uniq <list>                   distinct elements

    {{ [3, 1, 2] | array.sort | array.join ", " }}
    {{ [" hello", " world", "20"] | array.any @string.contains "20" }}

string (CodeBrix.Templating.Functions.StringFunctions)
------------------------------------------------------
    string.append <text> <with>         concatenates two strings
    string.base64_decode <text>         decodes Base64 (UTF-8)
    string.base64_encode <text>         encodes to Base64 (UTF-8)
    string.capitalize <text>            upper-cases the first character
    string.capitalizewords <text>       upper-cases the first letter of each word
    string.contains <text> <value>      true if text contains value
    string.downcase <text>              lower case
    string.empty <text>                 true if the string is empty
    string.ends_with <text> <value>     suffix test
    string.equals_ignore_case <a> <b>   case-insensitive equality
    string.escape <text>                escapes the string with escape characters
    string.handleize <text>             makes a URL handle/slug
    string.hmac_sha1 <text> <key>       HMAC-SHA1 with a secret key
    string.hmac_sha256 <text> <key>     HMAC-SHA256 with a secret key
    string.hmac_sha512 <text> <key>     HMAC-SHA512 with a secret key
    string.index_of <text> <search> [startIndex] [count] [stringComparison]
                                        zero-based index of the first match
    string.literal <text>               returns the string as a double-quoted
                                        string literal
    string.lstrip <text>                trims whitespace on the left
    string.md5 <text>                   md5 hash
    string.pad_left <text> <width>      pads with leading spaces
    string.pad_right <text> <width>     pads with trailing spaces
    string.pluralize <number> <singular> <plural>
                                        picks the singular or plural word
    string.prepend <text> <by>          puts `by` in front of `text`
    string.remove <text> <remove>       removes every occurrence
    string.remove_first <text> <remove> removes the first occurrence
    string.remove_last <text> <remove>  removes the last occurrence
    string.replace <text> <match> <replace>        replaces every occurrence
    string.replace_first <text> <match> <replace> [fromEnd]
                                        replaces the first occurrence
    string.rstrip <text>                trims whitespace on the right
    string.sha1 / sha256 / sha512 <text>           hash of the string
    string.size <text>                  number of characters
    string.slice <text> <start> [length]           substring
    string.slice1 <text> <start> [length=1]        substring, Liquid semantics
    string.split <text> <match>         splits on a delimiter into an array
    string.starts_with <text> <value>   prefix test
    string.strip <text>                 trims whitespace on both sides
    string.strip_newlines <text>        removes line breaks
    string.to_int / to_long / to_float / to_double <text>    conversions
    string.truncate <text> <length> [ellipsis]     truncates to N characters
    string.truncatewords <text> <count> [ellipsis] truncates to N words
    string.upcase <text>                upper case
    string.whitespace <text>            true if empty or all whitespace

math (CodeBrix.Templating.Functions.MathFunctions)
--------------------------------------------------
    math.abs <value>                    absolute value
    math.ceil <value>                   smallest integer >= value
    math.divided_by <value> <divisor>   division (integer divisor floors)
    math.floor <value>                  largest integer <= value
    math.format <value> <format> [culture]
                                        .NET numeric format string
    math.is_number <value>              true if the value is a number
    math.minus <value> <with>           value - with
    math.modulo <value> <with>          value % with
    math.plus <value> <with>            value + with
    math.random <minValue> <maxValue>   random number in the range
    math.round <value> [precision=0]    rounds to N fractional digits
    math.times <value> <with>           value * with
    math.uuid                           a new UUID string

date (CodeBrix.Templating.Functions.DateTimeFunctions)
------------------------------------------------------
    date.now                            current local time
    date.utc_now                        current UTC time
    date.add_days <date> <days>         adds days (double)
    date.add_hours <date> <hours>
    date.add_minutes <date> <minutes>
    date.add_seconds <date> <seconds>
    date.add_milliseconds <date> <millis>
    date.add_months <date> <months>     adds months (int)
    date.add_years <date> <years>       adds years (int)
    date.parse <text> [pattern] [culture]          parses a date
    date.parse_to_string <text> [output_pattern] [output_culture]
                                        [input_pattern] [input_culture]
    date.to_string <date> [pattern] [culture]      formats a date
    date.format                         read/write; the pattern used when a
                                        date is rendered without an explicit
                                        one. Assign it to change the default:
                                        {{ date.format = '%Y-%m-%d' }}
    date.default_format                 read-only, "%d %b %Y"

    Date patterns are strftime-style: %a %A %b %B %c %C %d %D %e %F %h %H %I
    %j %k %l %L %m %M %n %N %p %P %r %R %s %S %t %T %u %U %v %V %W %w %x %X
    %y %Y %Z and %% for a literal percent. Formatting uses the current
    culture; prefix the pattern with %g to force the invariant culture.
    A date value is a .NET DateTime, so its properties are reachable through
    the member renamer: {{ date.now.year }}, {{ date.now.day_of_week }}.

timespan (CodeBrix.Templating.Functions.TimeSpanFunctions)
----------------------------------------------------------
    timespan.zero                       TimeSpan.Zero
    timespan.from_days <days>
    timespan.from_hours <hours>
    timespan.from_minutes <minutes>
    timespan.from_seconds <seconds>
    timespan.from_milliseconds <millis>
    timespan.parse <text>               parses a timespan

object (CodeBrix.Templating.Functions.ObjectFunctions)
------------------------------------------------------
    object.default <value> <default>    default when value is null or ""
    object.eval <value>                 evaluates a string as an expression
    object.eval_template <value>        evaluates a string as a template
    object.format <value> <format> [culture]      formats any object
    object.from_json <json>             JSON text to a script value
    object.to_json <value>              script value to JSON text
    object.has_key <value> <key>        true if the member exists
    object.has_value <value> <key>      true if the member has a value
    object.keys <value>                 array of member names
    object.values <value>               array of member values
    object.size <value>                 length of a string, count of a list,
                                        member count of an object
    object.typeof <value>               "string", "boolean", "number",
                                        "array", "iterator" or "object"
    object.kind <value>                 finer-grained .NET type name
                                        ("int", "double", "datetime", ...)

regex (CodeBrix.Templating.Functions.RegexFunctions)
----------------------------------------------------
    regex.escape <pattern>              escapes regex metacharacters
    regex.unescape <pattern>            reverses regex.escape
    regex.match <text> <pattern> [options]     first match, as an array of
                                               groups (empty array if no match)
    regex.matches <text> <pattern> [options]   all matches
    regex.replace <text> <pattern> <replace> [options]
    regex.split <text> <pattern> [options]

    `options` is a string of flag characters: `i` case-insensitive, `m`
    multiline, `s` single-line (dot matches newline), `x` ignore pattern
    whitespace. Every regex call is bounded by TemplateContext.RegexTimeOut
    (10 seconds by default). regex.matches returns an array of match arrays.

html (CodeBrix.Templating.Functions.HtmlFunctions)
--------------------------------------------------
    html.escape <text>                  HTML-escapes (& becomes &amp;)
    html.strip <text>                   removes HTML tags (regex-based; not a
                                        security-grade sanitiser)
    html.newline_to_br <text>           inserts <br /> before each newline
    html.url_encode <text>              percent-encodes URL-unsafe characters
    html.url_escape <text>              escapes characters not allowed in URLs

include and include_join
------------------------
    include <template_name> [args...]
    include_join <template_names> <separator> [start] [end]

Both are IScriptCustomFunction implementations (IncludeFunction and
IncludeJoinFunction) rather than ScriptObject tables, and both require
TemplateContext.TemplateLoader to be set.


EXTENSIBILITY: CUSTOM FUNCTIONS AND MEMBER MAPPING
==================================================

THREE WAYS TO ADD A FUNCTION
----------------------------
1. Import a delegate under a name (simplest):

    var globals = new ScriptObject();
    globals.Import("shout", new Func<string, string>(s => s.ToUpperInvariant()));
    // {{ shout "hi" }} or {{ "hi" | shout }}

   Async delegates work too -- import a Func<Task<T>> / Func<ValueTask<T>>
   and await RenderAsync.

2. Import a whole type's public statics as a function table:

    public static class MyFunctions
    {
        public static string Shout(string text) => text.ToUpperInvariant();
        [ScriptMemberIgnore] public static void NotExposed() { }
    }

    var globals = new ScriptObject();
    globals.Import(typeof(MyFunctions));      // -> {{ shout "hi" }}

   Or derive from ScriptObject and let the constructor auto-import the
   statics of the derived type:

    public class MyFunctions : ScriptObject
    {
        public static string Shout(string text) => text.ToUpperInvariant();
    }
    context.PushGlobal(new MyFunctions());

3. Implement IScriptCustomFunction for full control over arity, argument
   conversion and the block statement:

    public interface IScriptCustomFunction : IScriptFunctionInfo
    {
        object Invoke(TemplateContext context, ScriptNode callerContext,
                      ScriptArray arguments, ScriptBlockStatement blockStatement);
        ValueTask<object> InvokeAsync(TemplateContext context,
                      ScriptNode callerContext, ScriptArray arguments,
                      ScriptBlockStatement blockStatement);
    }

    public interface IScriptFunctionInfo
    {
        int  RequiredParameterCount { get; }
        int  ParameterCount         { get; }
        ScriptVarParamKind VarParamKind { get; }   // None|Direct|LastParameter
        Type ReturnType             { get; }
        ScriptParameterInfo GetParameterInfo(int index);
    }

    readonly struct ScriptParameterInfo
        ScriptParameterInfo(Type parameterType, string name)
        ScriptParameterInfo(Type parameterType, string name, object defaultValue)
        Type ParameterType; string Name; bool HasDefaultValue; object DefaultValue

    static class ScriptFunctionInfoExtensions
        static bool IsParameterType<T>(this IScriptFunctionInfo info, int index)

READY-MADE IScriptCustomFunction WRAPPERS
-----------------------------------------
    class DelegateCustomFunction : DynamicCustomFunction
        DelegateCustomFunction(Delegate del)
        DelegateCustomFunction(object target, MethodInfo method)
        object Target { get; }
        static DelegateCustomFunction Create(Action action)
        static DelegateCustomFunction Create<T>(Action<T> action)
        static DelegateCustomFunction Create<T1,T2>(Action<T1,T2> action)
        static DelegateCustomFunction Create<T1,T2,T3>(Action<T1,T2,T3> action)
        static DelegateCustomFunction Create<T1,T2,T3,T4>(Action<...> action)
        static DelegateCustomFunction Create<T1,T2,T3,T4,T5>(Action<...> action)
        static DelegateCustomFunction CreateFunc<TResult>(Func<TResult> func)
        static DelegateCustomFunction CreateFunc<T1,TResult>(Func<T1,TResult> func)
        ... up to CreateFunc<T1,T2,T3,T4,T5,TResult>

    class DelegateCustomAction : DelegateCustomFunction
        DelegateCustomAction(Action func)
        Action Func { get; }

    abstract class DynamicCustomFunction : IScriptCustomFunction
        readonly MethodInfo Method
        object Tag { get; set; }
        static DynamicCustomFunction Create(Delegate del)
        static DynamicCustomFunction Create(object target, MethodInfo method)

DynamicCustomFunction.Create picks a pre-generated, allocation-free adapter
for the common built-in signatures and falls back to a reflection-based one
otherwise. A custom function may take a leading TemplateContext and/or
SourceSpan parameter; the engine supplies them and they are not template
arguments. A trailing `params object[]` makes the function variadic.

    class ScriptLazy<T> : IScriptCustomFunction
        ScriptLazy(Func<T> valueFactory)

Register a ScriptLazy<T> as a global to defer an expensive computation until
the template actually reads the name.

CONTROLLING WHICH .NET MEMBERS ARE VISIBLE
------------------------------------------
    * MemberRenamerDelegate maps a MemberInfo to the template name.
      StandardMemberRenamer.Default converts PascalCase to snake_case; pass
      `member => member.Name` to keep .NET names.
    * MemberFilterDelegate returns false to hide a member.
    * [ScriptMemberIgnore] hides a member unconditionally.
    * TemplateContext.MemberRenamer / MemberFilter apply to objects reached
      transitively during rendering; the Render/Import parameters apply to the
      object being imported.

    var context = new TemplateContext
    {
        MemberRenamer = member => member.Name,          // keep PascalCase
        MemberFilter  = member => member.Name != "Secret",
    };

ACCESSORS -- HOW THE ENGINE READS A VALUE
-----------------------------------------
    interface IObjectAccessor
        int  GetMemberCount(TemplateContext context, SourceSpan span, object target);
        IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span,
                                       object target);
        bool HasMember(TemplateContext context, SourceSpan span, object target,
                       string member);
        bool TryGetValue(TemplateContext context, SourceSpan span, object target,
                         string member, out object value);
        bool TrySetValue(TemplateContext context, SourceSpan span, object target,
                         string member, object value);
        bool TryGetItem(TemplateContext context, SourceSpan span, object target,
                        object index, out object value);
        bool TrySetItem(TemplateContext context, SourceSpan span, object target,
                        object index, object value);
        bool HasIndexer { get; }
        Type IndexType  { get; }

    interface IListAccessor
        int    GetLength(TemplateContext context, SourceSpan span, object target);
        object GetValue(TemplateContext context, SourceSpan span, object target,
                        int index);
        void   SetValue(TemplateContext context, SourceSpan span, object target,
                        int index, object value);

Shipped implementations, each exposing a `Default` instance:
ScriptObjectAccessor (IScriptObject), DictionaryAccessor (IDictionary and
generic dictionaries), ListAccessor (IList), ArrayAccessor (arrays),
StringAccessor (strings, so `text[0]` and `text.size` work), NullAccessor,
and TypedObjectAccessor (reflection over an arbitrary POCO; it takes a
MemberFilterDelegate and a MemberRenamerDelegate). Resolve the accessor the
engine would use with context.GetMemberAccessor(target) /
context.GetListAccessor(target).

    interface IScriptTransformable
        Type   ElementType { get; }
        bool   CanTransform(Type transformType);
        bool   Visit(TemplateContext context, SourceSpan span, Func<object,bool> visit);
        object Transform(TemplateContext context, SourceSpan span,
                         Func<object,object> apply, Type destType);


TEMPLATE LOADING (ITemplateLoader)
==================================

    public interface ITemplateLoader
    {
        string GetPath(TemplateContext context, SourceSpan callerSpan,
                       string templateName);
        string Load(TemplateContext context, SourceSpan callerSpan,
                    string templatePath);
        ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan,
                                    string templatePath);
    }

GetPath turns the name written in the template into a canonical path (return
null to make `include` yield nothing); Load / LoadAsync return the template
text for that path. RenderAsync uses LoadAsync, Render uses Load.

A file-system loader:

    using System.IO;
    using System.Threading.Tasks;
    using CodeBrix.Templating;
    using CodeBrix.Templating.Parsing;
    using CodeBrix.Templating.Runtime;

    public class FileTemplateLoader : ITemplateLoader
    {
        private readonly string _root;
        public FileTemplateLoader(string root) => _root = root;

        public string GetPath(TemplateContext context, SourceSpan callerSpan,
                              string templateName)
            => Path.GetFullPath(Path.Combine(_root, templateName));

        public string Load(TemplateContext context, SourceSpan callerSpan,
                           string templatePath)
            => File.ReadAllText(templatePath);

        public async ValueTask<string> LoadAsync(TemplateContext context,
                                                 SourceSpan callerSpan,
                                                 string templatePath)
            => await File.ReadAllTextAsync(templatePath).ConfigureAwait(false);
    }

    var context = new TemplateContext
    {
        TemplateLoader = new FileTemplateLoader("templates"),
    };

Guard against directory traversal inside GetPath when template names come
from untrusted input -- the engine does not sanitise them.


ERROR HANDLING AND DIAGNOSTICS
==============================

PARSE ERRORS
------------
Template.Parse / ParseLiquid never throw for a syntax error; they return a
Template with HasErrors set.

    var template = Template.Parse(text, "report.sbn");
    if (template.HasErrors)
    {
        foreach (var message in template.Messages)
        {
            // message.Type: ParserMessageType.Error | Warning
            // message.Span: SourceSpan (FileName, Start, End, Length)
            // message.Message: the text
            Console.Error.WriteLine(message);   // "file(line,col) : error : ..."
        }
        return;
    }

    class LogMessageBag : IReadOnlyList<LogMessage>
        int Count { get; }
        LogMessage this[int index] { get; }
        bool HasErrors { get; }
        void Add(LogMessage message)
        void AddRange(IEnumerable<LogMessage> messages)

    class LogMessage
        LogMessage(ParserMessageType type, SourceSpan span, string message)
        ParserMessageType Type { get; set; }
        SourceSpan        Span { get; set; }
        string            Message { get; set; }

    enum ParserMessageType { Error, Warning }

Rendering a template that HasErrors throws InvalidOperationException.

RUNTIME ERRORS
--------------
    class ScriptRuntimeException : Exception
        ScriptRuntimeException(SourceSpan span, string message)
        ScriptRuntimeException(SourceSpan span, string message, Exception inner)
        SourceSpan Span { get; }
        string     OriginalMessage { get; }     // message without the location
        override string Message                 // "file(line,col) : error : ..."
        static bool EnableDisplayInnerException // include the inner exception
                                                // in ToString()

    class ScriptParserRuntimeException : ScriptRuntimeException
        ScriptParserRuntimeException(SourceSpan span, string message,
                                     LogMessageBag parserMessages)
        ScriptParserRuntimeException(SourceSpan span, string message,
                                     LogMessageBag parserMessages, Exception inner)
        LogMessageBag ParserMessages { get; }   // errors from an included template

    class ScriptAbortException : ScriptRuntimeException
        ScriptAbortException(SourceSpan span, CancellationToken cancellationToken)
        ScriptAbortException(SourceSpan span, string message,
                             CancellationToken cancellationToken)
        CancellationToken CancellationToken { get; }

    class ScriptArgumentException : Exception
        ScriptArgumentException(int argumentIndex, string message)
        int ArgumentIndex { get; }              // throw this from a custom
                                                // function to blame one argument

Set TemplateContext.CancellationToken and the engine raises
ScriptAbortException at the next CheckAbort point. Exceeding LoopLimit,
RecursiveLimit, ObjectRecursionLimit or LimitToString raises
ScriptRuntimeException.

LOCATIONS
---------
    struct SourceSpan
        SourceSpan(string fileName, TextPosition start, TextPosition end)
        string FileName { get; set; }
        TextPosition Start { get; set; }
        TextPosition End { get; set; }
        int Length { get; }
        bool IsEmpty { get; }
        string ToStringSimple()

    struct TextPosition
        TextPosition(int offset, int line, int column)   // line/column 0-based
        int Offset { get; set; }
        int Line   { get; set; }
        int Column { get; set; }
        TextPosition NextColumn(int offset = 1)
        TextPosition NextLine(int offset = 1)
        static readonly TextPosition Eof

Pass a sourceFilePath to Parse so error messages and spans name the file.


PARSING, AST, VISITORS AND FORMATTING
=====================================

PARSE OPTIONS
-------------
    class ParserOptions
        static readonly ParserOptions Default
        int  ExpressionDepthLimit { get; init; }      // 250
        bool LiquidFunctionsToScriban { get; init; }
        bool ParseFloatAsDecimal { get; init; }

    class LexerOptions
        static readonly LexerOptions Default
        const string DefaultFrontMatterMarker = "+++"
        ScriptMode   Mode { get; init; }
        ScriptLang   Lang { get; init; }
        string       FrontMatterMarker { get; init; }
        bool         EnableIncludeImplicitString { get; init; }
        TextPosition StartPosition { get; init; }
        bool         KeepTrivia { get; init; }        // needed for formatting
        TryMatchCustomTokenDelegate TryMatchCustomToken { get; init; }

    enum ScriptLang { Default, Liquid, Scientific }
    enum ScriptMode { Default, FrontMatterOnly, FrontMatterAndContent, ScriptOnly }

ScriptMode.ScriptOnly parses the whole text as script with no `{{ }}` markers
-- useful for evaluating a single expression. KeepTrivia must be true if you
intend to reprint or format the AST and keep comments and spacing.

WORKING WITH THE AST
--------------------
template.Page is a ScriptPage (FrontMatter + Body). Every node derives from
ScriptNode:

    abstract class ScriptNode
        SourceSpan Span;
        ScriptNode Parent { get; }
        int        ChildrenCount { get; }
        ScriptNode GetChildren(int index)
        IEnumerable<ScriptNode> Children { get; }
        ScriptNode Clone()
        ScriptNode Clone(bool withTrivias)
        abstract object Evaluate(TemplateContext context)
        virtual ValueTask<object> EvaluateAsync(TemplateContext context)
        abstract void PrintTo(ScriptPrinter printer)
        virtual void Accept(ScriptVisitor visitor)
        virtual TResult Accept<TResult>(ScriptVisitor<TResult> visitor)
        sealed override string ToString()          // prints the node back

Two roots split the tree: ScriptExpression and ScriptStatement.

    Expressions: ScriptLiteral, ScriptVariable (ScriptVariableGlobal /
      ScriptVariableLocal), ScriptMemberExpression, ScriptIndexerExpression,
      ScriptNestedExpression, ScriptThisExpression, ScriptIsEmptyExpression,
      ScriptUnaryExpression, ScriptIncrementDecrementExpression,
      ScriptBinaryExpression, ScriptConditionalExpression,
      ScriptAssignExpression, ScriptFunctionCall, ScriptPipeCall,
      ScriptNamedArgument, ScriptArgumentBinary, ScriptAnonymousFunction,
      ScriptArrayInitializerExpression, ScriptObjectInitializerExpression,
      ScriptObjectMember, ScriptInterpolatedExpression,
      ScriptInterpolatedStringExpression
    Statements: ScriptPage, ScriptBlockStatement, ScriptRawStatement,
      ScriptEscapeStatement, ScriptExpressionStatement, ScriptFrontMatter,
      ScriptIfStatement, ScriptElseStatement, ScriptCaseStatement,
      ScriptWhenStatement, ScriptConditionStatement, ScriptForStatement,
      ScriptTableRowStatement, ScriptWhileStatement, ScriptLoopStatementBase,
      ScriptBreakStatement, ScriptContinueStatement, ScriptCaptureStatement,
      ScriptWithStatement, ScriptImportStatement, ScriptReadOnlyStatement,
      ScriptFunction, ScriptParameter, ScriptReturnStatement,
      ScriptWrapStatement, ScriptEndStatement, ScriptNopStatement
    Terminals and trivia: ScriptVerbatim, ScriptToken, ScriptKeyword,
      ScriptIdentifier, ScriptList / ScriptList<T>, ScriptTrivia,
      ScriptTrivias, ScriptTriviaType, ScriptStringSlice, IScriptTerminal
    Operator enums: ScriptBinaryOperator, ScriptUnaryOperator (each with a
      ...Extensions class exposing ToText()), ScriptVariableScope,
      ScriptLiteralStringQuoteType, ScriptFlowState, ScriptWhitespaceMode
    Extension points on nodes: IScriptVariablePath, IScriptCustomType,
      IScriptCustomTypeInfo, IScriptCustomBinaryOperation,
      IScriptCustomUnaryOperation, IScriptConvertibleFrom,
      IScriptConvertibleTo, IScriptCustomImplicitMultiplyPrecedence,
      IScriptNamedArgumentContainer, IScriptVisitorContext
    Metadata: ScriptSyntaxAttribute / ScriptTypeNameAttribute carry the
      human-readable grammar of each node (ScriptSyntaxAttribute.Get(node))
    Node-editing helpers: ScriptNodeExtensions (FindFirstTerminal,
      FindLastTerminal, RemoveLeadingSpace, RemoveTrailingSpace,
      MoveLeadingTriviasTo, MoveTrailingTriviasTo, AddLeadingSpace,
      AddSpaceAfter, AddCommaAfter, AddSemiColonAfter, AddTrivia,
      InsertTrivia, AddTrivias, HasLeadingSpaceTrivias,
      HasTrailingSpaceTrivias, HasTrivia, HasTriviaEndOfStatement),
      ScriptParameterContainerExtensions, ScriptTriviaTypeExtensions,
      ScriptBinaryOperatorExtensions and ScriptUnaryOperatorExtensions
      (ToText() for an operator). These matter when you build or rewrite
      nodes by hand and need the printed form to stay legal.

VISITING AND REWRITING
----------------------
    class ScriptVisitor
        virtual void Visit(ScriptNode node)
        virtual void Visit(ScriptList list)
        // plus a Visit overload per concrete node type
    class ScriptVisitor<TResult>
        virtual TResult Visit(ScriptNode node)
    class ScriptRewriter : ScriptVisitor<ScriptNode>
        bool CopyTrivias { get; set; }
        override ScriptNode Visit(ScriptNode node)

Derive from ScriptVisitor to inspect a template (find every variable, every
include, every function call) and from ScriptRewriter to transform it. A
ScriptRewriter returns a new tree; the base implementation copies structure
without changing it.

    public class VariableCollector : ScriptVisitor
    {
        public readonly List<string> Names = new List<string>();
        public override void Visit(ScriptNode node)
        {
            if (node is ScriptVariableGlobal v) { Names.Add(v.Name); }
            base.Visit(node);
        }
    }

    var template = Template.Parse("{{ a }} {{ b.c }}");
    var collector = new VariableCollector();
    collector.Visit(template.Page);

FORMATTING AND PRINTING
-----------------------
    class ScriptFormatter : ScriptRewriter
        ScriptFormatter(ScriptFormatterOptions options)
        readonly ScriptFormatterOptions Options
        ScriptNode Format(ScriptNode node)

    class ScriptFormatterOptions
        ScriptFormatterOptions(ScriptFormatterFlags flags)
        ScriptFormatterOptions(TemplateContext context, ScriptLang language,
                               ScriptFormatterFlags flags)
        readonly ScriptLang Language; readonly ScriptFormatterFlags Flags;
        readonly TemplateContext Context

    [Flags] enum ScriptFormatterFlags
        None = 0
        ExplicitParenthesis      = 1 << 0
        AddSpaceBetweenOperators = 1 << 1
        RemoveExistingTrivias    = 1 << 2
        CompressSpaces           = 1 << 3
        MinimizeParenthesisNesting = 1 << 4
        Clean         = AddSpaceBetweenOperators | CompressSpaces |
                        MinimizeParenthesisNesting
        ExplicitClean = ExplicitParenthesis | Clean

    static class ScriptFormatterExtensions
        static ScriptNode Format(this ScriptNode node, ScriptFormatterOptions options)
        static bool HasFlags(this ScriptFormatterFlags input,
                             ScriptFormatterFlags flags)

    class ScriptPrinter
        ScriptPrinter(IScriptOutput output,
                      ScriptPrinterOptions options = default)
        readonly ScriptPrinterOptions Options
        ScriptPrinter Write(ScriptNode node)
        ScriptPrinter Write(string text)
        ScriptPrinter Write(ScriptStringSlice slice)
        ScriptPrinter ExpectEos()
        ScriptPrinter ExpectSpace()
        ScriptPrinter WriteListWithCommas<T>(IList<T> list) where T : ScriptNode
        ScriptPrinter WriteEnterCode(int escape = 0)
        ScriptPrinter WriteExitCode(int escape = 0)
        bool PreviousHasSpace { get; }

    struct ScriptPrinterOptions { ScriptMode Mode; }

Format a template and print it back:

    var template = Template.Parse("  x   +2  ; ", null, null,
                                  new LexerOptions { KeepTrivia = true,
                                                     Mode = ScriptMode.ScriptOnly });
    var formatted = template.Page.Format(
        new ScriptFormatterOptions(ScriptFormatterFlags.Clean));
    string text = formatted.ToString();         // "x + 2"

template.ToText(options) round-trips the whole template back to source.

DRIVING THE LEXER AND PARSER DIRECTLY
-------------------------------------
    class Lexer
        Lexer(string text, string sourcePath = null, LexerOptions options = null)
        readonly LexerOptions Options
        string Text { get; }
        string SourcePath { get; }
        bool   HasErrors { get; }
        IEnumerable<LogMessage> Errors { get; }
        Enumerator GetEnumerator()            // foreach over Token

    class Parser
        Parser(Lexer lexer, ParserOptions options = null)
        readonly ParserOptions Options
        LogMessageBag Messages { get; }
        bool          HasErrors { get; }
        SourceSpan    CurrentSpan { get; }
        ScriptPage    Run()

    struct Token : IEquatable<Token>
        Token(TokenType type, TextPosition start, TextPosition end)
        readonly TokenType Type; readonly TextPosition Start; TextPosition End
        string GetText(string text)
        bool   Match(string textToMatch, string lexerText)
        static readonly Token Eof

    enum TokenType          // CodeEnter "{{", CodeExit "}}", LiquidTagEnter
                            // "{%", LiquidTagExit "%}", Raw, Escape,
                            // EscapeEnter, EscapeExit, NewLine, Whitespace,
                            // WhitespaceFull, Comment, CommentMulti,
                            // Identifier, IdentifierSpecial, Integer,
                            // HexaInteger, BinaryInteger, Float, String,
                            // VerbatimString, ImplicitString, the four
                            // Interpolated*String kinds, every operator and
                            // punctuation token, Custom..Custom9, Eof
    static class TokenTypeExtensions        // ToText() for a TokenType

Use the lexer for syntax highlighting; TokenType.ToText() gives the literal
spelling of an operator token.


RUNTIME EXTRAS
==============

    class ScriptArray<T> : IList<T>, IList, IScriptObject,
                           IScriptCustomBinaryOperation, IScriptTransformable
        ScriptArray(); ScriptArray(int capacity); ScriptArray(T[] array)
        ScriptArray(IEnumerable<T> values); ScriptArray(IEnumerable values)
        T this[int index] { get; set; }   // the setter auto-expands the list
        int Count { get; }  int Capacity { get; set; }
        void AddRange(IEnumerable<T> items)
        T[] ToArray()
        ScriptObject ScriptObject { get; }   // extra named members on the array
        bool IsReadOnly { get; set; }
        IScriptObject Clone(bool deep)

    class ScriptArray : ScriptArray<object>
        static ScriptArray From(object obj)   // IEnumerable or a JSON array

    class ScriptRange : IList<object>, IList, IScriptTransformable,
                        IScriptCustomBinaryOperation
        ScriptRange(); ScriptRange(IEnumerable values)
        IEnumerable Values { get; }
        static ScriptRange Offset(IEnumerable list, int index)
        static ScriptRange Limit(IEnumerable list, int count)
        static ScriptRange Compact(IEnumerable list)
        static ScriptRange Uniq(IEnumerable list)
        static ScriptRange Reverse(IEnumerable list)
        static ScriptRange Concat(IEnumerable left, IEnumerable right)
        static ScriptRange BinaryOr / BinaryAnd / ShiftLeft / ShiftRight /
                           Multiply / Divide / Modulus (...)

ScriptRange is the lazy sequence returned by the streaming array functions
(array.each, array.filter, and the static helpers above). A range expression
such as `1..5` yields its own lazy IEnumerable and is checked against
TemplateContext.LoopLimit at evaluation time: a range wider than LoopLimit
raises ScriptRuntimeException before a single item is produced.

    sealed class EmptyScriptObject : IScriptObject
        static readonly EmptyScriptObject Default

EmptyScriptObject.Default is registered as both `empty` and `blank`; comparing
a value against it is how a template asks "is this empty?" in Liquid style.

    class ScriptLazy<T> : IScriptCustomFunction
        ScriptLazy(Func<T> valueFactory)

    readonly struct ScriptStringSlice
        ScriptStringSlice(string fullText)
        ScriptStringSlice(string fullText, int index, int length)
        readonly string FullText; readonly int Index; readonly int Length
        char this[int index] { get; }
        string Substring(int index)
    static class ScriptStringSliceExtensions

ScriptStringSlice is an allocation-free window over the template source; raw
text statements hold one instead of copying the string.

Two low-level helpers are public but rarely useful outside the engine:
CharHelper (IsHexa, IsBinary, TryParseDigit, TryHexaToInt) and the
ReflectionHelper extension methods on Type (IsPrimitiveOrDecimal, IsNumber,
ScriptPrettyName -- the last is what object.typeof and error messages use to
name a .NET type).


COMPLETE EXAMPLES
=================

1. BASIC RENDER WITH A .NET MODEL
---------------------------------
    using System;
    using CodeBrix.Templating;

    public class Invoice
    {
        public string CustomerName { get; set; }
        public decimal TotalDue { get; set; }
    }

    public static class BasicRender
    {
        public static string Run()
        {
            var template = Template.Parse(
                "Dear {{ customer_name }}, you owe {{ total_due }}.");

            if (template.HasErrors)
            {
                throw new InvalidOperationException(template.Messages.ToString());
            }

            var model = new Invoice { CustomerName = "Ada", TotalDue = 42.50m };
            return template.Render(model);
            // "Dear Ada, you owe 42.50."
        }
    }

Note the snake_case names: StandardMemberRenamer turned CustomerName into
customer_name. Pass memberRenamer: member => member.Name to keep the .NET
spelling.

2. LOOP, PIPE AND INCLUDE WITH A TEMPLATE LOADER
------------------------------------------------
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CodeBrix.Templating;
    using CodeBrix.Templating.Parsing;
    using CodeBrix.Templating.Runtime;

    public class DictionaryTemplateLoader : ITemplateLoader
    {
        private readonly Dictionary<string, string> _templates;
        public DictionaryTemplateLoader(Dictionary<string, string> templates)
            => _templates = templates;

        public string GetPath(TemplateContext context, SourceSpan callerSpan,
                              string templateName) => templateName;

        public string Load(TemplateContext context, SourceSpan callerSpan,
                           string templatePath) => _templates[templatePath];

        public ValueTask<string> LoadAsync(TemplateContext context,
                                           SourceSpan callerSpan,
                                           string templatePath)
            => ValueTask.FromResult(Load(context, callerSpan, templatePath));
    }

    public static class LoopPipeInclude
    {
        public static async Task<string> RunAsync()
        {
            var loader = new DictionaryTemplateLoader(new Dictionary<string, string>
            {
                // $1 is the FIRST argument passed to include ($0 is the
                // template name itself)
                ["row"] = "  * {{ $1 | string.upcase }}\n",
            });

            var context = new TemplateContext { TemplateLoader = loader };
            var globals = new ScriptObject();
            globals["fruits"] = new[] { "apple", "pear", "fig" };
            context.PushGlobal(globals);

            var template = Template.Parse(
                "Fruit ({{ fruits | array.size }}):\n" +
                "{{ for fruit in (fruits | array.sort); include 'row' fruit; end }}");

            return await template.RenderAsync(context);
            // "Fruit (3):\n  * APPLE\n  * FIG\n  * PEAR\n"
        }
    }

3. A CUSTOM FUNCTION AND A CUSTOM RENAMER
-----------------------------------------
    using System;
    using System.Globalization;
    using CodeBrix.Templating;
    using CodeBrix.Templating.Runtime;

    public static class MoneyFunctions
    {
        public static string Money(decimal value, string currency = "USD")
            => string.Format(CultureInfo.InvariantCulture, "{0} {1:0.00}",
                             currency, value);

        [ScriptMemberIgnore]
        public static void InternalHelper() { }
    }

    public static class CustomFunctionSample
    {
        public static string Run()
        {
            var globals = new ScriptObject();
            globals.Import(typeof(MoneyFunctions));     // -> {{ money 3 }}
            globals.Import("now_utc",
                           new Func<DateTime>(() => DateTime.UtcNow));
            globals["amount"] = 12.5m;

            var context = new TemplateContext();
            context.PushGlobal(globals);

            var template = Template.Parse("{{ amount | money 'EUR' }}");
            return template.Render(context);      // "EUR 12.50"
        }
    }

4. A LIQUID TEMPLATE
--------------------
    using CodeBrix.Templating;
    using CodeBrix.Templating.Runtime;

    public static class LiquidSample
    {
        public static string Run()
        {
            var template = Template.ParseLiquid(
                "{% assign greeting = 'hello world' %}" +
                "{{ greeting | capitalize | truncate: 8 }}" +
                "{% for i in (1..3) %}[{{ forloop.index }}:{{ i }}]{% endfor %}");
            // "Hello...[1:1][2:2][3:3]"

            var context = new LiquidTemplateContext();
            return template.Render(context);
        }
    }

   Use LiquidTemplateContext (not TemplateContext) whenever the template came
   from ParseLiquid and you supply the context yourself.

5. ERROR HANDLING END TO END
----------------------------
    using System;
    using System.Threading;
    using CodeBrix.Templating;
    using CodeBrix.Templating.Parsing;
    using CodeBrix.Templating.Runtime;
    using CodeBrix.Templating.Syntax;

    public static class SafeRenderer
    {
        public static bool TryRender(string text, object model,
                                     TimeSpan timeout, out string result,
                                     out string error)
        {
            result = null;
            error = null;

            var template = Template.Parse(text, "user-template");
            if (template.HasErrors)
            {
                foreach (var message in template.Messages)
                {
                    if (message.Type == ParserMessageType.Error)
                    {
                        error = message.ToString();   // file(line,col) : error : ...
                        return false;
                    }
                }
            }

            using var cts = new CancellationTokenSource(timeout);
            var context = new TemplateContext
            {
                StrictVariables = true,
                LoopLimit = 500,
                RecursiveLimit = 25,
                CancellationToken = cts.Token,
            };
            var globals = new ScriptObject();
            globals.Import(model);
            context.PushGlobal(globals);

            try
            {
                result = template.Render(context);
                return true;
            }
            catch (ScriptAbortException)
            {
                error = "The template took too long to render.";
            }
            catch (ScriptParserRuntimeException ex)
            {
                error = ex.Message + " " + ex.ParserMessages.ToString();
            }
            catch (ScriptRuntimeException ex)
            {
                error = ex.Message;                   // includes the location
            }
            return false;
        }
    }


MINIMUM VIABLE PROJECT
======================

MyRenderer.csproj:

    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net10.0</TargetFramework>
        <Nullable>disable</Nullable>
      </PropertyGroup>
      <ItemGroup>
        <PackageReference Include="CodeBrix.Templating.BsdLicenseForever" />
      </ItemGroup>
    </Project>

Program.cs:

    using System;
    using CodeBrix.Templating;
    using CodeBrix.Templating.Runtime;

    var template = Template.Parse("Hello {{ name }}, {{ items | array.size }} items.");
    if (template.HasErrors)
    {
        Console.Error.WriteLine(template.Messages.ToString());
        return 1;
    }

    var globals = new ScriptObject();
    globals["name"] = "World";
    globals["items"] = new[] { 1, 2, 3 };

    var context = new TemplateContext();
    context.PushGlobal(globals);

    Console.WriteLine(template.Render(context));
    return 0;

(Use the package-version pinning your solution already uses -- Central Package
Management or an explicit Version attribute.)


PERFORMANCE TIPS
================

* PARSE ONCE, RENDER MANY. A Template is immutable after Parse; cache it and
  call Render repeatedly. Parsing is by far the expensive step.
* REUSE A TemplateContext when the globals do not change. Building the
  builtin table and importing globals is done once per context.
* PREFER ScriptObject OVER REFLECTION. Render(object model) imports the model
  by reflection on every call; pushing a pre-built ScriptObject avoids that
  and is also trim/AOT friendly.
* PUSH/POP INSTEAD OF REBUILDING. context.PushGlobal(perRequestObject) and
  PopGlobal() around a render is much cheaper than a new context.
* RENDER STRAIGHT INTO YOUR SINK. context.PushOutput(new TextWriterOutput(writer))
  writes to a stream instead of building a string in memory.
* USE RenderAsync when the model exposes async delegates or when the template
  loader does I/O -- the sync path blocks on those.
* KEEP LOOPS BOUNDED. LoopLimit (1000) applies per loop and to range
  expressions; raise it deliberately rather than setting it to 0.
* array.each / array.filter and range expressions are lazy; array.sort,
  array.uniq and array.reverse materialise. Order pipelines so filtering
  happens before sorting.
* StringBuilderOutput.GetThreadInstance() gives a reusable per-thread buffer
  for hot paths that render many small fragments.
* The engine needs no runtime code generation, so it works under trimming and
  NativeAOT -- but avoid the reflection-based Render(object model) /
  Import(object) / ScriptObject.From overloads there; they are marked
  [RequiresUnreferencedCode].


COMMON PITFALLS TO AVOID
========================

DO NOT use Scriban.* namespaces. Every type is under CodeBrix.Templating.*.
The package id is CodeBrix.Templating.BsdLicenseForever but the namespace and
assembly are CodeBrix.Templating.

DO NOT pass Render's arguments positionally in the wrong order. The signature
is Render(object model, MemberRenamerDelegate memberRenamer,
MemberFilterDelegate memberFilter) -- RENAMER first. And on
ScriptObjectExtensions.Import it is the other way round: (object obj,
MemberFilterDelegate filter, MemberRenamerDelegate renamer). Always use named
arguments.

DO NOT expect .NET member names in templates. The default renamer converts
PascalCase to snake_case, so `CustomerName` is `customer_name` and
`ToString` is `to_string`. Set MemberRenamer to `member => member.Name` if
you want the original spelling.

DO NOT call ScriptObject.Import without `using CodeBrix.Templating.Runtime;`.
The Import methods are extension methods on IScriptObject declared in
ScriptObjectExtensions, not instance methods -- without the using they simply
do not resolve.

DO NOT assign TemplateContext.Output; it is a read-only property. Use
PushOutput(IScriptOutput) / PopOutput().

DO NOT assign TemplateContext.CurrentCulture; it is read-only and defaults to
CultureInfo.InvariantCulture. Use PushCulture / PopCulture. Rendering is
culture-invariant unless you push a culture, which is usually what you want
for machine-readable output and a surprise for human-readable output.

DO NOT reuse a TemplateContext whose output is a TextWriterOutput and expect
Render(context) to return only the latest render. Render returns
context.Output.ToString() and only resets the buffer when the output is a
StringBuilderOutput; with a TextWriterOutput the returned string keeps
growing. Read the writer yourself and ignore the return value.

DO NOT call `include` without setting TemplateContext.TemplateLoader -- it
throws at render time. And sanitise template names inside GetPath if they can
come from user input.

DO NOT read the first include argument as $0. Inside an included template the
arguments array starts with the template NAME, so the first value you passed
is $1. Inside a `func` the first argument is $0. Named arguments are reachable
as $.name (and, when the name does not collide, as $name).

DO NOT render a Liquid template with a plain TemplateContext. Template
.ParseLiquid(...).Render(model) builds a LiquidTemplateContext for you, but
Render(context) uses exactly the context you pass; a plain TemplateContext
has none of the Liquid filter aliases.

DO NOT expect every Liquid filter to exist. `escape_once` is present in the
Liquid-to-Scriban name map but is NOT registered as a callable builtin, so
`{{ x | escape_once }}` fails at runtime. Verify unusual filters before
relying on them.

DO NOT iterate a range wider than LoopLimit. `{{ for i in 1..100000 }}`
raises "Range expression exceeds LoopLimit" with the default limit of 1000.
Raise LoopLimit explicitly for large loops.

DO NOT assume a missing variable is an error. By default a missing global
renders as an empty string; set StrictVariables = true to make it throw.
EnableRelaxedMemberAccess is true by default too, so `a.b.c` on a null `a`
yields null rather than failing -- turn it off for strict templates.

DO NOT ignore Template.HasErrors. Render/Evaluate on a template with parse
errors throws InvalidOperationException, not a ScriptRuntimeException.

DO NOT format dates with .NET format strings. date.to_string uses
strftime-style patterns (%d %b %Y); math.format and object.format are the
ones that take .NET format strings.

DO NOT treat html.strip as a sanitiser. It is a regex-based tag stripper and
can mis-handle malformed HTML; use a real HTML parser for security work.

DO NOT run untrusted templates without limits. object.eval and
object.eval_template execute arbitrary script; LoopLimit, RecursiveLimit,
ObjectRecursionLimit, LimitToString, RegexTimeOut and CancellationToken are
the controls that keep a hostile template from hanging the process.

DO NOT rely on parsing succeeding for AST work without trivia. Formatting and
faithful re-printing need LexerOptions.KeepTrivia = true; without it comments
and spacing are discarded.


WHAT THIS PACKAGE DOES NOT DO
=============================

* It does not compile templates to IL or emit code at runtime; evaluation is
  a tree-walking interpreter. That is what makes it trim- and AOT-safe.
* It does not sandbox .NET. Anything you import into the globals is callable
  from the template; the engine limits loops, recursion and time, not access.
* It does not sanitise HTML, escape output automatically, or provide
  contextual auto-escaping. Call html.escape / html.url_encode yourself.
* It does not read templates from disk on its own. `include` needs an
  ITemplateLoader you provide; there is no built-in file-system loader.
* It does not implement every Liquid tag or filter from every Liquid dialect,
  and it does not implement Shopify-specific tags. The Liquid support is the
  compatible subset listed above.
* It does not localise or format for a culture unless you push one; the
  default culture is invariant.
* It does not provide a designer, a language server, or MSBuild/source-
  generator integration. It is a runtime library.
* It does not render Markdown, PDF, images, or any binary format. It produces
  text.


WORKING EXAMPLES ON GITHUB
==========================

The test project is the most complete worked example of every feature.

    Parsing and the lexer
      https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests/TestParser.cs
      https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests/TestLexer.cs
    Runtime, ScriptObject, imports, renamers, limits
      https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests/TestRuntime.cs
    Built-in functions
      https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests/TestFunctions.cs
    Async rendering and async delegates
      https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests/TestAsync.cs
    ITemplateLoader, include and include_join
      https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests/TestIncludes.cs
      https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests/CustomTemplateLoader.cs
    ScriptVisitor / ScriptRewriter
      https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests/ScriptRewriterTests.cs
    ScriptFormatter and ScriptFormatterFlags
      https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests/TestFormatter.cs
    ScriptRuntimeException behaviour
      https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests/ScriptRuntimeExceptionTests.cs
    JSON bridge (object.from_json / to_json, JsonElement import)
      https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests/TestJsonSupport
    LINQ / IQueryable models
      https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests/TestQueryable.cs

Template-language behaviour is pinned by input/expected-output file pairs --
each `*.txt` is a template and the matching `*.out.txt` is exactly what it
renders to. They are the authoritative reference for the syntax:

    https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests/TestFiles
      000-basic/          raw text, comments, whitespace control, indentation
      010-literals/       every literal form
      020-interpolation/  interpolated strings
      100-expressions/    operators, ranges, members, indexers, initializers
      200-statements/     if/case/for/tablerow/while/capture/import/with/ret
      300-functions/      func, pipes, named/optional/variadic args, wrap
      400-builtins/       one file per builtin table, plus the full name list
      500-liquid/         Liquid tags, filters, raw and comment blocks
      600-ast/            AST round-tripping

Liquid compatibility cases adapted from DotLiquid:

    https://github.com/ellisnet/CodeBrix.Templating/tree/main/tests/CodeBrix.Templating.Tests/LiquidTests


QUICK REFERENCE CARD
====================

    Package     CodeBrix.Templating.BsdLicenseForever   (BSD-2-Clause)
    Namespace   CodeBrix.Templating[.Runtime|.Parsing|.Syntax|.Functions]
    Framework   .NET 10 or later, no native dependencies

    PARSE       Template.Parse(text[, sourceFilePath, parserOptions, lexerOptions])
                Template.ParseLiquid(text[, ...])
                template.HasErrors / template.Messages
    RENDER      template.Render(model[, memberRenamer, memberFilter])
                template.Render(context)
                await template.RenderAsync(model | context)
                template.Evaluate(...) -> last value instead of text
                template.ToText()      -> AST back to source

    MODEL       var g = new ScriptObject();
                g["key"] = value;
                g.Import(obj [, filter, renamer]);   // extension method
                g.Import(typeof(MyFunctions));
                g.Import("name", someDelegate);
                g.Import(jsonElement);
                context.PushGlobal(g); ... context.PopGlobal();

    OUTPUT      context.PushOutput(new TextWriterOutput(writer));
                context.PopOutput();

    LIMITS      LoopLimit 1000, RecursiveLimit 100, ObjectRecursionLimit 20,
                LimitToString 1048576, RegexTimeOut 10s, CancellationToken
    STRICTNESS  StrictVariables, EnableRelaxedMemberAccess (true),
                EnableRelaxedTargetAccess, EnableRelaxedFunctionAccess,
                EnableRelaxedIndexerAccess (true), EnableNullIndexer

    SYNTAX      {{ expr }}   {{- trim -}}   {{~ trim ~}}   {%{ escaped }%}
                {{ # comment }}   {{ ## multi ## }}
                if/else if/else ... end        case/when/else ... end
                for x in list [offset: n] [limit: n] [reversed] ... end
                tablerow x in list [cols: n] ... end     while cond ... end
                break   continue   ret   capture v ... end   with o ... end
                import o   readonly v   func name ... end   wrap call ... end
                a | fn arg    @fn    ^array    $0 $1 $ $$    1..5   1..<5
                a ?? b   a ?! b   a?.b   a.empty?   cond ? x : y
                $"interpolated {expr}"

    BUILTINS    array.*  string.*  math.*  date.*  timespan.*  object.*
                regex.*  html.*  include  include_join  empty  blank

    ERRORS      ScriptRuntimeException (Span, OriginalMessage)
                ScriptParserRuntimeException (ParserMessages)
                ScriptAbortException (CancellationToken)
                ScriptArgumentException (ArgumentIndex)
                LogMessage / LogMessageBag / ParserMessageType

    EXTEND      IScriptCustomFunction / IScriptFunctionInfo
                DelegateCustomFunction.Create / CreateFunc
                DynamicCustomFunction.Create   DelegateCustomAction
                ScriptLazy<T>   ITemplateLoader   IObjectAccessor
                IListAccessor   [ScriptMemberIgnore]   StandardMemberRenamer

    AST         template.Page (ScriptPage) -> ScriptNode tree
                ScriptVisitor / ScriptVisitor<T> / ScriptRewriter
                ScriptFormatter + ScriptFormatterFlags.Clean
                ScriptPrinter + ScriptPrinterOptions
                Lexer / Parser / Token / TokenType / SourceSpan / TextPosition


END OF AGENT-README
