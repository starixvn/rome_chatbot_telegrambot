namespace TelegramBot.Importer.Models.PersonalMessages;

public record PersonalMessageNotification(
    int UserId,
    int BotId,
    int MsgTargetId,
    int MessageId,
    string Message,
    bool IsRealUser,
    bool IsJobUAT) : Notification(UserId, BotId, Message, IsRealUser, IsJobUAT)
{
    public override string ToXml()
        => $"<r UserId=\"{UserId}\" BotId=\"{BotId}\" MsgTargetId=\"{MsgTargetId}\" MessageId=\"{MessageId}\" Message=\"{Message}\" Status=\"{Status}\" />";
}
