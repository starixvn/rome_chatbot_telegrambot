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
    private readonly IMessageSender messageSender;

    private const int SleepTime = 1000;

    public NotificationSenderJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        INotificationService notificationService,
        ICustomerService customerService,
        AppSettings appSettings,
        IMessageSender messageSender)
    {
        this.maintenanceService = maintenanceService;
        this.loggerService = loggerService;
        this.systemSettingService = systemSettingService;
        this.notificationService = notificationService;
        this.customerService = customerService;
        this.appSettings = appSettings;
        this.messageSender = messageSender;
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

            var notificationStatus = await SendNotificationByBatch(notifications, notificationChunkSize, environment);

            await UpdateNotificationStatus(notificationStatus, lastNotifications, jobName, environment);
        }
        catch (Exception ex)
        {
            await loggerService.Error(ex, jobName, environment);
        }
        finally
        {
            await loggerService.Debug($"{DateTime.Now} - {jobName}: End job.");
        }
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

    private async Task<IEnumerable<NotificationStatusParam>> SendNotificationByBatch(IEnumerable<Notification> notifications, int notificationChunkSize, string environment)
    {
        var status = new List<NotificationStatusParam>();
        var customerStatus = await GetCustomerStatus(notifications);

        var batches = GetBatches(notifications, notificationChunkSize);
        foreach (var notificationArray in batches)
        {
            status.AddRange(await SendNotifications(notificationArray, customerStatus, environment));
            await Task.Delay(SleepTime);
        }

        return status;
    }

    private async Task<IEnumerable<NotificationStatusParam>> SendNotifications(
        Notification[] notifications,
        IEnumerable<CustomerStatus> customerStatus,
        string environment)
    {
        var status = new List<NotificationStatusParam>();
        foreach (var item in notifications)
        {
            appSettings.FunctionPermissions.TryGetValue(item.FunctionKey, out var subAccPermission);
            var custStatus = customerStatus.FirstOrDefault(status => status.CustId == item.CustId && status.SubAccId == item.SubAccId);
            if (custStatus == null)
            {
                var message = $"Invalid customer status CustId: {item.CustId}, SubAccId: {item.SubAccId}, Id: {item.NotificationId} ";
                status.Add(new NotificationStatusParam(item.NotificationId, NotificationStatus.Invalid, message));
                continue;
            }

            if (custStatus.IsInvalid(subAccPermission, out var reason))
            {
                status.Add(new NotificationStatusParam(item.NotificationId, NotificationStatus.Invalid, reason));
                continue;
            }

            try
            {
                await messageSender.SendNotification(item);
                status.Add(new NotificationStatusParam(item.NotificationId, NotificationStatus.Completed));
            }
            catch (Exception ex)
            {
                await loggerService.Error("SendNotifications", environment, ex.Message);
                if (ex is ApiRequestException apiRequestException
                && TelegramError.IgnoredErrorCodes.Contains(apiRequestException.ErrorCode))
                {
                    status.Add(new NotificationStatusParam(item.NotificationId, NotificationStatus.Fail, ex.Message));
                }
                else
                {
                    await messageSender.RetrySendNotification(item, environment);
                }
            }
        }

        return status;
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

    public static IEnumerable<Notification[]> GetBatches(IEnumerable<Notification> source, int count)
    {
        var hashset = new HashSet<int>(count);
        var batch = new List<Notification>(count);
        var leftOvers = new Queue<Notification>();

        while (true)
        {
            foreach (var item in source)
            {

                if (hashset.Add(item.UserId))
                {
                    batch.Add(item);
                }
                else
                {
                    leftOvers.Enqueue(item);
                }

                while (batch.Count == count)
                {
                    yield return batch.ToArray();

                    batch.Clear();
                    hashset.Clear();

                    while (leftOvers.Any() && batch.Count != count)
                    {
                        if (hashset.Add(leftOvers.Peek().UserId))
                        {
                            batch.Add(leftOvers.Dequeue());
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            if (batch.Any())
            {
                yield return batch.ToArray();
            }

            if (!leftOvers.Any())
            {
                break;
            }

            source = leftOvers.ToList();
            hashset.Clear();
            batch.Clear();
            leftOvers.Clear();
        }
    }
}