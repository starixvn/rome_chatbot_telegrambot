namespace TelegramBot.SenderService.Constants;

public static class TelegramError
{
    private static readonly int[] ignoredErrorCodes = new[] { 400, 401, 403, 404, 406 };

    public static int[] IgnoredErrorCodes => ignoredErrorCodes;
}