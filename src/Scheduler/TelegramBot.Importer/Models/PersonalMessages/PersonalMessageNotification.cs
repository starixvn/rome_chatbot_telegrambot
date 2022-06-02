namespace TelegramBot.Importer.Models.PersonalMessages;

public record PersonalMessageNotification(
    int UserId,
    int MsgTargetId,
    int MessageId,
    string Message,
    bool IsRealUser,
    bool IsJobUAT) : Notification(UserId, Message, IsRealUser, IsJobUAT)
{
    public override string ToXml()
        => $"<r UserId=\"{UserId}\" MsgTargetId=\"{MsgTargetId}\" MessageId=\"{MessageId}\" Message=\"{Message}\" Status=\"{Status}\" />";
}
