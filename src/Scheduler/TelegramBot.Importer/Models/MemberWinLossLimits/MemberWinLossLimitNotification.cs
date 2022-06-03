using System.Text;

namespace TelegramBot.Importer.Models.MemberWinLossLimits;

public record MemberWinLossLimitNotification(
    int UserId,
    string Message,
    bool IsRealUser,
    bool IsJobUAT,
    IEnumerable<long> LogIds) : Notification(UserId, Message, IsRealUser, IsJobUAT)
{
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
        => $"<r UserId=\"{UserId}\" LogID=\"{logId}\" />";
}