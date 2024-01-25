namespace TelegramBot.Importer.Models.AutoTransfer;

public record AutoTransferAlert(
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
