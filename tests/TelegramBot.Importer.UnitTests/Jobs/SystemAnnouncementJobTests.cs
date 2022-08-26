using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Jobs;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.Announcement;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.UnitTests.Jobs;

public class SystemAnnouncementJobTests
{
    private readonly IConfiguration configuration;
    private readonly IAnnouncementService announcementService;
    private readonly IProductService productService;
    private readonly IMaintenanceService maintenanceService;
    protected readonly ILoggerService loggerService;
    protected readonly ISystemSettingService systemSettingService;
    protected readonly ICustomerService customerService;

    private readonly SystemAnnouncementJob systemAnnouncementJob;

    public SystemAnnouncementJobTests()
    {
        configuration = Substitute.For<IConfiguration>();
        announcementService = Substitute.For<IAnnouncementService>();
        productService = Substitute.For<IProductService>();
        maintenanceService = Substitute.For<IMaintenanceService>();
        loggerService = Substitute.For<ILoggerService>();
        systemSettingService = Substitute.For<ISystemSettingService>();
        customerService = Substitute.For<ICustomerService>();

        systemAnnouncementJob = new SystemAnnouncementJob(
            configuration,
            announcementService,
            productService,
            maintenanceService,
            loggerService,
            systemSettingService,
            customerService);
    }

    [Theory, AutoData]
    public async Task Execute_HaveNOTAnnouncement_CallImportWithEmptyList(ScheduleJob job, SystemSetting systemSetting)
    {
        // Arrange
        var announcements = Enumerable.Empty<Announcement>();
        var notifications = new List<SystemNotification>();
        announcementService.GetAnnouncements(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<string>()).Returns(Task.FromResult(announcements));
        var result = new Result(0, "");
        announcementService.Import(Arg.Any<List<SystemNotification>>(), Arg.Any<string>(), Arg.Any<DateTime>()).Returns(Task.FromResult(result));

        // Act
        await systemAnnouncementJob.Execute(job, systemSetting);

        // Assert
        await announcementService.Received().Import(
            Arg.Is<List<SystemNotification>>(l => JsonConvert.SerializeObject(l) == JsonConvert.SerializeObject(notifications)),
            job.Environment,
            Arg.Any<DateTime>());
    }

    [Theory, AutoData]
    public async Task Execute_ImportFailed_CallLogError(ScheduleJob job, SystemSetting systemSetting)
    {
        // Arrange
        var announcements = Enumerable.Empty<Announcement>();
        announcementService.GetAnnouncements(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<string>()).Returns(Task.FromResult(announcements));
        var result = new Result(1, "ErrMsg");
        announcementService.Import(Arg.Any<List<SystemNotification>>(), Arg.Any<string>(), Arg.Any<DateTime>()).Returns(Task.FromResult(result));

        // Act
        await systemAnnouncementJob.Execute(job, systemSetting);

        // Assert
        await loggerService.Received().Error(job, result.Message);
    }

    [Theory, AutoData]
    public async Task Execute_HasAnnouncement_CallGetLinkedCustomersByFunction(ScheduleJob job, SystemSetting systemSetting)
    {
        // Arrange
        IEnumerable<Announcement> announcements = new List<Announcement>(){
            new Announcement{
                Id=1,
                Code="72973",
                Contents=new List<MessageContent>{new MessageContent{LanguageKey="en-US", Text="Hello"}},
                Environments=new List<string>{"Local"},
                IsActive=true,
                Specifications=Enumerable.Empty<ISpecification<IParam>>(),
                Systems=new List<string>{"B-IBCBet"},
                Title="Add 4 New Sport Type and 5 bettype",
                ReleasedDate=DateTime.Parse("2018-08-29 00:00:00.000"),
                ExpiredDate=DateTime.Parse("2018-08-29 00:00:00.000"),
                CreatedDate=DateTime.Parse("2018-08-29 00:00:00.000")
            }
        };
        announcementService.GetAnnouncements(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<string>()).Returns(Task.FromResult(announcements));
        var result = new Result(0, "");
        announcementService.Import(Arg.Any<List<SystemNotification>>(), Arg.Any<string>(), Arg.Any<DateTime>()).Returns(Task.FromResult(result));
        IEnumerable<LinkedCustomer> linkedCustomers = new List<LinkedCustomer>
        {
            new LinkedCustomer(1,0,"en-US",2,3,true,10)
        };
        customerService.GetLinkedCustomersByFunction(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(linkedCustomers));
        IEnumerable<CustomerStatus> customerStatus = new List<CustomerStatus>
        {
            new CustomerStatus(1,0,true,false,false,"")
        };
        customerService.GetCustomerStatus(Arg.Any<IEnumerable<CustomerStatusParam>>()).Returns(Task.FromResult(customerStatus));
        var custInfo = new Customer { CustId = 1, SiteCode = 1 };
        customerService.GetCustomerInfo(Arg.Any<int>()).Returns(Task.FromResult(custInfo));

        // Act
        await systemAnnouncementJob.Execute(job, systemSetting);

        // Assert
        await customerService.Received().GetLinkedCustomersByFunction(job.Name, job.Environment);
    }
}