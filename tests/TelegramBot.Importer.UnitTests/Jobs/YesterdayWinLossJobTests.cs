using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Jobs;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Jobs;

public class YesterdayWinLossJobTests
{
    private readonly IYesterdayWinLossService yesterdayWinLossService;
    private readonly IMaintenanceService maintenanceService;
    private readonly ILoggerService loggerService;
    private readonly ISystemSettingService systemSettingService;
    private readonly ICustomerService customerService;
    private readonly YesterdayWinLossJob yesterdayWinLossJob;

    public YesterdayWinLossJobTests()
    {
        yesterdayWinLossService = Substitute.For<IYesterdayWinLossService>();
        maintenanceService = Substitute.For<IMaintenanceService>();
        loggerService = Substitute.For<ILoggerService>();
        systemSettingService = Substitute.For<ISystemSettingService>();
        customerService = Substitute.For<ICustomerService>();
        yesterdayWinLossJob = new YesterdayWinLossJob(maintenanceService, loggerService, systemSettingService, customerService, yesterdayWinLossService);
    }

    [Theory, AutoData]
    public async Task ImportYesterdayWinLossNotification_Always_CallsImport(ScheduleJob job, DateTime winLossDate, int maxUserId)
    {
        // Arrange
        customerService.GetYesterdayWinLossLinkedCustomers(Arg.Any<DateTime>(), Arg.Any<string>())
            .Returns(Task.FromResult(Tuple.Create(Enumerable.Empty<LinkedCustomer>(), maxUserId)));
        yesterdayWinLossService.Import(Arg.Any<IEnumerable<Notification>>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult(new Result(1, "")));

        // Act
        await yesterdayWinLossJob.ImportYesterdayWinLossNotification(job, winLossDate);

        // Assert
        await yesterdayWinLossService.Received().Import(Arg.Is(Enumerable.Empty<Notification>()), Arg.Is(winLossDate), Arg.Is(job.Environment), Arg.Is(maxUserId));
    }

    [Theory, AutoData]
    public async Task ImportYesterdayWinLossNotification_HasException_CallsLogError(ScheduleJob job, DateTime winLossDate, int maxUserId)
    {
        // Arrange
        customerService.GetYesterdayWinLossLinkedCustomers(Arg.Any<DateTime>(), Arg.Any<string>())
            .Returns(Task.FromResult(Tuple.Create(Enumerable.Empty<LinkedCustomer>(), maxUserId)));
        var ex = new Exception();
        yesterdayWinLossService.Import(Arg.Any<IEnumerable<Notification>>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromException<Result>(ex));

        // Act
        await yesterdayWinLossJob.ImportYesterdayWinLossNotification(job, winLossDate);

        // Assert
        await loggerService.Received().Error(ex, job);
    }
}