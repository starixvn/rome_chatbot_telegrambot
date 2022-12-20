namespace WLMonitored.Shared.Extensions;

public static class DateTimeExtensions
{
    private const string SimpleDate = "MM/dd/yyyy";

    public static string ToSimpleDate(this DateTime dateTime)
        => dateTime.ToString(SimpleDate);
}