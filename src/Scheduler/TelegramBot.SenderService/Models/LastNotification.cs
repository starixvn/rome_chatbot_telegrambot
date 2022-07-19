namespace TelegramBot.SenderService.Models;

public record LastNotification(long LastNotificationId, string FunctionKey)
{
    public string ToXml()
        => $"<r LastnotificationId=\"{LastNotificationId}\" FunctionKey=\"{FunctionKey}\"/>";
}
