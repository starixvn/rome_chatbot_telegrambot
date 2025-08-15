/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NSubstitute;
using ONEbook.UM.Configurations;
using ONEbook.UM.Services;
using WLMonitored.AccumWLService.Jobs;
using WLMonitored.AccumWLService.Models;
using WLMonitored.AccumWLService.Services;
using WLMonitored.Shared.Configuration;
using WLMonitored.Shared.Models;
using WLMonitored.Shared.Services;
using Xunit;

namespace WLMonitored.AccumWLService.UnitTests.Jobs;

public class AccumulateWinLossJobTests
{
    private readonly ScheduleJob jobDetail;
    private readonly ILoggerService loggerService;
    private readonly IMaintenanceService maintenanceService;
    private readonly IGlobalSettingService globalSettingService;
    private readonly ITicketDetailService ticketDetailService;
    private readonly IWinLossService winLossService;
    private readonly IBetTypeService betTypeService;
    private readonly AccumulateWinLossJob accumulateWinLossJob;

    public AccumulateWinLossJobTests()
    {
        jobDetail = new ScheduleJob
        {
            Name = "AccumulateWinLossJob",
            BatchSize = 1000,
            IsActive = true
        };
        IOptions<ScheduleJob> options = Options.Create(jobDetail);
        loggerService = Substitute.For<ILoggerService>();
        maintenanceService = Substitute.For<IMaintenanceService>();
        var umClientConfiguration = new UMClientConfiguration { ResultCacheDuration = 60 };
        globalSettingService = Substitute.For<IGlobalSettingService>();
        ticketDetailService = Substitute.For<ITicketDetailService>();
        winLossService = Substitute.For<IWinLossService>();
        betTypeService = Substitute.For<IBetTypeService>();

        accumulateWinLossJob = new AccumulateWinLossJob(
            options,
            loggerService,
            maintenanceService,
            umClientConfiguration,
            globalSettingService,
            ticketDetailService,
            winLossService,
            betTypeService
        );
    }

