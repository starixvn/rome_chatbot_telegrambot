using Fanex.Data.Repository;
using Newtonsoft.Json;
using TelegramBot.Importer.Criteria.MyWatchlist;
using TelegramBot.Importer.Models.MyWatchlist;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Services;

public class MyWatchlistServiceTests
{
    private readonly IDynamicRepository dynamicRepository;
    private readonly MyWatchlistService myWatchlistService;

    public MyWatchlistServiceTests()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        myWatchlistService = new MyWatchlistService(dynamicRepository);
    }

    [Theory, AutoData]
    public async Task GetMonitoredPlayers_Always_ReturnsCorrectData(int lastLogId, int batchSize, string lastDataRow, string serverName, IEnumerable<MonitoredPlayer> monitoredPlayers)
    {
        // Arrange
        var criteria = new GetMyWatchlistCriteria(lastLogId, batchSize, lastDataRow, serverName);
        dynamicRepository.FetchAsync<MonitoredPlayer>(Arg.Any<GetMyWatchlistCriteria>()).Returns(Task.FromResult(monitoredPlayers));

        // Act
        var result = await myWatchlistService.GetMonitoredPlayers(lastLogId, batchSize, lastDataRow, serverName);

        // Assert
        Assert.True(result.maxLogId == criteria.MaxLogId
            && result.lastDataRow == criteria.LastDataRow
            && result.serverName == criteria.ServerName
            && JsonConvert.SerializeObject(result.monitoredPlayers) == JsonConvert.SerializeObject(monitoredPlayers));
    }

    [Theory, AutoData]
    public async Task Import_Always_CallsExecuteAsync(IEnumerable<MyWatchlistNotification> notifications, string environment, string lastDataRow, string serverName, int maxLogId)
    {
        // Act
        await myWatchlistService.Import(notifications, environment, lastDataRow, serverName, maxLogId);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Is<ImportMyWatchlistCommand>(c =>
            c.Environment == environment
            && c.LastDataRow == lastDataRow
            && c.ServerName == serverName
            && c.MaxLogId == maxLogId));
    }
}