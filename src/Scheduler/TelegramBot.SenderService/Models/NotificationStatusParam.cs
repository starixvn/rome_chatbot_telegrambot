namespace TelegramBot.SenderService.Models;

public record NotificationStatusParam(int NotificationId, string Status, string ErrorMessage = "")
{
    public string ToXml()
        => $"<r NotificationId=\"{NotificationId}\" Status=\"{Status}\" ErrMsg=\"{ErrorMessage}\"/>";
}
