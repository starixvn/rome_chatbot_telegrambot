namespace TelegramBot.SenderService.Constants;

public static class NotificationStatus
{
    public const string Completed = "Completed";

    // Notifications cannot be sent due to a business error.
    public const string Invalid = "Invalid";

    // Notifications cannot be sent due to a technical error
    public const string Fail = "Fail";
}