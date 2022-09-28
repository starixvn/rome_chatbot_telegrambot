namespace WLMonitored.Shared.Extensions;

public static class EnumerableExtensions
{
    public static string JoinWithComma<T>(this IEnumerable<T> list)
        => list.Any() ? string.Join(',', list) : string.Empty;
}