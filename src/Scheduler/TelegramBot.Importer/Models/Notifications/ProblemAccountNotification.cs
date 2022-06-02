using System.Text;

namespace TelegramBot.Importer.Models.Notifications;

public record ProblemAccountNotification(
    int Id,
    int UserId,
    string Message,
    bool IsRealUser,
    bool IsJobUAT,
    IEnumerable<long> LogIds) : Notification(UserId, Message, IsRealUser, IsJobUAT)
{
    public override string ToXml()
        => $"<r Id=\"{Id}\" UserId=\"{UserId}\" Message=\"{Message}\" Status=\"{Status}\" />";

    public override string ToLogXml()
    {
        var sb = new StringBuilder();
        
        foreach (var id in LogIds)
        {
            sb.Append(ToLogXml(id));
        }

        return sb.ToString();
    }

    private string ToLogXml(long logId)
        => $"<r Id=\"{Id}\" LogId=\"{logId}\"/>";
}