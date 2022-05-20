namespace TelegramBot.Importer.Extensions;

public static class EnumerableExtensions
{
    public static string JoinWithComma<T>(this IEnumerable<T> list)
        => string.Join(',', list);
}