using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.PersonalMessages;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.Jobs;

public sealed class PersonalMessageJob : NotificationBaseJob
{
    private readonly IPersonalMessageService personalMessageService;

    public PersonalMessageJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService,
        IPersonalMessageService personalMessageService)
        : base(maintenanceService, loggerService, systemSettingService, customerService)
        => this.personalMessageService = personalMessageService;

    protected override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        var notifications = Enumerable.Empty<PersonalMessageNotification>();

        var (maxMsgTargetId, personalMessages) = await personalMessageService.GetNewMessages(
            systemSetting.MaxMsgTargetId,
            systemSetting.BatchSize,
            job.IsGetTestData);

        if (personalMessages.Any())
        {
            var custIds = personalMessages.Select(m => m.TargetId).Distinct().JoinWithComma();
            var linkedCustomers = await GetLinkedCustomers(custIds, job);

            if (linkedCustomers.Any())
            {
                var customerStatus = await GetCustomerStatus(linkedCustomers);

                notifications = BuildPersonalMessageNotifications(
                    job,
                    personalMessages,
                    linkedCustomers,
                    customerStatus);
            }
        }

        // In case of NoData, call Import to update JobStatus=Completed and LastRuntime.
        var result = await personalMessageService.Import(notifications, job.Environment, maxMsgTargetId);

        if (result.IsFailed)
        {
            await loggerService.Error(job, result.Message);
        }
    }

    private IEnumerable<PersonalMessageNotification> BuildPersonalMessageNotifications(
        ScheduleJob job,
        IEnumerable<PersonalMessage> personalMessages,
        IEnumerable<LinkedCustomer> linkedCustomers,
        IEnumerable<CustomerStatus> customerStatus)
    {
        var query =
            from message in personalMessages
            join customer in linkedCustomers on message.TargetId equals customer.CustId
            join status in customerStatus on new { customer.CustId, customer.SubAccId }
                equals new { status.CustId, status.SubAccId }
            where status.IsValid(job.SubAccPermission)
            select new
            {
                PersonalMessage = message,
                Customer = customer
            };

        var currentDate = DateTime.Today.ToSimpleDate();
        var notifications = new List<PersonalMessageNotification>();

        foreach (var item in query)
        {
            notifications.AddRange(BuildPersonalMessageNotifications(
                job,
                item.PersonalMessage,
                item.Customer,
                currentDate));
        }

        return notifications;
    }

    private IEnumerable<PersonalMessageNotification> BuildPersonalMessageNotifications(
        ScheduleJob job,
        PersonalMessage message,
        LinkedCustomer customer,
        string currentDate)
    {
        var isReplaceBetTransId = message.ContentEN.Contains("@@")
            && !string.IsNullOrWhiteSpace(message.BetTransId);

        if (isReplaceBetTransId)
        {
            var betTransIds = SplitBetTransId(message.BetTransId);

            foreach (var transId in betTransIds)
            {
                yield return new PersonalMessageNotification(
                    customer.UserId,
                    customer.BotId,
                    message.MsgTargetId,
                    message.MessageId,
                    personalMessageService.BuildMessage(
                        customer.Language,
                        currentDate,
                        message.GetContentByLanguage(customer.Language),
                        transId),
                    customer.IsRealUser,
                    job.IsUAT);
            }
        }
        else
        {
            yield return new PersonalMessageNotification(
                customer.UserId,
                customer.BotId,
                message.MsgTargetId,
                message.MessageId,
                personalMessageService.BuildMessage(
                    customer.Language,
                    currentDate,
                    message.GetContentByLanguage(customer.Language)),
                customer.IsRealUser,
                job.IsUAT);
        }
    }

    private static IEnumerable<string> SplitBetTransId(string betTransId)
    {
        const int MaxBetTransIdToChunk = 154;
        var betTransIdArray = betTransId.SplitComma();

        if (betTransIdArray.Length <= MaxBetTransIdToChunk)
        {
            return new[] { betTransId };
        }

        var chunkBetTransIds = betTransIdArray.Chunk(MaxBetTransIdToChunk);

        return chunkBetTransIds.Select(array => array.JoinWithComma());
    }
}