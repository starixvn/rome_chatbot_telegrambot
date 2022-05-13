namespace TelegramBot.SenderService.Models;

public record LastNotification(int LastNotificationId, string FunctionKey)
{
    public string ToXml()
        => $"<r LastnotificationId=\"{LastNotificationId}\" FunctionKey=\"{FunctionKey}\"/>";
}
