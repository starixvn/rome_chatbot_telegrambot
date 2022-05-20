using System.Diagnostics;
using Microsoft.Extensions.Options;
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
    private readonly AppSettings appSettings;

    public PersonalMessageJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        IPersonalMessageService personalMessageService,
        ICustomerService customerService,
        IOptions<AppSettings> options)
        : base(maintenanceService, loggerService, systemSettingService)
    {
        this.personalMessageService = personalMessageService;
        this.customerService = customerService;
        appSettings = options.Value;
    }

    protected override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        Stopwatch sw = Stopwatch.StartNew();
        var notifications = Enumerable.Empty<ImportPersonalMessage>();

        await loggerService.Debug(job, "Start get Personal Messages");
        sw.Start();

        var (maxMsgTargetId, personalMessages) = await personalMessageService.GetNewMessages(
            systemSetting.MaxMsgTargetId,
            systemSetting.BatchSize,
            job.IsGetTestData);

        sw.Stop();
        await loggerService.Debug(job, $"End get Personal Messages in {sw.Elapsed.TotalMilliseconds} - Data count {personalMessages.Count()}");

        if (personalMessages.Any())
        {
            await loggerService.Debug(job, "Start get Linked Customers");
            sw.Reset();
            sw.Start();

            var custIds = personalMessages.Select(m => m.TargetId).Distinct().JoinWithComma();
            var linkedCustomers = await customerService.GetLinkedCustomers(
                appSettings.System,
                true,
                appSettings.IsBotTest,
                job.Name,
                custIds,
                job.Environment);

            sw.Stop();
            await loggerService.Debug(job, $"End get Linked Customers in {sw.Elapsed.TotalMilliseconds} - Data count {linkedCustomers.Count()}");

            if (linkedCustomers.Any())
            {
                await loggerService.Debug(job, "Start get Customer Status");
                sw.Reset();
                sw.Start();

                var customerStatusParams = linkedCustomers.Select(c => new CustomerStatusParam(c.CustId, c.SubAccId));
                var customerStatus = await customerService.GetCustomerStatus(customerStatusParams);

                sw.Stop();
                await loggerService.Debug(job, $"End get Customer Status in {sw.Elapsed.TotalMilliseconds} - Data count {customerStatus.Count()}");
                
                await loggerService.Debug(job, "Start join 3 list PersonalMessages, LinkedCustomers, CustomerStatus");
                sw.Reset();
                sw.Start();

                notifications = BuildImportPersonalMessages(
                    job,
                    personalMessages,
                    linkedCustomers,
                    customerStatus);
                
                sw.Stop();
                await loggerService.Debug(job, $"End join 3 list PersonalMessages, LinkedCustomers, CustomerStatus in {sw.Elapsed.TotalMilliseconds}");
            }
        }

        await loggerService.Debug(job, $"Start import");
        sw.Reset();
        sw.Start();

        var result = await personalMessageService.Import(notifications, job.Environment, maxMsgTargetId);

        sw.Stop();
        await loggerService.Debug(job, $"End import in {sw.Elapsed.TotalMilliseconds} - Data count {notifications.Count()}, MaxMsgTargetId = {maxMsgTargetId}");

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

        var notifications = new List<ImportPersonalMessage>();

        foreach (var item in query)
        {
            notifications.AddRange(BuildImportPersonalMessages(
                job,
                item.PersonalMessage,
                item.Customer));
        }

        return notifications;
    }

    private IEnumerable<ImportPersonalMessage> BuildImportPersonalMessages(
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
                yield return new ImportPersonalMessage(
                    customer.UserId,
                    message.MsgTargetId,
                    message.MessageId,
                    personalMessageService.BuildMessage(
                        customer.Language,
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