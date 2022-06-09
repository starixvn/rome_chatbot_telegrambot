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

public class NoExceptionBackgroundServiceTests
{
    private readonly ScheduleJob scheduleJob;
    private readonly ILoggerService loggerService;
    private readonly IMaintenanceService maintenanceService;
    private readonly IGlobalSettingService globalSettingService;
    private readonly NoExceptionBackgroundService backgroundService;

    public NoExceptionBackgroundServiceTests()
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
        backgroundService = new NoExceptionBackgroundService(
            scheduleJob,
            loggerService,
            maintenanceService,
            new UMClientConfiguration { ResultCacheDuration = 60 },
            globalSettingService
        );
    }
    
    [Fact]
    public async Task ExecuteAsync_IsNotActive_DoesNotCallsLogInfo()
    {
        // Arrange
        var jobDetailInactive = new ScheduleJob();
        var backgroundServiceInactive = new NoExceptionBackgroundService(
            jobDetailInactive,
            loggerService,
            maintenanceService,
            new UMClientConfiguration { ResultCacheDuration = 60 },
            globalSettingService
        );

        // Act
        await backgroundServiceInactive.StartAsync(default);

        // Assert
        await loggerService.DidNotReceive().Info(Arg.Any<string>());
    }

    // [Fact]
    // public async Task ExecuteAsync_IsActive_CallsGetUnderMaintenanceTimeAsync()
    // {
    //     // Arrange
    //     loggerService
    //         .Info(Arg.Any<string>())
    //         .Returns(Task.CompletedTask);

    //     // Act
    //     await backgroundService.StartAsync(default);

    //     // Assert
    //     await maintenanceService.Received().GetUnderMaintenanceTimeAsync();
    // }

    // [Fact]
    // public async Task ExecuteAsync_ExecuteJob_NotCallsErrorLog()
    // {
    //     // Arrange
    //     loggerService
    //         .Info(Arg.Any<string>())
    //         .Returns(Task.CompletedTask);
    //     maintenanceService.GetUnderMaintenanceTimeAsync()
    //         .Returns(Task.FromResult(new UnderMaintenanceTime(false, string.Empty, string.Empty)));

    //     // Act
    //     await backgroundService.StartAsync(default);

    //     // Assert
    //     await loggerService.DidNotReceive().Error(
    //         Arg.Any<Exception>(),
    //         Arg.Any<ScheduleJob>(),
    //         Arg.Any<string>(),
    //         Arg.Any<string>()
    //     );
    // }

    [Fact]
    public void CalculateMultiplier_DelayExecutionTimeInSeconds_ReturnsDelayExecutionTimeInSeconds()
    {
        // Arrange
        var scheduleJobTest = new ScheduleJob
        {
            Name = "AccumulateWinLossJob",
            BatchSize = 1000,
            DelayExecutionTimeInSeconds = 10,
            IsActive = true
        };
        var backgroundServiceTest = new NoExceptionBackgroundService(
            scheduleJobTest,
            loggerService,
            maintenanceService,
            new UMClientConfiguration { ResultCacheDuration = 60 },
            globalSettingService
        );

        // Act
        var actual = backgroundServiceTest.CalculateMultiplier(true, 1);

        // Assert
        Assert.Equal(scheduleJobTest.DelayExecutionTimeInSeconds, actual);
    }

    [Fact]
    public void CalculateMultiplier_HasData_ReturnsOneSecond()
    {
        // Arrange & Act
        var actual = backgroundService.CalculateMultiplier(true, 10);

        // Assert
        Assert.Equal(1, actual);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 4)]
    [InlineData(4, 8)]
    [InlineData(8, 10)]
    [InlineData(10, 10)]
    public void CalculateMultiplier_NoData_ReturnsOneSecond(int multiplier, int expected)
    {
        // Arrange & Act
        var actual = backgroundService.CalculateMultiplier(false, multiplier);

        // Assert
        Assert.Equal(expected, actual);
    }
}