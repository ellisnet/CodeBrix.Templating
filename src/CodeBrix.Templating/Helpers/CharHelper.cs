using System;

namespace CodeBrix.Templating.Helpers; //was previously: Scriban.Helpers;
/// <summary><c>CharHelper</c>.</summary>
public
class CharHelper
{
    /// <summary><c>IsHexa</c>.</summary>
    public static bool IsHexa(char c)
    {
        return Char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
    }
    /// <summary><c>TryParseDigit</c>.</summary>
    public static bool TryParseDigit(char c, out int value)
    {
        if (c >= '0' && c <= '9')
        {
            value = c - '0';
            return true;
        }
        value = 0;
        return false;
    }
    /// <summary><c>TryHexaToInt</c>.</summary>
    public static bool TryHexaToInt(char c, out int value)
    {
        if (c >= '0' && c <= '9')
        {
            value = c - '0';
            return true;
        }
        if (c >= 'A' && c <= 'F')
        {
            value = 10 + (c - 'A');
            return true;
        }
        if (c >= 'a' && c <= 'f')
        {
            value = 10 + (c - 'a');
            return true;
        }

        value = 0;
        return false;
    }
    /// <summary><c>IsBinary</c>.</summary>
    public static bool IsBinary(char c) => c == '0' || c == '1';
}
