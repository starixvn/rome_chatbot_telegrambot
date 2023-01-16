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
            notifications.Add(new MyWatchlistNotification(
                            index,
                            customer.UserId,
                            customer.BotId,
                            monitoredPlayer.SettingType == SMAThresholds
                                ? BuildSMAThresholdMessage(customer.Language, monitoredPlayer)
                                : BuildMessage(customer.Language, monitoredPlayer),
                            customer.IsRealUser,
                            isUAT,
                            new List<long> { monitoredPlayer.LogId }));
            index++;
        }

        return notifications;
    }

    private static string BuildMessage(string language, MonitoredPlayer monitoredPlayer)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine($"[{TemplateHelper.Get(nameof(Template.MyWatchlistTitle), language)}]");

        messageBuilder.AppendLine("<pre>");
        messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.Username), monitoredPlayer.Username, language));
        messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.CurrentWinLoss), monitoredPlayer.AccumWL.FormatDecimal(), language));
        messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.WinningThreshold), monitoredPlayer.WinningThreshold.FormatDecimal(), language));
        messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.LosingThreshold), monitoredPlayer.LosingThreshold.FormatDecimal(), language));
        messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.ThresholdType), TemplateHelper.Get($"ThresholdType_{monitoredPlayer.SettingType}", language), language));
        messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.ReachedTime), monitoredPlayer.CreatedDate.ToString("MM-dd-yyyy HH:mm"), language));
        messageBuilder.AppendLine("</pre>");

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }

    private static string BuildSMAThresholdMessage(string language, MonitoredPlayer monitoredPlayer)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine($"[{TemplateHelper.Get(nameof(Template.SMAThresholdAlertTitle), language)}]");

        messageBuilder.AppendLine("<pre>");
        messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.CurrentWinLoss), monitoredPlayer.AccumWL.FormatDecimal(), language));
        messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.WinningThreshold), monitoredPlayer.WinningThreshold.FormatDecimal(), language));
        messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.LosingThreshold), monitoredPlayer.LosingThreshold.FormatDecimal(), language));
        messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.ReachedTime), monitoredPlayer.CreatedDate.ToString("MM-dd-yyyy HH:mm"), language));
        messageBuilder.AppendLine("</pre>");

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }
}