
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Services;
using ClientServices = Alpha.API.Client.Services;

namespace TelegramBot.Importer.Jobs;

public sealed class YesterdayWinLossJob : NotificationBaseJob
{
    private readonly ClientServices.IWinLossService winLossService;
    private readonly IMessageTemplate messageTemplate;
    private readonly IImportNotificationService importNotificationService;

    public YesterdayWinLossJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService,
        ClientServices.IWinLossService winLossService,
        IMessageTemplate messageTemplate,
        IImportNotificationService importNotificationService)
        : base(maintenanceService, loggerService, systemSettingService, customerService)
    {
        this.winLossService = winLossService;
        this.messageTemplate = messageTemplate;
        this.importNotificationService = importNotificationService;
    }

    protected override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        bool continueGettingData;
        var isFirstBatch = true;
        const int delayTime = 500;
        var winLossDate = DateTime.Today.Yesterday();

        do
        {
            // Must update JobStatus=Running in 2nd, 3rd, ... batch.
            if (!isFirstBatch)
            {
                await UpdateJobStatusToRunning(job);
            }

            continueGettingData = await ImportYesterdayWinLossNotification(job, winLossDate);
            isFirstBatch = false;

            await Task.Delay(delayTime);
        }
        while (continueGettingData);
    }

    private async Task<bool> ImportYesterdayWinLossNotification(ScheduleJob job, DateTime winLossDate)
    {
        var (linkedCustomers, maxUserId) = await customerService.GetYesterdayWinLossLinkedCustomers(winLossDate, job.Environment);
        var hasLinkedCustomer = linkedCustomers.Any();

        try
        {
            var notifications = hasLinkedCustomer
                ? await BuildYesterdayWinLossNotification(job, linkedCustomers, winLossDate)
                : Enumerable.Empty<Notification>();

            // In case of NoData, call Import to update JobStatus=Completed and LastRuntime.
            var result = await importNotificationService.ImportYesterdayWinLoss(notifications, winLossDate, job.Environment, maxUserId);

            if (result.IsFailed)
            {
                await loggerService.Error(job, result.Message);
            }
        }
        catch (Exception ex)
        {
            await loggerService.Error(ex, job);
        }

        return hasLinkedCustomer;
    }

    private async Task<IEnumerable<Notification>> BuildYesterdayWinLossNotification(
        ScheduleJob job,
        IEnumerable<LinkedCustomer> linkedCustomers,
        DateTime winLossDate)
    {
        var customerStatusParams = linkedCustomers.Select(c => new CustomerStatusParam(c.CustId, c.SubAccId));
        var customerStatus = await customerService.GetCustomerStatus(customerStatusParams);

        var query =
            from customer in linkedCustomers
            join status in customerStatus on new { customer.CustId, customer.SubAccId }
                equals new { status.CustId, status.SubAccId }
            where status.IsValid(job.SubAccPermission)
            select customer;

        var notifications = new List<Notification>();

        await Parallel.ForEachAsync(
            query,
            async (customer, _) =>
            {
                var yesterdayWinLoss = await BuildYesterdayWinLossNotification(job, customer, winLossDate);

                if (yesterdayWinLoss != null)
                {
                    notifications.Add(yesterdayWinLoss);
                }
            });

        return notifications;
    }

    private async Task<Notification> BuildYesterdayWinLossNotification(
        ScheduleJob job,
        LinkedCustomer customer,
        DateTime winLossDate)
    {
        try
        {
            var winloss = await winLossService.GetWinLoss(customer.CustId, customer.CustomerLevel, winLossDate);
            var message = messageTemplate.BuildYesterdayWinLossMessage(winloss, customer.CustomerLevel, customer.Language, winLossDate.ToSimpleDate());
            
            return new Notification(customer.UserId, message, customer.IsRealUser, job.IsUAT);
        }
        catch (Exception ex)
        {
            await loggerService.Error(ex, job, $"Could not get Yesterday Win Loss of CustId={customer.CustId}, CustLevel={customer.CustomerLevel}, DateTime={winLossDate}");
            return null;
        }
    }
}