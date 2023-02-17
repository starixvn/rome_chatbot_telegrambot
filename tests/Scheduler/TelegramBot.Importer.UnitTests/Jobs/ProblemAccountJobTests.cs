using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Jobs;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.ProblemAccounts;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Jobs;

public class ProblemAccountJobTests
{
    private readonly IProblemAccountService problemAccountService;
    private readonly IMaintenanceService maintenanceService;
    private readonly ILoggerService loggerService;
    private readonly ISystemSettingService systemSettingService;
    private readonly ICustomerService customerService;
    private readonly ProblemAccountJob problemAccountJob;

    public ProblemAccountJobTests()
    {
        problemAccountService = Substitute.For<IProblemAccountService>();
        maintenanceService = Substitute.For<IMaintenanceService>();
        loggerService = Substitute.For<ILoggerService>();
        systemSettingService = Substitute.For<ISystemSettingService>();
        customerService = Substitute.For<ICustomerService>();
        problemAccountJob = new ProblemAccountJob(maintenanceService, loggerService, systemSettingService, problemAccountService, customerService);
    }

    [Theory, AutoData]
    public async Task Execute_Always_CallImport(ScheduleJob job, SystemSetting systemSetting, int maxLogId)
    {
        // Arrange
        IEnumerable<ProblemAccount> problemAccounts = new List<ProblemAccount>
        {
            new ProblemAccount(1,DateTime.Now,1,"Username",2,3,4)
        };
        IEnumerable<LinkedCustomer> linkedCustomers = new List<LinkedCustomer>
        {
            new LinkedCustomer(1,0,"en-US",2,1,true,10)
        };
        IEnumerable<CustomerStatus> customerStatus = new List<CustomerStatus>
        {
            new CustomerStatus(1,0,true,false,false,string.Empty)
        };
        customerService.GetCustomerStatus(Arg.Any<IEnumerable<CustomerStatusParam>>()).Returns(Task.FromResult(customerStatus));
        customerService.GetLinkedCustomers(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(linkedCustomers));
        problemAccountService.GetProblemAccounts(Arg.Any<int>(), Arg.Any<int>())
            .Returns(Task.FromResult(Tuple.Create(maxLogId, problemAccounts)));
        problemAccountService.Import(Arg.Any<IEnumerable<ProblemAccountNotification>>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult(new Result(1, "")));

        // Act
        await problemAccountJob.Execute(job, systemSetting);

        // Assert
        await problemAccountService.Received().Import(Arg.Any<IEnumerable<ProblemAccountNotification>>(), job.Environment, maxLogId);
    }
}