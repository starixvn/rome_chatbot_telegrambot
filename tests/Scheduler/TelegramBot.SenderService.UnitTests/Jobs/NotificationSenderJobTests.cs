using ONEbook.UM.Models;
using ONEbook.UM.Services;
using Quartz;
using Telegram.Bot.Exceptions;
using TelegramBot.SenderService.Configurations;
using TelegramBot.SenderService.Constants;
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
        appSettings = new AppSettings() { FunctionPermissions = new Dictionary<string, string> { ["MyWatchlist"] = "[H]" } };
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

    [Theory, AutoData]
    public async Task UpdateNotificationStatus_Always_CallsUpdateNotificationStatus(IEnumerable<NotificationStatusParam> notifications, IEnumerable<LastNotification> lastNotifications, string jobName, string environment)
    {
        // Arrange
        notificationService.UpdateNotificationStatus(Arg.Any<IEnumerable<NotificationStatusParam>>(), Arg.Any<IEnumerable<LastNotification>>(), Arg.Any<string>())
            .Returns(Task.FromResult(new Result(0, "")));

        // Act
        await notificationSenderJob.UpdateNotificationStatus(notifications, lastNotifications, jobName, environment);

        // Assert
        await notificationService.Received().UpdateNotificationStatus(Arg.Is(notifications), Arg.Is(lastNotifications), Arg.Is(environment));
    }

    [Theory, AutoData]
    public async Task UpdateNotificationStatus_UpdateFailed_CallsLogError(IEnumerable<NotificationStatusParam> notifications, IEnumerable<LastNotification> lastNotifications, string jobName, string environment)
    {
        // Arrange
        notificationService.UpdateNotificationStatus(Arg.Any<IEnumerable<NotificationStatusParam>>(), Arg.Any<IEnumerable<LastNotification>>(), Arg.Any<string>())
            .Returns(Task.FromResult(new Result(1, "errMessage")));

        // Act
        await notificationSenderJob.UpdateNotificationStatus(notifications, lastNotifications, jobName, environment);

        // Assert
        await loggerService.Received().Error(jobName, environment, "errMessage");
    }

    [Theory, AutoData]
    public async Task HandleException_IsNOTApiRequestException_CallsRetrySendNotification(string environment, Notification notification, IList<NotificationStatusParam> status)
    {
        // Arrange
        var exception = new Exception();

        // Act
        await notificationSenderJob.HandleException(exception, environment, notification, status);

        // Assert
        await messageSender.Received().RetrySendNotification(notification, environment);
    }

    [Theory, AutoData]
    public async Task HandleException_IsFORBIDDENException_AddBotBlockedStatusParam(string environment, Notification notification, IList<NotificationStatusParam> status)
    {
        // Arrange
        var exception = new ApiRequestException("", TelegramError.FORBIDDEN);
        var param = new NotificationStatusParam(notification.NotificationId, NotificationStatus.BotBlocked, exception.Message);

        // Act
        await notificationSenderJob.HandleException(exception, environment, notification, status);

        // Assert
        Assert.Contains(param, status);
    }

    [Theory, AutoData]
    public async Task HandleException_ErrorCodeIsIgnore_AddFailedStatusParam(string environment, Notification notification, IList<NotificationStatusParam> status)
    {
        // Arrange
        var exception = new ApiRequestException("", 400);
        var param = new NotificationStatusParam(notification.NotificationId, NotificationStatus.Fail, exception.Message);

        // Act
        await notificationSenderJob.HandleException(exception, environment, notification, status);

        // Assert
        Assert.Contains(param, status);
    }

    [Theory, AutoData]
    public async Task HandleException_ErrorCodeIsUnHandle_CallsRetrySendNotification(string environment, Notification notification, IList<NotificationStatusParam> status)
    {
        // Arrange
        var exception = new ApiRequestException("", 500);

        // Act
        await notificationSenderJob.HandleException(exception, environment, notification, status);

        // Assert
        await messageSender.Received().RetrySendNotification(notification, environment);
    }

    [Theory, AutoData]
    public async Task SendNotifications_CouldNotFoundCustomerStatus_AddInvalidStatusParam(string environment)
    {
        // Arrange
        var notifications = new Notification[] { new Notification() { NotificationId = 1, CustId = 1, SubAccId = 1, FunctionKey = "MyWatchlist" } };
        var customerStatus = Enumerable.Empty<CustomerStatus>();
        var status = new NotificationStatusParam(1, NotificationStatus.Invalid, "Invalid customer status CustId: 1, SubAccId: 1, Id: 1");

        // Act
        var actualResult = await notificationSenderJob.SendNotifications(notifications, customerStatus, environment);

        // Assert
        Assert.Contains(status, actualResult);
    }

    [Theory, AutoData]
    public async Task SendNotifications_CustomerStatusIsInvalid_AddInvalidStatusParam(string environment)
    {
        // Arrange
        var notifications = new Notification[] { new Notification() { NotificationId = 1, CustId = 1, SubAccId = 1, FunctionKey = "MyWatchlist" } };
        IEnumerable<CustomerStatus> customerStatus = new List<CustomerStatus>()
        {
            new CustomerStatus(1,1,false,false,false,"[H]")
        };
        var status = new NotificationStatusParam(1, NotificationStatus.Invalid, "Customer Status: Disabled or Closed");

        // Act
        var actualResult = await notificationSenderJob.SendNotifications(notifications, customerStatus, environment);

        // Assert
        Assert.Contains(status, actualResult);
    }

    [Theory, AutoData]
    public async Task SendNotifications_CustomerStatusIsValid_AddCompletedStatusParam(string environment)
    {
        // Arrange
        var notifications = new Notification[] { new Notification() { NotificationId = 1, CustId = 1, SubAccId = 1, FunctionKey = "MyWatchlist" } };
        IEnumerable<CustomerStatus> customerStatus = new List<CustomerStatus>()
        {
            new CustomerStatus(1,1,true,false,false,"[H]")
        };
        var status = new NotificationStatusParam(1, NotificationStatus.Completed);

        // Act
        var actualResult = await notificationSenderJob.SendNotifications(notifications, customerStatus, environment);

        // Assert
        Assert.Contains(status, actualResult);
    }
}