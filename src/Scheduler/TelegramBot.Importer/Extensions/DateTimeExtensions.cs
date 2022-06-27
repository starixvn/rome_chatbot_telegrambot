namespace TelegramBot.Importer.Extensions;

public static class DateTimeExtensions
{
    private const string SimpleDate = "yyyyMMdd";
    private const string LongDateTime24h = "yyyy-MM-dd HH:mm:ss";
    private const string LongDateTime24hWithSlash = "MM/dd/yyyy HH:mm:ss";
    private const string Time24h = "HH:mm:ss";

    public static string ToSimpleDate(this DateTime dateTime)
        => dateTime.ToString(SimpleDate);

    public static string ToLongDateTime24h(this DateTime dateTime)
        => dateTime.ToString(LongDateTime24h);

    public static string ToLongDateTime24hWithSlash(this DateTime dateTime)
        => dateTime.ToString(LongDateTime24hWithSlash);

    public static string ToTime24h(this DateTime dateTime)
        => dateTime.ToString(Time24h);

    public static DateTime Yesterday(this DateTime dateTime)
        => dateTime.AddDays(-1);
}