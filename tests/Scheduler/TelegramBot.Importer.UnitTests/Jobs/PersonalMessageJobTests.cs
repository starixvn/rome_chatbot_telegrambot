using Newtonsoft.Json;
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Jobs;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.PersonalMessages;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Jobs;

public class PersonalMessageJobTests
{
    private readonly IPersonalMessageService personalMessageService;
    private readonly IMaintenanceService maintenanceService;
    private readonly ILoggerService loggerService;
    private readonly ISystemSettingService systemSettingService;
    private readonly ICustomerService customerService;
    private readonly PersonalMessageJob personalMessageJob;

    public PersonalMessageJobTests()
    {
        personalMessageService = Substitute.For<IPersonalMessageService>();
        maintenanceService = Substitute.For<IMaintenanceService>();
        loggerService = Substitute.For<ILoggerService>();
        systemSettingService = Substitute.For<ISystemSettingService>();
        customerService = Substitute.For<ICustomerService>();
        personalMessageJob = new PersonalMessageJob(maintenanceService, loggerService, systemSettingService, customerService, personalMessageService);
    }

    [Theory, AutoData]
    public async Task Execute_HaveNOTNewMessages_CallImportWithEmptyList(ScheduleJob job, SystemSetting systemSetting)
    {
        // Arrange
        var personalMessages = Enumerable.Empty<PersonalMessage>();
        var notifications = Enumerable.Empty<PersonalMessageNotification>();
        personalMessageService.GetNewMessages(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>()).Returns(Task.FromResult(Tuple.Create(1, personalMessages)));
        var result = new Result(0, "");
        personalMessageService.Import(Arg.Any<IEnumerable<PersonalMessageNotification>>(), Arg.Any<string>(), Arg.Any<int>()).Returns(Task.FromResult(result));

        // Act
        await personalMessageJob.Execute(job, systemSetting);

        // Assert
        await personalMessageService.Received().Import(
            Arg.Is<IEnumerable<PersonalMessageNotification>>(l => JsonConvert.SerializeObject(l) == JsonConvert.SerializeObject(notifications)),
            job.Environment,
            Arg.Any<int>());
    }

    [Theory, AutoData]
    public async Task Execute_ImportFailed_CallLogError(ScheduleJob job, SystemSetting systemSetting)
    {
        // Arrange
        var personalMessages = Enumerable.Empty<PersonalMessage>();
        personalMessageService.GetNewMessages(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>()).Returns(Task.FromResult(Tuple.Create(1, personalMessages)));
        var result = new Result(1, "ErrMsg");
        personalMessageService.Import(Arg.Any<IEnumerable<PersonalMessageNotification>>(), Arg.Any<string>(), Arg.Any<int>()).Returns(Task.FromResult(result));

        // Act
        await personalMessageJob.Execute(job, systemSetting);

        // Assert
        await loggerService.Received().Error(job, result.Message);
    }

    [Theory, AutoData]
    public async Task Execute_HasPersonalMessages_CallLogError(ScheduleJob job, SystemSetting systemSetting)
    {
        // Arrange
        IEnumerable<PersonalMessage> personalMessages = new List<PersonalMessage>
        {
            new PersonalMessage(1,1,"contentEN@@","contentCS","contentCH","contentTH","contentVI",1,"123456,123321",0,null)
        };
        personalMessageService.GetNewMessages(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>()).Returns(Task.FromResult(Tuple.Create(1, personalMessages)));
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
        personalMessageService.Import(Arg.Any<IEnumerable<PersonalMessageNotification>>(), Arg.Any<string>(), Arg.Any<int>()).Returns(Task.FromResult(result));

        // Act
        await personalMessageJob.Execute(job, systemSetting);

        // Assert
        await customerService.Received().GetLinkedCustomers(job.Name, "1", job.Environment);
    }
}