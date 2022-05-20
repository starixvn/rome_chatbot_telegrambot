namespace TelegramBot.Importer.Extensions;

public static class DateTimeExtensions
{
    private const string SimpleDate = "yyyyMMdd";
    private const string LongDateTime12h = "yyyy-MM-dd hh:mm:ss tt";

    public static string ToSimpleDate(this DateTime dateTime)
        => dateTime.ToString(SimpleDate);

    public static string ToLongDateTime12h(this DateTime dateTime)
        => dateTime.ToString(LongDateTime12h);
}