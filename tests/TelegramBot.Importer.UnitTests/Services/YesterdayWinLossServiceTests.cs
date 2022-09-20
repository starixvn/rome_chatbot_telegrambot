using Alpha.API.Client.Services;
using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.YesterdayWinLoss;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Services;

public class YesterdayWinLossServiceTests
{
    private readonly IWinLossService winLossService;
    private readonly IDynamicRepository dynamicRepository;
    private readonly YesterdayWinLossService yesterdayWinLossService;

    public YesterdayWinLossServiceTests()
    {
        winLossService = Substitute.For<IWinLossService>();
        dynamicRepository = Substitute.For<IDynamicRepository>();
        yesterdayWinLossService = new YesterdayWinLossService(winLossService, dynamicRepository);
    }

    [Theory, AutoData]
    public async Task GetWinLoss_Always_CallsGetWinLoss(int custId, byte custLevel, DateTime winLossDate)
    {
        // Act
        await yesterdayWinLossService.GetWinLoss(custId, custLevel, winLossDate);

        // Assert
        await winLossService.Received().GetWinLoss(custId, custLevel, winLossDate);
    }

    [Theory, AutoData]
    public async Task Import_Always_CallsExecuteAsync(IEnumerable<Notification> notifications, DateTime winLossDate, string environment, int maxUserId)
    {
        // Act
        await yesterdayWinLossService.Import(notifications, winLossDate, environment, maxUserId);

        // Assert
        await dynamicRepository.Received().ExecuteAsync(Arg.Is<ImportYesterdayWinLossCommand>(c => c.Environment == environment && c.WinLostDate == winLossDate && c.MaxUserId == maxUserId));
    }
}