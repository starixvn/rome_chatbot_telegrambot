namespace TelegramBot.Importer.Models.MemberWinLossLimits;

public record MemberWinLossLimitNotification(
    int UserId,
    int BotId,
    string Message,
    bool IsRealUser,
    bool IsJobUAT,
    long LogId) : Notification(UserId, BotId, Message, IsRealUser, IsJobUAT)
{
    public override string ToLogXml()
        => $"<r UserId=\"{UserId}\" LogID=\"{LogId}\" />";
}