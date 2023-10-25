using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Jobs;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.Customers;
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
    public async Task Execute_Always_CallImport(ScheduleJob job, SystemSetting systemSetting, long maxLogId)
    {
        // Arrange
        IEnumerable<MemberWinLossLimit> memberWinLossLimits = new List<MemberWinLossLimit>{
            new MemberWinLossLimit(1,"username",1,1,1,1,2,3,1,"sUsername","mUsername","aUsername",1)
        };
        memberWinLostLimitService.GetMemberWinLossLimits(Arg.Any<int>(), Arg.Any<long>())
            .Returns(Task.FromResult(Tuple.Create(maxLogId, memberWinLossLimits)));
        IEnumerable<LinkedCustomer> linkedCustomers = new List<LinkedCustomer>
        {
            new LinkedCustomer
            {
                CustId = 1,
                SubAccId = 0,
                Language = "en-US",
                CustomerLevel = 2,
                UserId = 1,
                IsRealUser = true,
                BotId = 10
            }
        };
        IEnumerable<CustomerStatus> customerStatus = new List<CustomerStatus>
        {
            new CustomerStatus(1,0,true,false,false,string.Empty)
        };
        customerService.GetCustomerStatus(Arg.Any<IEnumerable<CustomerStatusParam>>()).Returns(Task.FromResult(customerStatus));
        customerService.GetLinkedCustomers(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(linkedCustomers));
        memberWinLostLimitService.Import(Arg.Any<IEnumerable<MemberWinLossLimitNotification>>(), Arg.Any<string>(), Arg.Any<long>())
            .Returns(Task.FromResult(new Result(1, "")));

        // Act
        await memberWinLossLimitJob.Execute(job, systemSetting);

        // Assert
        await memberWinLostLimitService.Received().Import(Arg.Any<IEnumerable<MemberWinLossLimitNotification>>(), job.Environment, maxLogId);
    }
}