using System.Text;
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.MyWatchlist;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Resources;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.Jobs;

public sealed class MyWatchlistJob : NotificationBaseJob
{
    private readonly IMyWatchlistService myWatchlistService;
    private const byte SMAThresholds = 3;

    public MyWatchlistJob(
        IMyWatchlistService myWatchlistService,
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService)
        : base(maintenanceService, loggerService, systemSettingService, customerService)
    {
        this.myWatchlistService = myWatchlistService;
    }

    public override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        var notifications = new List<MyWatchlistNotification>();

        var (maxLogId, lastDataRow, serverName, monitoredPlayers) = await myWatchlistService.GetMonitoredPlayers(
            systemSetting.MaxLogId,
            systemSetting.BatchSize,
            systemSetting.LastDataRow,
            systemSetting.ServerName);

        if (monitoredPlayers.Any())
        {
            var custIds = monitoredPlayers.SelectMany(p => new[] { p.Recommend, p.MRecommend, p.SRecommend }).Distinct().JoinWithComma();
            var linkedCustomers = await customerService.GetLinkedCustomers(FunctionKey.RiskAlert, custIds, job.Environment);
            if (linkedCustomers.Any())
            {
                var customerStatus = await GetCustomerStatus(linkedCustomers);
                var validCustomers = from linkedCustomer in linkedCustomers
                                     join status in customerStatus
                                     on new { linkedCustomer.CustId, linkedCustomer.SubAccId } equals new { status.CustId, status.SubAccId }
                                     select linkedCustomer;

                var index = 1;
                foreach (var customer in validCustomers)
                {
                    notifications.AddRange(BuildNotifications(
                        customer,
                        monitoredPlayers.Where(p => p.Recommend == customer.CustId || p.MRecommend == customer.CustId || p.SRecommend == customer.CustId),
                        job.IsUAT,
                        ref index));
                }
            }
        }

        var result = await myWatchlistService.Import(notifications, job.Environment, lastDataRow, serverName, maxLogId);

        if (result.IsFailed)
        {
            await loggerService.Error(job, result.Message);
        }
    }

    private static IEnumerable<MyWatchlistNotification> BuildNotifications(
        LinkedCustomer customer,
        IEnumerable<MonitoredPlayer> monitoredPlayers,
        bool isUAT,
        ref int index)
    {
        var notifications = new List<MyWatchlistNotification>();
        foreach (var monitoredPlayer in monitoredPlayers)
        {
            if (monitoredPlayer.SettingType == SMAThresholds && customer.CustId == monitoredPlayer.CustId)
            {
                notifications.Add(new MyWatchlistNotification(
                    index,
                    customer.UserId,
                    customer.BotId,
                    BuildSMAThresholdMessage(customer.Language, monitoredPlayer),
                    customer.IsRealUser,
                    isUAT,
                    new List<long> { monitoredPlayer.LogId }));
            }
            else
            {
                if (monitoredPlayer.Recommend == customer.CustId)
                {
                    notifications.Add(new MyWatchlistNotification(
                        index,
                        customer.UserId,
                        customer.BotId,
                        BuildMessage(customer.Language, monitoredPlayer),
                        customer.IsRealUser,
                        isUAT,
                        new List<long> { monitoredPlayer.LogId }));
                }
            }

            index++;
        }

        return notifications;
    }

    private static string BuildMessage(string language, MonitoredPlayer monitoredPlayer)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine(TemplateHelper.BuildTitleWithEmoji(language, nameof(Template.MyWatchlistTitle), EmojiUnicode.Warning));
        messageBuilder.Append("<pre>");
        if (monitoredPlayer.AccumWL >= 0)
        {
            messageBuilder.AppendFormat(
                TemplateHelper.Get(nameof(Template.MyWatchlistWinContent), language),
                monitoredPlayer.Username,
                monitoredPlayer.AccumWL.FormatDecimal(),
                monitoredPlayer.WinningThreshold.FormatDecimal());
        }
        else
        {
            messageBuilder.AppendFormat(
                TemplateHelper.Get(nameof(Template.MyWatchlistLossContent), language),
                monitoredPlayer.Username,
                monitoredPlayer.AccumWL.FormatDecimal(),
                monitoredPlayer.LosingThreshold.FormatDecimal());
        }

        messageBuilder.Append("</pre>");

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }

    private static string BuildSMAThresholdMessage(string language, MonitoredPlayer monitoredPlayer)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine(TemplateHelper.BuildTitleWithEmoji(language, nameof(Template.MyWatchlistTitle), EmojiUnicode.Warning));
        messageBuilder.Append("<pre>");
        if (monitoredPlayer.AccumWL >= 0)
        {
            messageBuilder.AppendFormat(
                TemplateHelper.Get(nameof(Template.SMAThresholdAlertWinContent), language),
                monitoredPlayer.AccumWL.FormatDecimal(),
                monitoredPlayer.WinningThreshold.FormatDecimal());
        }
        else
        {
            messageBuilder.AppendFormat(
                TemplateHelper.Get(nameof(Template.SMAThresholdAlertLossContent), language),
                monitoredPlayer.AccumWL.FormatDecimal(),
                monitoredPlayer.LosingThreshold.FormatDecimal());
        }

        messageBuilder.Append("</pre>");

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }
}