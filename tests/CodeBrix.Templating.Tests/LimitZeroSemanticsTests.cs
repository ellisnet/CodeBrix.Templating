using Xunit;
using CodeBrix.Templating.Runtime;
using CodeBrix.Templating.Syntax;

namespace CodeBrix.Templating.Tests;

/// <summary>
/// Every limit on <see cref="TemplateContext"/> reads 0 - and any negative value - as "no limit".
/// Before 2026-10-10 a negative LoopLimit / RecursiveLimit / ObjectRecursionLimit threw on the very first
/// iteration, call or level, while a negative LoopLimit was already "no limit" for range expressions.
/// </summary>
public class LimitZeroSemanticsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void loop_limit_zero_or_negative_means_no_limit(int limit)
    {
        //Arrange
        var context = new TemplateContext { LoopLimit = limit };
        var template = Template.Parse("{{ for i in 1..2500 }}{{ end }}{{ 1..2500 | array.size }}");

        //Act
        var result = template.Render(context);

        //Assert
        Assert.Equal("2500", result);
    }

    [Fact]
    public void loop_limit_positive_is_enforced()
    {
        //Arrange
        var context = new TemplateContext { LoopLimit = 10 };
        var template = Template.Parse("{{ for i in 1..20 }}{{ end }}");

        //Act + Assert
        Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void recursive_limit_zero_or_negative_means_no_limit(int limit)
    {
        //Arrange
        var context = new TemplateContext { RecursiveLimit = limit };
        var template = Template.Parse("{{ func f(n); ret n <= 0 ? 0 : f(n - 1) + 1; end; f 150 }}");

        //Act
        var result = template.Render(context);

        //Assert
        Assert.Equal("150", result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void object_recursion_limit_zero_or_negative_means_no_limit(int limit)
    {
        //Arrange
        var context = new TemplateContext { ObjectRecursionLimit = limit };
        var template = Template.Parse("{{ x = [1]; for i in 1..40; x = [x]; end; x }}");

        //Act
        var result = template.Render(context);

        //Assert
        Assert.Contains("1", result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void limit_to_string_zero_or_negative_means_no_limit(int limit)
    {
        //Arrange
        var context = new TemplateContext { LimitToString = limit, LoopLimit = 0 };
        var template = Template.Parse("{{ ('x' * 3000) | string.size }}");

        //Act
        var result = template.Render(context);

        //Assert
        Assert.Equal("3000", result);
    }
}
