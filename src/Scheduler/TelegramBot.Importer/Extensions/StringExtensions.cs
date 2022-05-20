namespace TelegramBot.Importer.Extensions;

public static class StringExtensions
{
    public static string[] SplitComma(this string text)
        => text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
}