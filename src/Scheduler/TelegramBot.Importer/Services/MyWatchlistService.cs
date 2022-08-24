using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.MyWatchlist;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.MyWatchlist;

namespace TelegramBot.Importer.Services;

public interface IMyWatchlistService
{
    Task<(int maxLogId, string lastDataRow, string serverName, IEnumerable<MonitoredPlayer> monitoredPlayers)> GetMonitoredPlayers(int lastLogId, int batchSize);

    Task<Result> Import(IEnumerable<MyWatchlistNotification> notifications, string environment, string lastDataRow, string serverName, int maxLogId);
}

public class MyWatchlistService : NotificationServiceBase, IMyWatchlistService
{
    public MyWatchlistService(IDynamicRepository dynamicRepository)
        : base(dynamicRepository)
    {
    }

    public async Task<(int maxLogId, string lastDataRow, string serverName, IEnumerable<MonitoredPlayer> monitoredPlayers)> GetMonitoredPlayers(int lastLogId, int batchSize)
    {
        var criteria = new GetMyWatchlistCriteria(lastLogId, batchSize);

        var monitoredPlayers = await dynamicRepository.FetchAsync<MonitoredPlayer>(criteria);

        return (criteria.MaxLogId, criteria.LastDataRow, criteria.ServerName, monitoredPlayers);
    }

    public Task<Result> Import(IEnumerable<MyWatchlistNotification> notifications, string environment, string lastDataRow, string serverName, int maxLogId)
        => ExecuteNonQuery(new ImportMyWatchlistCommand(
            BuildXmlData(notifications),
            BuildXmlDataLog(notifications),
            lastDataRow,
            serverName,
            maxLogId,
            environment));
}