    [Fact]
    public async Task ExecuteJob_NoTicketDetails_ReturnsFalse()
    {
        // Arrange
        long lastId = 1;
        var dict = new Dictionary<string, string>
        {
            ["LastId"] = lastId.ToString()
        };
        var globalSetting = new GlobalSetting(dict, string.Empty);
        ticketDetailService
            .GetTicketDetails(lastId, jobDetail.BatchSize)
            .Returns(Task.FromResult(Enumerable.Empty<TicketDetail>()));

        // Act
        var actual = await accumulateWinLossJob.ExecuteJob(globalSetting);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task ExecuteJob_NoWinLossSettings_ReturnsFalse()
    {
        // Arrange
        long lastId = 1;
        var dict = new Dictionary<string, string>
        {
            ["LastId"] = lastId.ToString()
        };
        var globalSetting = new GlobalSetting(dict, string.Empty);
        var ticketDetails = new List<TicketDetail>
         {
             new TicketDetail(1, 9899011289, 2211693, 1, 970, 1970, DateTime.Parse("2022-06-20 23:20:26.073")),
         };
        ticketDetailService
            .GetTicketDetails(lastId, jobDetail.BatchSize)
            .Returns(Task.FromResult<IEnumerable<TicketDetail>>(ticketDetails));
        winLossService
            .GetWinLossSettings(Arg.Any<string>())
            .Returns(Task.FromResult(Enumerable.Empty<WinLossSetting>()));

        // Act
        var actual = await accumulateWinLossJob.ExecuteJob(globalSetting);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task ExecuteJob_NoAccumulatedWinLoss_ReturnsActionIsInsert()
    {
        // Arrange
        long lastId = 1;
        var dict = new Dictionary<string, string>
        {
            ["LastId"] = lastId.ToString()
        };
        var globalSetting = new GlobalSetting(dict, string.Empty);
        var ticketDetails = new List<TicketDetail>
         {
             new TicketDetail(85, 9899011658, 2211693, 1, 1000, 0, DateTime.Parse("2022-06-20 23:22:06.923")),
             new TicketDetail(1, 9899011289, 2211693, 2, 970, 1970, DateTime.Parse("2022-06-20 23:20:26.073")),
             new TicketDetail(37, 9899005395, 2211693, 3, 970, 485, DateTime.Parse("2022-06-20 23:20:26.073")),
             new TicketDetail(38, 9899005394, 2211693, 4, 970, 1470, DateTime.Parse("2022-06-20 23:20:26.073")),
         };
        ticketDetailService
            .GetTicketDetails(lastId, jobDetail.BatchSize)
            .Returns(Task.FromResult<IEnumerable<TicketDetail>>(ticketDetails));
        var winLossSettings = new List<WinLossSetting>
         {
             new WinLossSetting(2211693, DateTime.Parse("2022-06-20 23:20:00.000"), false)
         };
        winLossService
            .GetWinLossSettings(Arg.Any<string>())
            .Returns(Task.FromResult<IEnumerable<WinLossSetting>>(winLossSettings));
        winLossService
            .GetAccumulatedWinLoss(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>())
            .Returns(Enumerable.Empty<AccumulatedWinLoss>());
        var expected = new AccumulatedWinLossParam(2211693, false, 15, DateTime.Parse("2022-06-20"), "i");

        // Act
        await accumulateWinLossJob.ExecuteJob(globalSetting);

        // Assert
        await winLossService.Received().UpdateAccumulatedWinLoss(
            Arg.Is<IEnumerable<AccumulatedWinLossParam>>(accumulatedWinLossParams =>
                expected == accumulatedWinLossParams.First()),
            Arg.Is<long>(lastId => 85 == lastId));
    }

    [Fact]
    public async Task ExecuteJob_InitSettingTimeIsGreaterThanUpdatedTime_ReturnsActionIsInsert()
    {
        // Arrange
        long lastId = 1;
        var dict = new Dictionary<string, string>
        {
            ["LastId"] = lastId.ToString()
        };
        var globalSetting = new GlobalSetting(dict, string.Empty);
        var ticketDetails = new List<TicketDetail>
         {
             new TicketDetail(85, 9899011658, 2211693, 1, 1000, 0, DateTime.Parse("2022-06-20 23:22:06.923")),
             new TicketDetail(1, 9899011289, 2211693, 2, 970, 1970, DateTime.Parse("2022-06-20 23:20:26.073")),
             new TicketDetail(37, 9899005395, 2211693, 3, 970, 485, DateTime.Parse("2022-06-20 23:20:26.073")),
             new TicketDetail(38, 9899005394, 2211693, 4, 970, 1470, DateTime.Parse("2022-06-20 23:20:26.073")),
         };
        ticketDetailService
            .GetTicketDetails(lastId, jobDetail.BatchSize)
            .Returns(Task.FromResult<IEnumerable<TicketDetail>>(ticketDetails));
        var winLossSettings = new List<WinLossSetting>
         {
             new WinLossSetting(2211693, DateTime.Parse("2022-06-20 23:21:00.000"), false)
         };
        winLossService
            .GetWinLossSettings(Arg.Any<string>())
            .Returns(Task.FromResult<IEnumerable<WinLossSetting>>(winLossSettings));
        var accumulatedWinLossList = new List<AccumulatedWinLoss>
         {
             new AccumulatedWinLoss
             {
                 CustId = 2211693,
                 AccumWL = 0,
                 UpdatedTime = DateTime.Parse("2022-06-20 23:20:00.000")
             }
         };
        winLossService
            .GetAccumulatedWinLoss(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>())
            .Returns(Task.FromResult<IEnumerable<AccumulatedWinLoss>>(accumulatedWinLossList));
        var expected = new AccumulatedWinLossParam(2211693, false, -1000, DateTime.Parse("2022-06-20"), "i");

        // Act
        await accumulateWinLossJob.ExecuteJob(globalSetting);

        // Assert
        await winLossService.Received().UpdateAccumulatedWinLoss(
            Arg.Is<IEnumerable<AccumulatedWinLossParam>>(accumulatedWinLossParams =>
                expected == accumulatedWinLossParams.First()),
            Arg.Is<long>(lastId => 85 == lastId));
    }

    [Fact]
    public async Task ExecuteJob_InitSettingTimeIsLessThanUpdatedTime_ReturnsActionIsInsert()
    {
        // Arrange
        long lastId = 1;
        var dict = new Dictionary<string, string>
        {
            ["LastId"] = lastId.ToString()
        };
        var globalSetting = new GlobalSetting(dict, string.Empty);
        var ticketDetails = new List<TicketDetail>
         {
             new TicketDetail(85, 9899011658, 2211693, 1, 1000, 0, DateTime.Parse("2022-06-20 23:22:06.923")),
             new TicketDetail(1, 9899011289, 2211693, 2, 970, 1970, DateTime.Parse("2022-06-20 23:20:26.073")),
             new TicketDetail(37, 9899005395, 2211693, 3, 970, 485, DateTime.Parse("2022-06-20 23:20:26.073")),
             new TicketDetail(38, 9899005394, 2211693, 4, 970, 1470, DateTime.Parse("2022-06-20 23:20:26.073")),
         };
        ticketDetailService
            .GetTicketDetails(lastId, jobDetail.BatchSize)
            .Returns(Task.FromResult<IEnumerable<TicketDetail>>(ticketDetails));
        var winLossSettings = new List<WinLossSetting>
         {
             new WinLossSetting(2211693, DateTime.Parse("2022-06-20 23:21:00.000"), false)
         };
        winLossService
            .GetWinLossSettings(Arg.Any<string>())
            .Returns(Task.FromResult<IEnumerable<WinLossSetting>>(winLossSettings));
        var accumulatedWinLossList = new List<AccumulatedWinLoss>
         {
             new AccumulatedWinLoss
             {
                 CustId = 2211693,
                 AccumWL = 0,
                 UpdatedTime = DateTime.Parse("2022-06-20 23:25:00.000")
             }
         };
        winLossService
            .GetAccumulatedWinLoss(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>())
            .Returns(Task.FromResult<IEnumerable<AccumulatedWinLoss>>(accumulatedWinLossList));
        var expected = new AccumulatedWinLossParam(2211693, false, 0, DateTime.Parse("2022-06-20"), string.Empty);

        // Act
        await accumulateWinLossJob.ExecuteJob(globalSetting);

        // Assert
        await winLossService.Received().UpdateAccumulatedWinLoss(
            Arg.Is<IEnumerable<AccumulatedWinLossParam>>(accumulatedWinLossParams =>
                expected == accumulatedWinLossParams.First()),
            Arg.Is<long>(lastId => 85 == lastId));
    }
}*/