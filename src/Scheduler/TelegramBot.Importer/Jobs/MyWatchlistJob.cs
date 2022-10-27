using System.Text;
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.MyWatchlist;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Resources;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.Jobs;

public sealed class MyWatchlistJob : NotificationBaseJob
{
    private const int ChunkSize = 28;
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

        var (maxLogId, lastDataRow, serverName, monitoredPlayers) = await myWatchlistService.GetMonitoredPlayers(
            systemSetting.MaxLogId,
            systemSetting.BatchSize,
            systemSetting.LastDataRow,
            systemSetting.ServerName);

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

                var currentTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");

                var index = 1;
                foreach (var group in groups)
                {
                    notifications.AddRange(BuildNotifications(group.Key, group.Select(g => g.monitoredPlayer), currentTime, job.IsUAT, ref index));
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
        string currentTime,
        bool isUAT,
        ref int index)
    {
        var notifications = new List<MyWatchlistNotification>();
        var chunks = monitoredPlayers.Chunk(ChunkSize);

        foreach (var chunk in chunks)
        {
            notifications.Add(new MyWatchlistNotification(
                index,
                customer.UserId,
                customer.BotId,
                BuildMessage(customer.Language, currentTime, chunk),
                customer.IsRealUser,
                isUAT,
                chunk.Select(p => (long)p.LogId)));
            index++;
        }

        return notifications;
    }

    private static string BuildMessage(string language, string currentDate, MonitoredPlayer[] monitoredPlayers)
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