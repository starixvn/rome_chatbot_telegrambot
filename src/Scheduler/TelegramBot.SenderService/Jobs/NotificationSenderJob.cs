using ONEbook.UM.Services;
using Quartz;
using Telegram.Bot.Exceptions;
using TelegramBot.SenderService.Configurations;
using TelegramBot.SenderService.Constants;
using TelegramBot.SenderService.Models;
using TelegramBot.SenderService.Services;

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
    private readonly IBotService botService;

    public NotificationSenderJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        INotificationService notificationService,
        ICustomerService customerService,
        AppSettings appSettings,
        IBotService botService)
    {
        this.maintenanceService = maintenanceService;
        this.loggerService = loggerService;
        this.systemSettingService = systemSettingService;
        this.notificationService = notificationService;
        this.customerService = customerService;
        this.appSettings = appSettings;
        this.botService = botService;
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

            var notificationStatus = await ChunkAndSendNotifications(notifications, notificationChunkSize);

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

    public async Task<IEnumerable<CustomerStatus>> GetCustomerStatus(IEnumerable<Notification> notifications)
    {
        var customerStatusParams = notifications.Select(n => new CustomerStatusParam(n.CustId, n.SubAccId));
        return await customerService.GetCustomerStatus(customerStatusParams);
    }

    private async Task<IEnumerable<NotificationStatusParam>> ChunkAndSendNotifications(IEnumerable<Notification> notifications, int notificationChunkSize)
    {
        var status = new List<NotificationStatusParam>();
        var customerStatus = await GetCustomerStatus(notifications);

        var sleepTime = 1000 / notificationChunkSize;
        var chunkNotifications = notifications.Chunk(notificationChunkSize);

        foreach (var notificationArray in chunkNotifications)
        {

            status.AddRange(await SendNotifications(notificationArray, customerStatus, sleepTime));
        }

        return status;
    }

    private async Task<IEnumerable<NotificationStatusParam>> SendNotifications(Notification[] notifications, IEnumerable<CustomerStatus> customerStatus, int sleepTime)
    {
        var status = new List<NotificationStatusParam>();

        foreach (var item in notifications)
        {
            appSettings.FunctionPermissions.TryGetValue(item.FunctionKey, out var subAccPermission);
            var custStatus = customerStatus.FirstOrDefault(status => status.CustId == item.CustId && status.SubAccId == item.SubAccId);
            if (custStatus != null)
            {
                if (custStatus.IsInvalid(subAccPermission, out var reason))
                {
                    status.Add(new NotificationStatusParam(item.NotificationId, NotificationStatus.Invalid, reason));
                }
                else
                {
                    var result = await NotifyToUser(item);
                    status.Add(new NotificationStatusParam(item.NotificationId, result.IsFailed ? NotificationStatus.Fail : NotificationStatus.Completed, result.Message));
                    await Task.Delay(sleepTime);
                }
            }
        }

        return status;
    }

    private async Task<Result> NotifyToUser(Notification notification)
    {
        try
        {
            await botService.SendMessage(notification.BotId, notification.ChatId, notification.Message);

            return Result.CreateSuccessfulResult();
        }
        catch (Exception ex)
        {
            if (ex is ApiRequestException apiRequestException && TelegramError.IgnoredErrorCodes.Contains(apiRequestException.ErrorCode))
            {
                return new Result(apiRequestException.ErrorCode, apiRequestException.Message);
            }
            else
            {
                SendToRetryService(notification);
                return new Result(500, ex.Message);
            }
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