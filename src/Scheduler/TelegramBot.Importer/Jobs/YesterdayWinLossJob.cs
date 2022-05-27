
using ONEbook.UM.Services;
using Quartz;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.PersonalMessages;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.Jobs;

[DisallowConcurrentExecution]
public sealed class YesterdayWinLossJob : NotificationBaseJob
{
    private readonly ICustomerService customerService;
    private readonly IYesterdayWinLossService yesterdayWinLossService;

    public YesterdayWinLossJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService,
        IYesterdayWinLossService yesterdayWinLossService)
        : base(maintenanceService, loggerService, systemSettingService)
    {
        this.customerService = customerService;
        this.yesterdayWinLossService = yesterdayWinLossService;
    }

    protected override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        bool continueGettingData;
        var isFirstBatch = true;
        const int delayTime = 1000;
        var winLossDate = DateTime.Today.Yesterday();

        do
        {
            // Must update JobStatus=Running in 2nd, 3rd, ... batch.
            if (!isFirstBatch)
            {
                await UpdateJobStatusToRunning(job);
            }

            continueGettingData = await ImportYesterdayWinLoss(job, winLossDate);
            isFirstBatch = false;

            await Task.Delay(delayTime);
        }
        while (continueGettingData);
    }

    private async Task<bool> ImportYesterdayWinLoss(ScheduleJob job, DateTime winLossDate)
    {
        var (linkedCustomers, maxUserId) = await customerService.GetYesterdayWinLossLinkedCustomers(winLossDate, job.Environment);
        var hasLinkedCustomer = linkedCustomers.Any();

        try
        {
            var notifications = hasLinkedCustomer
                ? await BuildImportYesterdayWinLoss(job, linkedCustomers, winLossDate)
                : Enumerable.Empty<ImportYesterdayWinLoss>();

            // In case of NoData, call Import to update JobStatus=Completed and LastRuntime.
            var result = await yesterdayWinLossService.Import(notifications, winLossDate, job.Environment, maxUserId);

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

    private async Task<IEnumerable<ImportYesterdayWinLoss>> BuildImportYesterdayWinLoss(
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

        var notifications = new List<ImportYesterdayWinLoss>();

        await Parallel.ForEachAsync(
            query,
            async (customer, _) =>
            {
                var yesterdayWinLoss = await BuildImportYesterdayWinLoss(job, customer, winLossDate);

                if (yesterdayWinLoss != null)
                {
                    notifications.Add(yesterdayWinLoss);
                }
            });

        return notifications;
    }

    private async Task<ImportYesterdayWinLoss> BuildImportYesterdayWinLoss(
        ScheduleJob job,
        LinkedCustomer customer,
        DateTime winLossDate)
    {
        try
        {
            var winloss = await yesterdayWinLossService.GetYesterdayWinLoss(customer.CustId, customer.CustomerLevel, winLossDate);
            var message = yesterdayWinLossService.BuildMessage(winloss, customer.CustomerLevel, customer.Language, winLossDate.ToSimpleDate());
            
            return new ImportYesterdayWinLoss(customer.UserId, message, customer.IsRealUser && job.IsUAT ? NotificationStatus.Draft : NotificationStatus.Initial);
        }
        catch (Exception ex)
        {
            await loggerService.Error(ex, job, $"Could not get Yesterday Win Loss of CustId={customer.CustId}, CustLevel={customer.CustomerLevel}, DateTime={winLossDate}");
            return null;
        }
    }
}