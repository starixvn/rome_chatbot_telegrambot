using Newtonsoft.Json;
using ONEbook.UM.Models;
using ONEbook.UM.Services;
using Quartz;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Jobs;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.MyWatchlist;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Jobs;

public class MyWatchlistJobTests
{
    private readonly IMyWatchlistService myWatchlistService;
    private readonly IMaintenanceService maintenanceService;
    private readonly ILoggerService loggerService;
    private readonly ISystemSettingService systemSettingService;
    private readonly ICustomerService customerService;
    private readonly MyWatchlistJob myWatchlistJob;

    public MyWatchlistJobTests()
    {
        myWatchlistService = Substitute.For<IMyWatchlistService>();
        maintenanceService = Substitute.For<IMaintenanceService>();
        loggerService = Substitute.For<ILoggerService>();
        systemSettingService = Substitute.For<ISystemSettingService>();
        customerService = Substitute.For<ICustomerService>();
        myWatchlistJob = new MyWatchlistJob(myWatchlistService, maintenanceService, loggerService, systemSettingService, customerService);
    }

    [Theory, AutoData]
    public async Task Execute_HaveNOTMonitoredPlayers_CallImportWithEmptyList(ScheduleJob job, SystemSetting systemSetting, int maxLogId, string lastDataRow, string serverName)
    {
        // Arrange
        var notifications = new List<MyWatchlistNotification>();
        var monitoredPlayers = Enumerable.Empty<MonitoredPlayer>();
        myWatchlistService.GetMonitoredPlayers(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult((maxLogId, lastDataRow, serverName, monitoredPlayers)));

        var result = new Result(0, "");
        myWatchlistService.Import(Arg.Any<IEnumerable<MyWatchlistNotification>>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult(result));

        // Act
        await myWatchlistJob.Execute(job, systemSetting);

        // Assert
        await myWatchlistService.Received().Import(
            Arg.Is<IEnumerable<MyWatchlistNotification>>(l => JsonConvert.SerializeObject(l) == JsonConvert.SerializeObject(notifications)),
            job.Environment,
            lastDataRow,
            serverName,
            maxLogId);
    }

    [Theory, AutoData]
    public async Task Execute_ImportFailed_CallLogError(ScheduleJob job, SystemSetting systemSetting, int maxLogId, string lastDataRow, string serverName)
    {
        // Arrange
        var monitoredPlayers = Enumerable.Empty<MonitoredPlayer>();
        myWatchlistService.GetMonitoredPlayers(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult((maxLogId, lastDataRow, serverName, monitoredPlayers)));

        var result = new Result(1, "ErrMsg");
        myWatchlistService.Import(Arg.Any<IEnumerable<MyWatchlistNotification>>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult(result));

        // Act
        await myWatchlistJob.Execute(job, systemSetting);

        // Assert
        await loggerService.Received().Error(job, result.Message);
    }

