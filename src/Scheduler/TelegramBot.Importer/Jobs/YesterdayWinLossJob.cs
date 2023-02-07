
using System.Runtime.CompilerServices;
using System.Text;
using Alpha.API.Client.Exceptions;
using Alpha.API.Client.Models.WinLoss;
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Resources;
using TelegramBot.Importer.Services;

[assembly: InternalsVisibleTo("TelegramBot.Importer.UnitTests")]
namespace TelegramBot.Importer.Jobs;

public sealed class YesterdayWinLossJob : NotificationBaseJob
{
    private readonly IYesterdayWinLossService yesterdayWinLossService;

    public YesterdayWinLossJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService,
        IYesterdayWinLossService yesterdayWinLossService)
        : base(maintenanceService, loggerService, systemSettingService, customerService)
        => this.yesterdayWinLossService = yesterdayWinLossService;

    public override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
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

    internal async Task<bool> ImportYesterdayWinLossNotification(ScheduleJob job, DateTime winLossDate)
    {
        var (linkedCustomers, maxUserId) = await customerService.GetYesterdayWinLossLinkedCustomers(winLossDate, job.Name, job.Environment);
        var hasLinkedCustomer = linkedCustomers.Any();

        try
        {
            var notifications = hasLinkedCustomer
                ? await BuildYesterdayWinLossNotification(job, linkedCustomers, winLossDate)
                : Enumerable.Empty<Notification>();

            // In case of NoData, call Import to update JobStatus=Completed and LastRuntime.
            var result = await yesterdayWinLossService.Import(notifications, winLossDate, job.Name, job.Environment, maxUserId);

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
            var winloss = await yesterdayWinLossService.GetWinLoss(customer.CustId, customer.CustomerLevel, winLossDate);

            if (winloss != null)
            {
                var message = BuildMessage(customer.Language, winloss, customer.CustomerLevel, winLossDate.ToSimpleDate());

                return new Notification(customer.UserId, customer.BotId, message, customer.IsRealUser, job.IsUAT);
            }
        }
        catch (MinimizeUMException umEx)
        {
            await loggerService.Error(umEx, job, $"{umEx.Message}. CustId={customer.CustId}, CustLevel={customer.CustomerLevel}, DateTime={winLossDate}");
        }
        catch (Exception ex)
        {
            await loggerService.Error(ex, job, $"Could not get Yesterday Win Loss of CustId={customer.CustId}, CustLevel={customer.CustomerLevel}, DateTime={winLossDate}");
        }

        return null;
    }

    private static string BuildMessage(string language, WinLossBase winloss, byte custLevel, string winLossDate)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine(TemplateHelper.BuildTitle(language, "YesterdayWinLoss", winLossDate));

        var content = custLevel switch
        {
            CustomerLevel.Super => AddMasterWinLossContent((WinLossMaster)winloss),
            CustomerLevel.Master => AddAgentWinLossContent((WinLossAgent)winloss),
            CustomerLevel.Agent => AddMemberWinLossContent((WinLossMember)winloss),
            _ => throw new ArgumentOutOfRangeException(nameof(custLevel), $"Not expected {nameof(custLevel)} value: {custLevel}")
        };

        FormatMessageContent(messageBuilder, content, language);

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }


    private static Dictionary<string, decimal> AddMasterWinLossContent(WinLossMaster winloss)
        => new()
        {
            {nameof(winloss.Turnover),winloss.Turnover},
            {nameof(winloss.GrossComm),winloss.GrossComm},
            {nameof(winloss.MasterTotal),winloss.MasterTotal},
            {nameof(winloss.SuperWinLoss),winloss.SuperWinLoss},
            {nameof(winloss.SuperComm),winloss.SuperComm},
            {nameof(winloss.SuperTotal),winloss.SuperTotal},
            {nameof(winloss.CompanyWinLoss),winloss.CompanyWinLoss}
        };

    private static Dictionary<string, decimal> AddAgentWinLossContent(WinLossAgent winloss)
        => new()
        {
            {nameof(winloss.Turnover),winloss.Turnover},
            {nameof(winloss.GrossComm),winloss.GrossComm},
            {nameof(winloss.AgentTotal),winloss.AgentTotal},
            {nameof(winloss.MasterWinLoss),winloss.MasterWinLoss},
            {nameof(winloss.MasterComm),winloss.MasterComm},
            {nameof(winloss.MasterTotal),winloss.MasterTotal},
            {nameof(winloss.CompanyWinLoss),winloss.CompanyWinLoss}
        };

    private static Dictionary<string, decimal> AddMemberWinLossContent(WinLossMember winloss)
        => new()
        {
            {nameof(winloss.Turnover),winloss.Turnover},
            {nameof(winloss.GrossComm),winloss.GrossComm},
            {nameof(winloss.MemberWinLoss),winloss.MemberWinLoss},
            {nameof(winloss.MemberComm),winloss.MemberComm},
            {nameof(winloss.MemberTotal),winloss.MemberTotal},
            {nameof(winloss.AgentWinLoss),winloss.AgentWinLoss},
            {nameof(winloss.AgentComm),winloss.AgentComm},
            {nameof(winloss.AgentTotal),winloss.AgentTotal},
            {nameof(winloss.CompanyWinLoss),winloss.CompanyWinLoss}
        };

    private static void FormatMessageContent(StringBuilder messageBuilder, Dictionary<string, decimal> content, string language)
    {
        messageBuilder.AppendLine("<pre>");

        foreach (var item in content)
        {
            messageBuilder.AppendLine(TemplateHelper.FormatRow(item.Key, item.Value.FormatDecimal(), language));
        }

        messageBuilder.AppendLine("</pre>");
    }
}