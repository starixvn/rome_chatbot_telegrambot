namespace TelegramBot.Importer.Models.Announcement;

public record SystemNotification(int UserId, int BotId, string Message, bool IsRealUser, bool IsJobUAT)
    : Notification(UserId, BotId, Message, IsRealUser, IsJobUAT)
{
}