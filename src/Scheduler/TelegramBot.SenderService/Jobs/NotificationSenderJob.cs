using ONEbook.UM.Services;
using Quartz;
using TelegramBot.SenderService.Configurations;
using TelegramBot.SenderService.Constants;
using TelegramBot.SenderService.Models;
using TelegramBot.SenderService.Services;
using TelegramBotNotificationService = TelegramBot.Client.Services.INotificationService;

namespace TelegramBot.SenderService.Jobs;

[DisallowConcurrentExecution]
public sealed class NotificationSenderJob : IJob
{
    private readonly IMaintenanceService maintenanceService;
    private readonly ILoggerService loggerService;
    private readonly ISystemSettingService systemSettingService;
    private readonly INotificationService notificationService;
    private readonly ICustomerService customerService;
    private readonly AppSettings appSettings;
    private readonly TelegramBotNotificationService telegramBotNotificationService;

    public NotificationSenderJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        INotificationService notificationService,
        ICustomerService customerService,
        AppSettings appSettings,
        TelegramBotNotificationService telegramBotNotificationService)
    {
        this.maintenanceService = maintenanceService;
        this.loggerService = loggerService;
        this.systemSettingService = systemSettingService;
        this.notificationService = notificationService;
        this.customerService = customerService;
        this.appSettings = appSettings;
        this.telegramBotNotificationService = telegramBotNotificationService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var jobName = dataMap.GetString(JobDataKey.JobName);
        var environment = dataMap.GetString(JobDataKey.Environment);
        var notificationChunkSize = dataMap.GetIntValue(JobDataKey.NotificationChunkSize);

        await loggerService.Debug($"{DateTime.Now} - {jobName}: Start job.");

        if (await IsUM(jobName, environment))
        {
            return;
        }

        try
        {
            var systemSetting = await systemSettingService.GetSystemSetting(jobName, environment);

            if (IsStopJob(systemSetting))
            {
                return;
            }

            var (notifications, lastNotifications) = await notificationService.GetNotifications(environment);

            if (!notifications.Any())
            {
                return;
            }

            var successNotifications = new List<Notification>();
            var failureNotificationStatus = new List<NotificationStatusParam>();

            await JoinWithCustomerStatus(notifications, successNotifications, failureNotificationStatus);

            var notificationStatus = await ChunkAndSendNotifications(
                successNotifications,
                failureNotificationStatus,
                notificationChunkSize);

            await UpdateNotificationStatus(notificationStatus, lastNotifications, jobName, environment);
        }
        catch (Exception ex)
        {
            await loggerService.Error(ex, jobName, environment);
        }

        await loggerService.Debug($"{DateTime.Now} - {jobName}: End job.");
    }

    private async Task<bool> IsUM(string jobName, string environment)
    {
        try
        {
            var umInfo = await maintenanceService.GetUnderMaintenanceTimeAsync();

            return umInfo.IsUnderMaintenanceTime;
        }
        catch (Exception ex)
        {
            await loggerService.Error(ex, jobName, environment, "Could not get UM information.");
        }

        return false;
    }

    private static bool IsStopJob(SystemSetting systemSetting)
        => systemSetting == null || !systemSetting.ServiceFlag;

    private async Task JoinWithCustomerStatus(
        IEnumerable<Notification> notifications,
        IList<Notification> successNotifications,
        IList<NotificationStatusParam> failureNotificationStatus)
    {
        var customerStatusParams = notifications
            .Select(n => new CustomerStatusParam(n.CustId, n.SubAccId));
        var customerStatus = await customerService.GetCustomerStatus(customerStatusParams);

        var query =
            from notification in notifications
            join status in customerStatus on new { notification.CustId, notification.SubAccId }
                equals new { status.CustId, status.SubAccId }
            select new
            {
                Notification = notification,
                CustomerStatus = status
            };

        foreach (var item in query)
        {
            UpdateSuccessAndFailureNotifications(
                successNotifications,
                failureNotificationStatus,
                item.Notification,
                item.CustomerStatus);
        }
    }

    private void UpdateSuccessAndFailureNotifications(
        IList<Notification> successNotifications,
        IList<NotificationStatusParam> failureNotificationStatus,
        Notification notification,
        CustomerStatus customerStatus)
    {
        if (notification.ErrCode != 0)
        {
            failureNotificationStatus.Add(new NotificationStatusParam(
                notification.NotificationId,
                NotificationStatus.Invalid,
                ConvertToDBErrorMessage(notification.ErrCode)));
        }
        else
        {
            appSettings.FunctionPermissions.TryGetValue(notification.FunctionKey, out var subAccPermission);
            var customerStatusText = customerStatus.GetStatus(subAccPermission);

            if (customerStatusText.Length > 0)
            {
                failureNotificationStatus.Add(new NotificationStatusParam(
                    notification.NotificationId,
                    NotificationStatus.Invalid,
                    customerStatusText));
            }
            else
            {
                successNotifications.Add(notification);
            }
        }
    }

    private static string ConvertToDBErrorMessage(int errorCode)
        => errorCode switch
        {
            1 => "Turn off Notification Setting",
            2 => "Unlink",
            3 => "Unverify OTP",
            _ => "Unknown"
        };

    private async Task<IEnumerable<NotificationStatusParam>> ChunkAndSendNotifications(
        IEnumerable<Notification> notifications,
        IList<NotificationStatusParam> failureNotificationStatus,
        int notificationChunkSize)
    {
        var successNotificationStatus = new List<NotificationStatusParam>();

        var sleepTime = 1000 / notificationChunkSize;
        var chunkNotifications = notifications.Chunk(notificationChunkSize);

        foreach (var notificationArray in chunkNotifications)
        {
            await SendNotifications(
                notificationArray,
                successNotificationStatus,
                failureNotificationStatus,
                sleepTime);
        }

        successNotificationStatus.AddRange(failureNotificationStatus);

        return successNotificationStatus;
    }

    private async Task SendNotifications(
        Notification[] notifications,
        IList<NotificationStatusParam> successNotificationStatus,
        IList<NotificationStatusParam> failureNotificationStatus,
        int sleepTime)
    {
        Result result;

        foreach (var item in notifications)
        {
            result = await NotifyToUser(item);

            if (result.IsFailed)
            {
                failureNotificationStatus.Add(new NotificationStatusParam(
                    item.NotificationId,
                    NotificationStatus.Fail,
                    result.Message));
            }
            else
            {
                successNotificationStatus.Add(new NotificationStatusParam(
                    item.NotificationId,
                    NotificationStatus.Completed));
            }

            Thread.Sleep(sleepTime);
        }
    }

    private async Task<Result> NotifyToUser(Notification notification)
    {
        try
        {
            var telegramBotResult = await telegramBotNotificationService.Notify(
                notification.BotId,
                notification.ChatId,
                notification.Message);
            
            return new Result(telegramBotResult.Code, telegramBotResult.Message);
        }
        catch (Exception ex)
        {
            SendToRetryService(notification);

            return new Result(500, ex.Message);
        }
    }

    private static void SendToRetryService(Notification notification)
    {
        try
        {
        }
        catch (Exception ex)
        {
        }
    }

    private async Task UpdateNotificationStatus(
        IEnumerable<NotificationStatusParam> notifications,
        IEnumerable<LastNotification> lastNotifications,
        string jobName,
        string environment)
    {
        var result = await notificationService.UpdateNotificationStatus(
            notifications,
            lastNotifications,
            environment);

        if (result.IsFailed)
        {
            await loggerService.Error(jobName, environment, result.Message);
        }
    }
}