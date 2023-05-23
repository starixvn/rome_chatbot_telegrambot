using System;
using System.Threading.Tasks;
using NSubstitute;
using ONEbook.UM.Configurations;
using ONEbook.UM.Models;
using ONEbook.UM.Services;
using WLMonitored.Shared.Configuration;
using WLMonitored.Shared.Services;
using Xunit;

namespace WLMonitored.Shared.UnitTests.Jobs;

public class ExceptionBackgroundServiceTests
{
    private readonly ScheduleJob scheduleJob;
    private readonly ILoggerService loggerService;
    private readonly IMaintenanceService maintenanceService;
    private readonly IGlobalSettingService globalSettingService;
    private readonly ExceptionBackgroundService backgroundService;

    public ExceptionBackgroundServiceTests()
    {
        scheduleJob = new ScheduleJob
        {
            Name = "AccumulateWinLossJob",
            BatchSize = 1000,
            IsActive = true
        };
        loggerService = Substitute.For<ILoggerService>();
        maintenanceService = Substitute.For<IMaintenanceService>();
        globalSettingService = Substitute.For<IGlobalSettingService>();
        backgroundService = new ExceptionBackgroundService(
            scheduleJob,
            loggerService,
            maintenanceService,
            new UMClientConfiguration { ResultCacheDuration = 60 },
            globalSettingService
        );
    }

    [Fact]
    public async Task ExecuteAsync_ExecuteJob_WriteErrorLog()
    {
        // Arrange
        loggerService
            .Info(Arg.Any<string>())
            .Returns(Task.CompletedTask);
        maintenanceService.GetUnderMaintenanceTimeAsync()
            .Returns(Task.FromResult(new UnderMaintenanceTime(false, string.Empty, string.Empty)));

        // Act
        await backgroundService.StartAsync(default);

        // Assert
        await loggerService.Received().Error(
            Arg.Any<Exception>(),
            Arg.Any<ScheduleJob>(),
            Arg.Any<string>(),
            Arg.Any<string>()
        );
    }
}