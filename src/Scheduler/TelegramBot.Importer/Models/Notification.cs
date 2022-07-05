namespace TelegramBot.Importer.Models;

public record Notification(int UserId, int BotId, string Message, bool IsRealUser, bool IsJobUAT)
{
    private const string Draft = "Draft";
    private const string Initial = "Initial";

    public string Status => IsRealUser && IsJobUAT ? Draft : Initial;

    public virtual string ToXml()
        => $"<r UserId=\"{UserId}\" BotId=\"{BotId}\" Message=\"{Message}\" Status=\"{Status}\"/>";

    public virtual string ToLogXml()
        => string.Empty;
}