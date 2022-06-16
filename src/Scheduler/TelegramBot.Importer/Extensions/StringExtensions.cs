namespace TelegramBot.Importer.Extensions;

public static class StringExtensions
{
    public static string[] SplitComma(this string text)
        => text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

    public static string FormatDecimal(this decimal value)
        => value.ToString("N2");

    public static string FormatDecimalUptoTwoDigit(this decimal value)
        => value.ToString("0.##");

    public static string FormatDecimalToInt(this decimal value)
        => value.ToString("0");
}