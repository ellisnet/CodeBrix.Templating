using Xunit;

namespace DotLiquid.Tests.Tags; //was previously: DotLiquid.Tests.Tags;

public class RawTests
{
    [Fact]
    public void TestTagInRaw ()
    {
        Helper.AssertTemplateResult ("{% comment %} test {% endcomment %}",
            "{% raw %}{% comment %} test {% endcomment %}{% endraw %}");
    }

    [Fact]
    public void TestOutputInRaw ()
    {
        Helper.AssertTemplateResult ("{{ test }}",
            "{% raw %}{{ test }}{% endraw %}");
    }

    [Fact]
    public void TestRawAndFollowing()
    {
        Helper.AssertTemplateResult("{{ test }}65",
            "{% raw %}{{ test }}{% endraw %}6{{ 5 }}");
    }
}
