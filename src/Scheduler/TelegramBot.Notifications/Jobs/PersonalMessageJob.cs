using Alpha.Notifications.Messages.Models;
using Microsoft.Extensions.Options;
using ONEbook.UM.Services;
using Quartz;
using TelegramBot.Notifications.Messages.Services;
using TelegramBot.Notifications.Shared.Configuration;
using TelegramBot.Notifications.Shared.Constants;
using TelegramBot.Notifications.Shared.Extensions;
using TelegramBot.Notifications.Shared.Models;
using TelegramBot.Notifications.Shared.Params;
using TelegramBot.Notifications.Shared.Services;

namespace TelegramBot.Notifications.Jobs;

[DisallowConcurrentExecution]
public sealed class PersonalMessageJob : NotificationBaseJob
{
    private readonly IMessageService messageService;
    private readonly ICustomerService customerService;
    private readonly INotificationService notificationService;

    public PersonalMessageJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        IOptions<AppSettings> options,
        IMessageService messageService,
        ICustomerService customerService,
        INotificationService notificationService)
        : base(maintenanceService, loggerService, systemSettingService, options)
    {
        this.messageService = messageService;
        this.customerService = customerService;
        this.notificationService = notificationService;
    }

    protected override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        var notifications = Enumerable.Empty<ImportPersonalMessageParam>();

        await loggerService.Debug(job, "Start get Personal Messages");

        var (maxMsgTargetId, personalMessages) = await messageService.GetPersonalMessages(
            systemSetting.MaxMsgTargetId,
            systemSetting.BatchSize,
            job.IsGetTestData);

        await loggerService.Debug(job, $"End get Personal Messages - Data count {personalMessages.Count()}");

        if (personalMessages.Any())
        {
            await loggerService.Debug(job, "Start get Linked Customers");

            var custIds = personalMessages.Select(m => m.TargetId).Distinct().JoinWithComma();
            var linkedCustomers = await customerService.GetLinkedCustomers(
                appSettings.System,
                true,
                appSettings.IsBotTest,
                job.Name,
                custIds,
                job.Environment);

            await loggerService.Debug(job, $"End get Linked Customers - Data count {linkedCustomers.Count()}");

            if (linkedCustomers.Any())
            {
                await loggerService.Debug(job, "Start get Customer Status");

                var customerStatusParams = linkedCustomers.Select(c => new CustomerStatusParam(c.CustId, c.SubAccId));
                var customerStatus = await customerService.GetCustomerStatus(customerStatusParams);

                await loggerService.Debug(job, $"End get Customer Status - Data count {customerStatus.Count()}");

                await loggerService.Debug(job, "Start join 3 list PersonalMessages, LinkedCustomers, CustomerStatus");

                notifications = BuildImportPersonalMessageParams(
                    job,
                    personalMessages,
                    linkedCustomers,
                    customerStatus);

                await loggerService.Debug(job, "End join 3 list PersonalMessages, LinkedCustomers, CustomerStatus");
            }
        }

        await loggerService.Debug(job, $"Start import");
        
        var result = await notificationService.Import(notifications, job.Environment, maxMsgTargetId);

        await loggerService.Debug(job, $"End import - Data count {notifications.Count()}, MaxMsgTargetId = {maxMsgTargetId}");
        
        if (result.IsFail)
        {
            await loggerService.Error(job, result.Message);
        }
    }

    private static IEnumerable<ImportPersonalMessageParam> BuildImportPersonalMessageParams(
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

        var notifications = new List<ImportPersonalMessageParam>();

        foreach (var item in query)
        {
            notifications.AddRange(BuildImportPersonalMessageParam(
                job,
                item.PersonalMessage,
                item.Customer));
        }

        return notifications;
    }

    private static IEnumerable<ImportPersonalMessageParam> BuildImportPersonalMessageParam(
        ScheduleJob job,
        PersonalMessage message,
        LinkedCustomer customer)
    {
        var isReplaceBetTransId = message.ContentEN.Contains("@@")
            && !string.IsNullOrWhiteSpace(message.BetTransId);

        if (isReplaceBetTransId)
        {
            var betTransIds = SplitBetTransId(message.BetTransId);

            foreach (var transId in betTransIds)
            {
                yield return new ImportPersonalMessageParam(
                    customer.UserId,
                    message.MsgTargetId,
                    message.MessageId,
                    MessageTemplate.BuildPersonalMessage(
                        customer.Language,
                        message.GetContentByLanguage(customer.Language),
                        transId),
                    customer.IsRealUser && job.IsUAT ? NotificationStatus.Draft : NotificationStatus.Initial);
            }
        }
        else
        {
            yield return new ImportPersonalMessageParam(
                customer.UserId,
                message.MsgTargetId,
                message.MessageId,
                MessageTemplate.BuildPersonalMessage(
                    customer.Language,
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