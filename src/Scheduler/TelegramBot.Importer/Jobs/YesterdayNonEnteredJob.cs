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

namespace TelegramBot.Importer.Jobs;

public sealed class YesterdayNonEnteredJob : NotificationBaseJob
{
    private readonly IYesterdayWinLossService yesterdayWinLossService;

    public YesterdayNonEnteredJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService,
        IYesterdayWinLossService yesterdayWinLossService)
        : base(maintenanceService, loggerService, systemSettingService, customerService)
    {
        this.yesterdayWinLossService = yesterdayWinLossService;
    }

    public override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        bool continueGettingData;
        var isFirstBatch = true;
        const int delayTime = 500;
        var winLossDate = DateTime.Today.Yesterday();

        try
        {
            var enteredResult = await yesterdayWinLossService.GetEnteredResult();

            do
            {
                // Must update JobStatus=Running in 2nd, 3rd, ... batch.
                if (!isFirstBatch)
                {
                    await UpdateJobStatusToRunning(job);
                }

                continueGettingData = await ImportYesterdayNonEnteredNotifications(job, winLossDate, enteredResult);
                isFirstBatch = false;

                await Task.Delay(delayTime);
            }
            while (continueGettingData);
        }
        catch (MinimizeUMException umEx)
        {
            await loggerService.Error(umEx, job);
        }
    }

    internal async Task<bool> ImportYesterdayNonEnteredNotifications(ScheduleJob job, DateTime winLossDate, EnteredResult enteredResult)
    {
        var (linkedCustomers, maxUserId) = await customerService.GetYesterdayWinLossLinkedCustomers(winLossDate, job.Name, job.Environment);
        var hasLinkedCustomer = linkedCustomers.Any();

        try
        {
            var notifications = hasLinkedCustomer
                ? await BuildYesterdayNonEnteredNotifications(job, linkedCustomers, enteredResult)
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

    private async Task<IEnumerable<Notification>> BuildYesterdayNonEnteredNotifications(
        ScheduleJob job,
        IEnumerable<LinkedCustomer> linkedCustomers,
        EnteredResult enteredResult)
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

        foreach (var customer in query)
        {
            var notification = BuildYesterdayNonEnteredNotification(job, customer, enteredResult);

            if (notification != null)
            {
                notifications.Add(notification);
            }
        }

        return notifications;
    }

    private static Notification BuildYesterdayNonEnteredNotification(
        ScheduleJob job,
        LinkedCustomer customer,
        EnteredResult enteredResult)
    {
        var message = BuildMessage(customer.Language, enteredResult);

        return new Notification(customer.UserId, customer.BotId, message, customer.IsRealUser, job.IsUAT);
    }

    private static string BuildMessage(string language, EnteredResult enteredResult)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine(TemplateHelper.BuildTitleWithEmoji(language, nameof(Template.NonEnteredMatchNotification), EmojiUnicode.Envelope));

        messageBuilder.Append("<pre>");
        messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.NonEnteredMatches), enteredResult.NonEntered.ToString(), language));
        messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.EnteredMatches), enteredResult.Entered.ToString(), language));
        if (enteredResult.NonEntered == 0)
        {
            messageBuilder.Append(TemplateHelper.Get(nameof(Template.NonEnteredMatchNote), language));
        }
        messageBuilder.Append("</pre>");

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }
}