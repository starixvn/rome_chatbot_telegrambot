using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace TelegramBot.Importer.Extensions;

[ExcludeFromCodeCoverage]
public static class StringExtensions
{
    private const string HtmlTagPattern = "<.*?>";

    public static string StripHTML(this string value)
        => Regex.Replace(value, HtmlTagPattern, string.Empty);

    public static string[] SplitComma(this string text)
        => text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

    public static string FormatDecimal(this decimal value)
        => value.ToString("N2");

    public static string FormatDecimalUptoTwoDigit(this decimal value)
        => value.ToString("0.##");

    public static string FormatDecimalToInt(this decimal value)
        => value.ToString("0");
}