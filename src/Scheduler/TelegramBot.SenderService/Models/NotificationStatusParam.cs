namespace TelegramBot.SenderService.Models;

public record NotificationStatusParam(int NotificationId, string Status, string ErrorMessage = "", int Retry = 0)
{
    public string ToXml()
        => $"<r NotificationId=\"{NotificationId}\" Status=\"{Status}\" ErrMsg=\"{ErrorMessage}\" Retry=\"{Retry}\" />";
}
