using TelegramBot.Notifications.Shared.Constants;

namespace TelegramBot.Notifications.Shared.Params;

public record ImportPersonalMessageParam(
    int UserId,
    int MsgTargetId,
    int MessageId,
    string Message,
    string Status)
{
    public string ToXml()
        => $"<r UserId=\"{UserId}\" MsgTargetId=\"{MsgTargetId}\" MessageId=\"{MessageId}\" Message=\"{Message}\" Status=\"{Status}\" />";
}