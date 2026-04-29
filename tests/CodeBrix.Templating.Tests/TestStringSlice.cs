// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using Xunit;
using CodeBrix.Templating.Syntax;

namespace CodeBrix.Templating.Tests; //was previously: Scriban.Tests;

public class TestStringSlice
{
    private static readonly string[] StringsToCompare = new string[]
    {
        null,
        "",
        "A",
        "ABCDEF",
        "BC",
        "Ad",
        "ABCDEFGHJIKL",
    };

    [InlineData("A")]
    [InlineData("ABCDEF")]
    [Theory]
    public void TestMixed(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            var subText = text.Substring(i);
            var slice = text.Slice(i);
            CompareString(subText, slice);
        }
    }

    [InlineData("     A    \n   B       ", 0, 6)]
    [InlineData("\n\n\n\n\n", 0, 3)]
    [InlineData("\n \n \n \n \n", 0, 3)]
    [Theory]
    public void TestTrim(string text, int index, int length)
    {
        for (int i = 0; i < text.Length - 1; i++)
        {
            for (int j = 1; j < length && i + j < text.Length; j++)
            {
                var subText = text.Substring(index, length);
                var slice = text.Slice(index, length);

                var newSlice = slice.TrimEnd();
                var newSubText = subText.TrimEnd();

                Assert.Equal(newSubText, newSlice.ToString());
            }
        }
    }

    [Fact]
    public void TestEmpty()
    {
        CompareString(string.Empty, ScriptStringSlice.Empty);
    }

    private static void CompareString(string subText, ScriptStringSlice slice)
    {
        Assert.Equal(subText, slice.ToString());

        Assert.Equal(subText.Length, slice.Length);

        if (slice.Length > 0)
        {
            Assert.True(slice.GetHashCode() != 0);
        }

        Assert.True(subText == slice, "String not comparing correctly: Expecting: {subText}, Result: {slice}");

        for (int j = 0; j < slice.Length; j++)
        {
            Assert.Equal(subText[j], slice[j]);
        }

        foreach (var compare in StringsToCompare)
        {
            {
                var result = subText.CompareTo(compare);
                var sliceResult = slice.CompareTo(compare);
                Assert.Equal(result, sliceResult);
            }

            var sliceEqualsCompare = compare is not null && slice == compare;
            var compareEqualsSlice = compare is not null && compare == slice;
            Assert.Equal(subText == compare, sliceEqualsCompare);
            Assert.Equal(subText == compare, compareEqualsSlice);
        }
    }
}