    [Theory, AutoData]
    public async Task Execute_HasMonitoredPlayers_CallLogError(ScheduleJob job, SystemSetting systemSetting, int maxLogId, string lastDataRow, string serverName)
    {
        // Arrange
        IEnumerable<MonitoredPlayer> monitoredPlayers = new List<MonitoredPlayer>
        {
            new MonitoredPlayer{LogId=1, CustId=1,Username="Username",AccumWL=1000,WinningThreshold=1000,LosingThreshold=1000,SettingType=1,CreatedDate=DateTime.Now,Recommend=1,MRecommend=2,SRecommend=3}
        };
        myWatchlistService.GetMonitoredPlayers(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
           .Returns(Task.FromResult((maxLogId, lastDataRow, serverName, monitoredPlayers)));
        IEnumerable<LinkedCustomer> linkedCustomers = new List<LinkedCustomer>
        {
            new LinkedCustomer
            {
                CustId = 1,
                SubAccId = 0,
                Language = "en-US",
                CustomerLevel = 2,
                UserId = 3,
                IsRealUser = true,
                BotId = 10
            }
        };
        customerService.GetLinkedCustomers(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(linkedCustomers));
        IEnumerable<CustomerStatus> customerStatus = new List<CustomerStatus>
        {
            new CustomerStatus(1,0,true,false,false,"")
        };
        customerService.GetCustomerStatus(Arg.Any<IEnumerable<CustomerStatusParam>>()).Returns(Task.FromResult(customerStatus));
        var result = new Result(0, "");
        myWatchlistService.Import(Arg.Any<IEnumerable<MyWatchlistNotification>>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult(result));

        // Act
        await myWatchlistJob.Execute(job, systemSetting);

        // Assert
        await customerService.Received().GetLinkedCustomers(FunctionKey.RiskAlert, "1,2,3", job.Environment);
    }

    [Fact]
    public async Task Execute_IsUM_CallsLogInfoConsole()
    {
        // Arrange
        var context = Substitute.For<IJobExecutionContext>();
        var jobName = "MyWatchlist";
        var environment = "UAT";
        var subAccPermission = "[A]";
        IDictionary<string, object> keyValuePairs = new Dictionary<string, object> {
            { "JobName", jobName },
            { "Environment", environment },
            { "SubAccPermission", subAccPermission },
            { "IsGetTestData", "false" }
        };
        context.JobDetail.JobDataMap.Returns(new JobDataMap(keyValuePairs));
        var underMaintenanceTime = new UnderMaintenanceTime(true, "3:00", "4:00");
        maintenanceService.GetUnderMaintenanceTimeAsync().Returns(Task.FromResult(underMaintenanceTime));

        // Act
        await myWatchlistJob.Execute(context);

        // Assert
        loggerService.Received().InfoConsole(
            Arg.Is<ScheduleJob>(j => j.Name == jobName && j.SubAccPermission == subAccPermission && j.Environment == environment && !j.IsGetTestData && j.IsActive),
            "is Under Maintenance.");
    }

    [Fact]
    public async Task Execute_SettingIsNull_CallsLogInfoConsole()
    {
        // Arrange
        var context = Substitute.For<IJobExecutionContext>();
        var jobName = "MyWatchlist";
        var environment = "UAT";
        var subAccPermission = "[A]";
        IDictionary<string, object> keyValuePairs = new Dictionary<string, object> {
            { "JobName", jobName },
            { "Environment", environment },
            { "SubAccPermission", subAccPermission },
            { "IsGetTestData", "false" }
        };
        context.JobDetail.JobDataMap.Returns(new JobDataMap(keyValuePairs));
        var underMaintenanceTime = new UnderMaintenanceTime(false, "3:00", "4:00");
        maintenanceService.GetUnderMaintenanceTimeAsync().Returns(Task.FromResult(underMaintenanceTime));
        systemSettingService.GetSystemSetting(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult<SystemSetting>(null));

        // Act
        await myWatchlistJob.Execute(context);

        // Assert
        loggerService.Received().InfoConsole(
            Arg.Is<ScheduleJob>(j => j.Name == jobName && j.SubAccPermission == subAccPermission && j.Environment == environment && !j.IsGetTestData && j.IsActive),
            "is set stopped from DB.");
    }

    [Fact]
    public async Task Execute_ServiceFlagIsFalse_CallsLogInfoConsole()
    {
        // Arrange
        var context = Substitute.For<IJobExecutionContext>();
        var jobName = "MyWatchlist";
        var environment = "UAT";
        var subAccPermission = "[A]";
        IDictionary<string, object> keyValuePairs = new Dictionary<string, object> {
            { "JobName", jobName },
            { "Environment", environment },
            { "SubAccPermission", subAccPermission },
            { "IsGetTestData", "false" }
        };
        context.JobDetail.JobDataMap.Returns(new JobDataMap(keyValuePairs));
        var underMaintenanceTime = new UnderMaintenanceTime(false, "3:00", "4:00");
        maintenanceService.GetUnderMaintenanceTimeAsync().Returns(Task.FromResult(underMaintenanceTime));
        systemSettingService.GetSystemSetting(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(new SystemSetting(new Dictionary<string, string> { ["ServiceFlag"] = "0" }, string.Empty)));

        // Act
        await myWatchlistJob.Execute(context);

        // Assert
        loggerService.Received().InfoConsole(
            Arg.Is<ScheduleJob>(j => j.Name == jobName && j.SubAccPermission == subAccPermission && j.Environment == environment && !j.IsGetTestData && j.IsActive),
            "is set stopped from DB.");
    }

    [Fact]
    public async Task Execute_ServiceFlagIsTrue_CallsLogInfoConsole()
    {
        // Arrange
        var context = Substitute.For<IJobExecutionContext>();
        var jobName = "MyWatchlist";
        var environment = "UAT";
        var subAccPermission = "[A]";
        IDictionary<string, object> keyValuePairs = new Dictionary<string, object> {
            { "JobName", jobName },
            { "Environment", environment },
            { "SubAccPermission", subAccPermission },
            { "IsGetTestData", "false" }
        };
        context.JobDetail.JobDataMap.Returns(new JobDataMap(keyValuePairs));
        var underMaintenanceTime = new UnderMaintenanceTime(false, "3:00", "4:00");
        maintenanceService.GetUnderMaintenanceTimeAsync().Returns(Task.FromResult(underMaintenanceTime));
        systemSettingService.GetSystemSetting(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(new SystemSetting(new Dictionary<string, string> { ["ServiceFlag"] = "1" }, string.Empty)));

        // Act
        await myWatchlistJob.Execute(context);

        // Assert
        await systemSettingService.Received().UpdateJobStatusAndLastRunTime(jobName, environment, "Running", Arg.Any<DateTime>());
    }
}