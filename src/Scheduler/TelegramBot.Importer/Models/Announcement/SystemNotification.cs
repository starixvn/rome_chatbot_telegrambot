namespace TelegramBot.Importer.Models.Announcement;

public record SystemNotification(int AnnouncementId, int UserId, int BotId, string Message, bool IsRealUser, bool IsJobUAT)
    : Notification(UserId, BotId, Message, IsRealUser, IsJobUAT)
{
    public override string ToXml()
        => $"<r Id=\"{AnnouncementId}\" UserId=\"{UserId}\" BotId=\"{BotId}\" Message=\"{Message}\" Status=\"{Status}\" />";
}