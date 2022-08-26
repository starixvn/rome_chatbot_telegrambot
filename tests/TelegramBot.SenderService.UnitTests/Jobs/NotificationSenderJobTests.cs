using ONEbook.UM.Models;
using ONEbook.UM.Services;
using Quartz;
using TelegramBot.SenderService.Configurations;
using TelegramBot.SenderService.Jobs;
using TelegramBot.SenderService.Models;
using TelegramBot.SenderService.Services;

namespace TelegramBot.SenderService.UnitTests.Jobs;

public class NotificationSenderJobTests
{
    private readonly IMaintenanceService maintenanceService;
    private readonly ILoggerService loggerService;
    private readonly ISystemSettingService systemSettingService;
    private readonly INotificationService notificationService;
    private readonly ICustomerService customerService;
    private readonly AppSettings appSettings;
    private readonly IMessageSender messageSender;
    private readonly NotificationSenderJob notificationSenderJob;

    public NotificationSenderJobTests()
    {
        maintenanceService = Substitute.For<IMaintenanceService>();
        loggerService = Substitute.For<ILoggerService>();
        systemSettingService = Substitute.For<ISystemSettingService>();
        notificationService = Substitute.For<INotificationService>();
        customerService = Substitute.For<ICustomerService>();
        appSettings = Substitute.For<AppSettings>();
        messageSender = Substitute.For<IMessageSender>();
        notificationSenderJob = new NotificationSenderJob(
            maintenanceService,
            loggerService,
            systemSettingService,
            notificationService,
            customerService,
            appSettings,
            messageSender);
    }

    [Fact]
    public async Task Execute_IsUnderMaintenance_DoesNOTCallsGetSystemSetting()
    {
        // Arrange
        var context = Substitute.For<IJobExecutionContext>();
        IDictionary<string, object> keyValuePairs = new Dictionary<string, object> {
            { "JobName", "SendNotification" },
            { "Environment", "UAT" },
            { "NotificationChunkSize", 20 }
        };
        context.JobDetail.JobDataMap.Returns(new JobDataMap(keyValuePairs));
        var underMaintenanceTime = new UnderMaintenanceTime(true, "3:00", "4:00");
        maintenanceService.GetUnderMaintenanceTimeAsync().Returns(Task.FromResult(underMaintenanceTime));

        // Act
        await notificationSenderJob.Execute(context);

        // Assert
        await systemSettingService.DidNotReceive().GetSystemSetting(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Execute_HasExceptionWhenGetUM_CallsLogError()
    {
        // Arrange
        var context = Substitute.For<IJobExecutionContext>();
        IDictionary<string, object> keyValuePairs = new Dictionary<string, object> {
            { "JobName", "SendNotification" },
            { "Environment", "UAT" },
            { "NotificationChunkSize", 20 }
        };
        context.JobDetail.JobDataMap.Returns(new JobDataMap(keyValuePairs));
        var exception = new Exception("ErrorMessage");
        maintenanceService.GetUnderMaintenanceTimeAsync().Returns(Task.FromException<UnderMaintenanceTime>(exception));

        // Act
        await notificationSenderJob.Execute(context);

        // Assert
        await loggerService.Received().Error(exception, "SendNotification", "UAT", "Could not get UM information.");
    }

    [Fact]
    public async Task Execute_IsStopJob_DoesNOTCallsGetNotifications()
    {
        // Arrange
        var context = Substitute.For<IJobExecutionContext>();
        IDictionary<string, object> keyValuePairs = new Dictionary<string, object> {
            { "JobName", "SendNotification" },
            { "Environment", "UAT" },
            { "NotificationChunkSize", 20 }
        };
        context.JobDetail.JobDataMap.Returns(new JobDataMap(keyValuePairs));
        var underMaintenanceTime = new UnderMaintenanceTime(false, "3:00", "4:00");
        maintenanceService.GetUnderMaintenanceTimeAsync().Returns(Task.FromResult(underMaintenanceTime));
        systemSettingService.GetSystemSetting(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult<SystemSetting>(null));

        // Act
        await notificationSenderJob.Execute(context);

        // Assert
        await notificationService.DidNotReceive().GetNotifications(Arg.Any<string>());
    }

    [Theory, AutoData]
    public async Task Execute_IsJobRunning_CallsGetNotifications(IEnumerable<Notification> notifications, IEnumerable<LastNotification> lastNotifications)
    {
        // Arrange
        var context = Substitute.For<IJobExecutionContext>();
        IDictionary<string, object> keyValuePairs = new Dictionary<string, object> {
            { "JobName", "SendNotification" },
            { "Environment", "UAT" },
            { "NotificationChunkSize", 20 }
        };
        context.JobDetail.JobDataMap.Returns(new JobDataMap(keyValuePairs));
        var underMaintenanceTime = new UnderMaintenanceTime(false, "3:00", "4:00");
        maintenanceService.GetUnderMaintenanceTimeAsync().Returns(Task.FromResult(underMaintenanceTime));
        IDictionary<string, string> settings = new Dictionary<string, string> { { "ServiceFlag", "1" } };
        systemSettingService.GetSystemSetting(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(new SystemSetting(settings, "UAT")));
        notificationService.GetNotifications(Arg.Any<string>()).Returns(Task.FromResult((notifications, lastNotifications)));

        // Act
        await notificationSenderJob.Execute(context);

        // Assert
        await notificationService.Received().GetNotifications("UAT");
    }
}