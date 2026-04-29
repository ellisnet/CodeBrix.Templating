using System;
using Xunit;
using CodeBrix.Templating.Parsing;
using CodeBrix.Templating.Syntax;

namespace CodeBrix.Templating.Tests; //was previously: Scriban.Tests;

public class ScriptRuntimeExceptionTests
{
    [Fact]
    public void TestInnerExceptionExists()
    {
        ScriptRuntimeException.EnableDisplayInnerException = true;
        SourceSpan testSourcespanObject = new SourceSpan("fileName", new TextPosition(0, 0, 0), new TextPosition(0, 0, 0));
        Exception exception = new StubException("Test RunTime message", "TestStacTrace");

        ScriptRuntimeException testScriptruntimeObject = new ScriptRuntimeException(testSourcespanObject, "Any string", exception);

        Assert.Contains("TestStacTrace", testScriptruntimeObject.ToString());
        Assert.Contains("Test RunTime message", testScriptruntimeObject.ToString());
    }

    [Fact]
    public void TestInnerExceptiondosentExists()
    {
        ScriptRuntimeException.EnableDisplayInnerException = true;
        SourceSpan testSourcespanObject = new SourceSpan("fileName", new TextPosition(0, 0, 0), new TextPosition(0, 0, 0));

        ScriptRuntimeException testScriptruntimeObject = new ScriptRuntimeException(testSourcespanObject, "Any string");

        Assert.Equal(testScriptruntimeObject.Message, testScriptruntimeObject.ToString());
    }

    [Fact]
    public void TestInnerExceptionDisabled()
    {
        ScriptRuntimeException.EnableDisplayInnerException = false;
        SourceSpan testSourcespanObject = new SourceSpan("fileName", new TextPosition(0, 0, 0), new TextPosition(0, 0, 0));
        Exception exception = new StubException("Test RunTime message", "TestStacTrace");

        ScriptRuntimeException testScriptruntimeObject = new ScriptRuntimeException(testSourcespanObject, "Any string", exception);

        Assert.Equal(testScriptruntimeObject.Message, testScriptruntimeObject.ToString());
    }

    private sealed class StubException : Exception
    {
        private readonly string _stackTrace;
        public StubException(string message, string stackTrace) : base(message) { _stackTrace = stackTrace; }
        public override string StackTrace => _stackTrace;
    }
}
