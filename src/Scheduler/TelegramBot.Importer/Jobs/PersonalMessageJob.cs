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
public sealed class PersonalMessageJob : NotificationBaseJob
{
    private readonly IPersonalMessageService personalMessageService;
    private readonly ICustomerService customerService;

    public PersonalMessageJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        IPersonalMessageService personalMessageService,
        ICustomerService customerService)
        : base(maintenanceService, loggerService, systemSettingService)
    {
        this.personalMessageService = personalMessageService;
        this.customerService = customerService;
    }

    protected override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        var notifications = Enumerable.Empty<ImportPersonalMessage>();

        var (maxMsgTargetId, personalMessages) = await personalMessageService.GetNewMessages(
            systemSetting.MaxMsgTargetId,
            systemSetting.BatchSize,
            job.IsGetTestData);

        if (personalMessages.Any())
        {
            var custIds = personalMessages.Select(m => m.TargetId).Distinct().JoinWithComma();
            var linkedCustomers = await customerService.GetLinkedCustomers(
                job.Name,
                custIds,
                job.Environment);

            if (linkedCustomers.Any())
            {
                var customerStatusParams = linkedCustomers.Select(c => new CustomerStatusParam(c.CustId, c.SubAccId));
                var customerStatus = await customerService.GetCustomerStatus(customerStatusParams);

                notifications = BuildImportPersonalMessages(
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

    private IEnumerable<ImportPersonalMessage> BuildImportPersonalMessages(
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
        var notifications = new List<ImportPersonalMessage>();

        foreach (var item in query)
        {
            notifications.AddRange(BuildImportPersonalMessages(
                job,
                item.PersonalMessage,
                item.Customer,
                currentDate));
        }

        return notifications;
    }

    private IEnumerable<ImportPersonalMessage> BuildImportPersonalMessages(
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
                yield return new ImportPersonalMessage(
                    customer.UserId,
                    message.MsgTargetId,
                    message.MessageId,
                    personalMessageService.BuildMessage(
                        customer.Language,
                        currentDate,
                        message.GetContentByLanguage(customer.Language),
                        transId),
                    customer.IsRealUser && job.IsUAT ? NotificationStatus.Draft : NotificationStatus.Initial);
            }
        }
        else
        {
            yield return new ImportPersonalMessage(
                customer.UserId,
                message.MsgTargetId,
                message.MessageId,
                personalMessageService.BuildMessage(
                    customer.Language,
                    currentDate,
                    message.GetContentByLanguage(customer.Language)),
                customer.IsRealUser && job.IsUAT ? NotificationStatus.Draft : NotificationStatus.Initial);
        }
    }

    private static IEnumerable<string> SplitBetTransId(string betTransId)
    {
        const int MaxBetTransIdToChunk = 220;
        var betTransIdArray = betTransId.SplitComma();

        if (betTransIdArray.Length <= MaxBetTransIdToChunk)
        {
            return new[] { betTransId };
        }

        var chunkBetTransIds = betTransIdArray.Chunk(MaxBetTransIdToChunk);

        return chunkBetTransIds.Select(array => array.JoinWithComma());
    }
}