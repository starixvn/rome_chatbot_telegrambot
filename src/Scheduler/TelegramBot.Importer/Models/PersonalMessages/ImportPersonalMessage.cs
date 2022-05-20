namespace TelegramBot.Importer.Models.PersonalMessages;

public record ImportPersonalMessage(
    int UserId,
    int MsgTargetId,
    int MessageId,
    string Message,
    string Status)
{
    public string ToXml()
        => $"<r UserId=\"{UserId}\" MsgTargetId=\"{MsgTargetId}\" MessageId=\"{MessageId}\" Message=\"{Message}\" Status=\"{Status}\" />";
}