namespace TelegramBot.Importer.Models.AbnormalLogin;

public record AbnormalLoginNotification(
    int UserId,
    string Message,
    int BotId,
    bool IsRealUser,
    bool IsJobUAT,
    long messageId)
    : Notification(UserId, BotId, Message, IsRealUser, IsJobUAT)
{
    public override string ToXml()
        => $"<r UserId=\"{UserId}\" BotId=\"{BotId}\" MessageId=\"{messageId}\" Message=\"{Message}\" Status=\"{Status}\" />";
}
