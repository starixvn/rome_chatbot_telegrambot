namespace TelegramBot.SenderService.Constants;

public static class TelegramError
{
    public const int FORBIDDEN = 403;
    private static readonly int[] ignoredErrorCodes = new[] { 400, 401, 404, 406 };
    public static int[] IgnoredErrorCodes => ignoredErrorCodes;
}