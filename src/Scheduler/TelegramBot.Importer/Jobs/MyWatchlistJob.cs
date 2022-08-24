using System.Text;
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models.MyWatchlist;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Resources;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.Jobs;

public sealed class MyWatchlistJob : NotificationBaseJob
{
    private readonly IMyWatchlistService myWatchlistService;

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

        var (maxLogId, lastDataRow, serverName, monitoredPlayers) = await myWatchlistService.GetMonitoredPlayers(systemSetting.MaxLogId, systemSetting.BatchSize);

        if (monitoredPlayers.Any())
        {
            var custIds = monitoredPlayers.Select(p => p.Recommend).Distinct().JoinWithComma();
            var linkedCustomers = await GetLinkedCustomers(custIds, job);
            if (linkedCustomers.Any())
            {
                var customerStatus = await GetCustomerStatus(linkedCustomers);
                var groups = (from monitoredPlayer in monitoredPlayers
                              join linkedCustomer in linkedCustomers on monitoredPlayer.Recommend equals linkedCustomer.CustId
                              join status in customerStatus on new { linkedCustomer.CustId, linkedCustomer.SubAccId } equals new { status.CustId, status.SubAccId }
                              where status.IsValid(job.SubAccPermission)
                              select (linkedCustomer, monitoredPlayer))
                             .GroupBy(g => g.linkedCustomer);

                var currentDate = DateTime.Now.ToString("yyyymmdd HH:mm:ss");

                var index = 1;
                foreach (var group in groups)
                {
                    var customer = group.Key;
                    var players = group.Select(g => g.monitoredPlayer);
                    notifications.Add(new MyWatchlistNotification(
                        index,
                        customer.UserId,
                        customer.BotId,
                        BuildMessage(customer.Language, currentDate, players),
                        customer.IsRealUser,
                        job.IsUAT,
                        players.Select(p => (long)p.LogId)));

                    index++;
                }
            }
        }

        var result = await myWatchlistService.Import(notifications, job.Environment, lastDataRow, serverName, maxLogId);

        if (result.IsFailed)
        {
            await loggerService.Error(job, result.Message);
        }
    }

    private static string BuildMessage(string language, string currentDate, IEnumerable<MonitoredPlayer> monitoredPlayers)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine(TemplateHelper.BuildTitle(language, "MyWatchlistTitle", currentDate))
            .AppendLine(TemplateHelper.Get("MyWatchlistMessage", language))
            .AppendLine($"<b>{TemplateHelper.Get("MyWatchlistHeader", language)}</b>");

        foreach (var monitoredPlayer in monitoredPlayers)
        {
            messageBuilder.AppendLine(string.Format(
                "{0} | {1} | {2} | {3} | {4} | {5}",
                monitoredPlayer.Username,
                monitoredPlayer.AccumWL.FormatDecimal(),
                monitoredPlayer.WinningThreshold.FormatDecimal(),
                monitoredPlayer.LosingThreshold.FormatDecimal(),
                monitoredPlayer.CreatedDate.ToString("dd-MM-yyyy HH:mm:ss"),
                TemplateHelper.Get($"ThresholdType_{monitoredPlayer.SettingType}", language)));
        }

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }
}