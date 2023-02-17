namespace TelegramBot.Shared.Constants;

public static class TelegramAPIErrors
{
    public const int BAD_REQUEST = 400;

    public const int FORBIDDEN = 403;

    public const string DuplicatedErrorMessage
        = "Bad Request: message is not modified: specified new message content and reply markup are exactly the same as a current content and reply markup of the message";
}