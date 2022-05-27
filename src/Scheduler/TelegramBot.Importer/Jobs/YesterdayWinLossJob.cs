
using System.Diagnostics;
using Alpha.API.Client.Models.WinLoss;
using ONEbook.UM.Services;
using Quartz;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.PersonalMessages;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Services;
using ClientServices = Alpha.API.Client.Services;

namespace TelegramBot.Importer.Jobs;

[DisallowConcurrentExecution]
public sealed class YesterdayWinLossJob : NotificationBaseJob
{
    private readonly ICustomerService customerService;
    private readonly ClientServices.IWinLossService winLossService;

    public YesterdayWinLossJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService,
        ClientServices.IWinLossService winLossService)
        : base(maintenanceService, loggerService, systemSettingService)
    {
        this.customerService = customerService;
        this.winLossService = winLossService;
    }

    protected override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        bool continueGettingData;
        var yesterday = DateTime.Today.Yesterday();

        do
        {
            continueGettingData = await ImportYesterdayWinLoss(job, yesterday);

            await Task.Delay(1000);
        }
        while (continueGettingData);
    }

    private async Task<bool> ImportYesterdayWinLoss(ScheduleJob job, DateTime yesterday)
    {
        Stopwatch sw = Stopwatch.StartNew();

        await loggerService.Debug(job, "Start get Linked Customers");
        sw.Start();

        var (linkedCustomers, maxUserId) = await customerService.GetYesterdayWLLinkedCustomers(yesterday, job.Environment);

        sw.Stop();
        await loggerService.Debug(job, $"End get Linked Customers in {sw.Elapsed.TotalMilliseconds} - Data count {linkedCustomers.Count()}");

        var hasLinkedCustomer = linkedCustomers.Any();

        if (hasLinkedCustomer)
        {
            await loggerService.Debug(job, "Start get Customer Status");
            sw.Reset();
            sw.Start();

            var customerStatusParams = linkedCustomers.Select(c => new CustomerStatusParam(c.CustId, c.SubAccId));
            var customerStatus = await customerService.GetCustomerStatus(customerStatusParams);

            sw.Stop();
            await loggerService.Debug(job, $"End get Customer Status in {sw.Elapsed.TotalMilliseconds} - Data count {customerStatus.Count()}");

            await loggerService.Debug(job, "Start join 2 list LinkedCustomers, CustomerStatus");
            sw.Reset();
            sw.Start();

            var notifications = await BuildImportYesterdayWinLossList(job, linkedCustomers, customerStatus);

            sw.Stop();
            await loggerService.Debug(job, $"End join 2 list LinkedCustomers, CustomerStatus in {sw.Elapsed.TotalMilliseconds}");
        }

        return hasLinkedCustomer;
    }

    private async Task<IEnumerable<ImportYesterdayWinLoss>> BuildImportYesterdayWinLossList(
        ScheduleJob job,
        IEnumerable<LinkedCustomer> linkedCustomers,
        IEnumerable<CustomerStatus> customerStatus)
    {
        var query =
            from customer in linkedCustomers
            join status in customerStatus on new { customer.CustId, customer.SubAccId }
                equals new { status.CustId, status.SubAccId }
            where status.IsValid(job.SubAccPermission)
            select customer;

        var yesterday = DateTime.Today.Yesterday();
        var notifications = new List<ImportYesterdayWinLoss>();

        await Parallel.ForEachAsync(
            query,
            async (customer, cancellationToken) =>
            {
                var yesterdayWinLoss = await BuildImportYesterdayWinLoss(job, customer, yesterday);

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
        DateTime yesterday)
    {
        try
        {
            string template, message;
            var winloss = await winLossService.GetWinLoss(customer.CustId, customer.CustomerLevel, yesterday);

            if (winloss == null)
            {
                template = "";//GetTemplate("NoInfo", customer.Language);
                message = string.Format(template, yesterday.ToSimpleDate());
            }
            else
            {
                template = "";//GetTemplate(job.Name, customer.CustomerLevel, customer.Language);
                message = customer.CustomerLevel switch
                {
                    CustomerLevel.Super => GetWinLossSuper(template, yesterday, (WinLossMaster)winloss),
                    CustomerLevel.Master => GetWinLossMaster(template, yesterday, (WinLossAgent)winloss),
                    CustomerLevel.Agent => GetWinLossAgent(template, yesterday, (WinLossMember)winloss),
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(customer.CustomerLevel),
                        $"Not expected custLevel value: {customer.CustomerLevel}")
                };
            }

            return new ImportYesterdayWinLoss(customer.UserId, message, NotificationStatus.Initial);
        }
        catch (Exception ex)
        {
            await loggerService.Error(ex, job, $"Could not get Yesterday Win Loss of CustId={customer.CustId}, CustLevel={customer.CustomerLevel}, DateTime={yesterday}");
            return null;
        }
    }

    private static string GetWinLossSuper(string template, DateTime yesterday, WinLossMaster winloss)
        => string.Format(
            template,
            yesterday.ToSimpleDate(),
            winloss.Turnover.FormatDecimal(),
            winloss.GrossComm.FormatDecimal(),
            winloss.MasterTotal.FormatDecimal(),
            winloss.SuperWinLoss.FormatDecimal(),
            winloss.SuperComm.FormatDecimal(),
            winloss.SuperTotal.FormatDecimal(),
            winloss.CompanyWinLoss.FormatDecimal());

    private static string GetWinLossMaster(string template, DateTime yesterday, WinLossAgent winloss)
        => string.Format(
            template,
            yesterday.ToSimpleDate(),
            winloss.Turnover.FormatDecimal(),
            winloss.GrossComm.FormatDecimal(),
            winloss.AgentTotal.FormatDecimal(),
            winloss.MasterWinLoss.FormatDecimal(),
            winloss.MasterComm.FormatDecimal(),
            winloss.MasterTotal.FormatDecimal(),
            winloss.CompanyWinLoss.FormatDecimal());

    private static string GetWinLossAgent(string template, DateTime yesterday, WinLossMember winloss)
        => string.Format(
            template,
            yesterday.ToSimpleDate(),
            winloss.Turnover.FormatDecimal(),
            winloss.GrossComm.FormatDecimal(),
            winloss.MemberWinLoss.FormatDecimal(),
            winloss.MemberComm.FormatDecimal(),
            winloss.MemberTotal.FormatDecimal(),
            winloss.AgentWinLoss.FormatDecimal(),
            winloss.AgentComm.FormatDecimal(),
            winloss.AgentTotal.FormatDecimal(),
            winloss.CompanyWinLoss.FormatDecimal());
}