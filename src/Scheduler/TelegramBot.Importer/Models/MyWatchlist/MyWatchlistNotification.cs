using TelegramBot.Importer.Models.ProblemAccounts;

namespace TelegramBot.Importer.Models.MyWatchlist;

public record MyWatchlistNotification(int Id, int UserId, int BotId, string Message, bool IsRealUser, bool IsJobUAT, IEnumerable<long> LogIds)
    : ProblemAccountNotification(Id, UserId, BotId, Message, IsRealUser, IsJobUAT, LogIds)
{
}