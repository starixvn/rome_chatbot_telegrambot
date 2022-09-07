using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Jobs;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.MemberWinLossLimits;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Jobs;

public class MemberWinLossLimitJobTests
{
    private readonly IMemberWinLossLimitService memberWinLostLimitService;
    private readonly ICacheRefService cacheRefService;
    private readonly IMaintenanceService maintenanceService;
    private readonly ILoggerService loggerService;
    private readonly ISystemSettingService systemSettingService;
    private readonly ICustomerService customerService;
    private readonly MemberWinLossLimitJob memberWinLossLimitJob;

    public MemberWinLossLimitJobTests()
    {
        maintenanceService = Substitute.For<IMaintenanceService>();
        loggerService = Substitute.For<ILoggerService>();
        systemSettingService = Substitute.For<ISystemSettingService>();
        customerService = Substitute.For<ICustomerService>();
        memberWinLostLimitService = Substitute.For<IMemberWinLossLimitService>();
        cacheRefService = Substitute.For<ICacheRefService>();
        memberWinLossLimitJob = new MemberWinLossLimitJob(maintenanceService, loggerService, systemSettingService, customerService, memberWinLostLimitService, cacheRefService);
    }

    [Theory, AutoData]
    public async Task Execute_Always_CallImport(ScheduleJob job, SystemSetting systemSetting, long maxLogId, IEnumerable<MemberWinLossLimit> memberWinLossLimits)
    {
        // Arrange
        memberWinLostLimitService.GetMemberWinLossLimits(Arg.Any<int>(), Arg.Any<long>())
            .Returns(Task.FromResult(Tuple.Create(maxLogId, memberWinLossLimits)));
        memberWinLostLimitService.Import(Arg.Any<IEnumerable<MemberWinLossLimitNotification>>(), Arg.Any<string>(), Arg.Any<long>())
            .Returns(Task.FromResult(new Result(1, "")));

        // Act
        await memberWinLossLimitJob.Execute(job, systemSetting);

        // Assert
        await memberWinLostLimitService.Received().Import(Arg.Any<IEnumerable<MemberWinLossLimitNotification>>(), job.Environment, maxLogId);
    }
}