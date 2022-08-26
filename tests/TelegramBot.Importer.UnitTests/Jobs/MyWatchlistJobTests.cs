using Newtonsoft.Json;
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
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
            new LinkedCustomer(1,0,"en-US",2,3,true,10)
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
        await customerService.Received().GetLinkedCustomers(job.Name, "1", job.Environment);
    }
}