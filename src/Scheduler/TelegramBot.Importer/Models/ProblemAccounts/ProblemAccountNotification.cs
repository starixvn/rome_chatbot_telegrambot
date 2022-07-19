using System.Text;

namespace TelegramBot.Importer.Models.ProblemAccounts;

public record ProblemAccountNotification(
    int Id,
    int UserId,
    int BotId,
    string Message,
    bool IsRealUser,
    bool IsJobUAT,
    IEnumerable<long> LogIds) : Notification(UserId, BotId, Message, IsRealUser, IsJobUAT)
{
    public override string ToXml()
        => $"<r Id=\"{Id}\" UserId=\"{UserId}\" BotId=\"{BotId}\" Message=\"{Message}\" Status=\"{Status}\" />";

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