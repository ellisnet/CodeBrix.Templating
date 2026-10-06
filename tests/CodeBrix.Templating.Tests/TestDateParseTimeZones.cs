// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using Xunit;
using CodeBrix.Templating.Functions;

namespace CodeBrix.Templating.Tests;

public class TestDateParseTimeZones
{
    [Theory]
    [InlineData("2021/11/30 09:50:23Z", null, "en-US", 2021, 11, 30, 9, 50, 23, 0)]
    [InlineData("20/01/2022 08:32:48 +00:00", null, "en-GB", 2022, 1, 20, 8, 32, 48, 0)]
    [InlineData("20/01/2022 08:32:48 +05:30", null, "en-GB", 2022, 1, 20, 8, 32, 48, 330)]
    [InlineData("20/01/2022 08:32:48 +05:30", "%d/%m/%Y %H:%M:%S %Z", "en-GB", 2022, 1, 20, 8, 32, 48, 330)]
    public void ParseOffsetConvertsToLocalTime(string text, string pattern, string culture,
        int year, int month, int day, int hour, int minute, int second, int offsetMinutes)
    {
        var expected = new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.FromMinutes(offsetMinutes));
        var actual = DateTimeFunctions.Parse(new TemplateContext(), text, pattern, culture)
            ?? throw new Xunit.Sdk.XunitException("Expected a parsed date.");

        Assert.Equal(expected.LocalDateTime, actual);
        Assert.Equal(DateTimeKind.Local, actual.Kind);
        Assert.Equal(expected.UtcDateTime, actual.ToUniversalTime());
    }
}
