using System.Diagnostics.CodeAnalysis;

namespace TelegramBot.Importer.Extensions;

[ExcludeFromCodeCoverage]
public static class EnumerableExtensions
{
    public static string JoinWithComma<T>(this IEnumerable<T> list)
        => string.Join(',', list);

    public static string Join<T>(this IEnumerable<T> list, string separator)
        => string.Join(separator, list);
}