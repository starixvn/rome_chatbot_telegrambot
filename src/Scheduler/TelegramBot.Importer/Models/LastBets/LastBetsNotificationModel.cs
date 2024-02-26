namespace TelegramBot.Importer.Models.LastBets;

public record LastBetsNotificationModel(
    int UserId,
    string Message,
    int BotId,
    bool IsRealUser,
    bool IsJobUAT)
    : Notification(UserId, BotId, Message, IsRealUser, IsJobUAT)
{
    public override string ToXml()
        => $"<r UserId=\"{UserId}\" BotId=\"{BotId}\" Message=\"{Message}\" Status=\"{Status}\" />";
}
