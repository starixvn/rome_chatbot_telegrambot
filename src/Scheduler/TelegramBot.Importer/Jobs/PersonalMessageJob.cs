using System.Text;
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.PersonalMessages;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Resources;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.Jobs;

public sealed class PersonalMessageJob : NotificationBaseJob
{
    private const byte DomainsMsgContentsType = 110;
    private readonly IPersonalMessageService personalMessageService;

    public PersonalMessageJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService,
        IPersonalMessageService personalMessageService)
        : base(maintenanceService, loggerService, systemSettingService, customerService)
        => this.personalMessageService = personalMessageService;

    public override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        var notifications = new List<PersonalMessageNotification>();

        var (maxMsgTargetId, personalMessages) = await personalMessageService.GetNewMessages(
            systemSetting.MaxMsgTargetId,
            systemSetting.BatchSize,
            job.IsGetTestData);

        if (personalMessages.Any())
        {
            var personalMessageByContentType = SplitPersonalMessages(personalMessages);
            var linkedCustomers = await GetLinkedCustomers(
                personalMessageByContentType.CustIds,
                job,
                personalMessageByContentType.Sites);

            if (linkedCustomers.Any())
            {
                var customerStatus = await GetCustomerStatus(linkedCustomers);

                if (customerStatus.Any())
                {
                    notifications.AddRange(BuildNormalMessages(job, personalMessageByContentType.NormalMessages, linkedCustomers, customerStatus));
                    notifications.AddRange(BuildDomainMessages(job, personalMessageByContentType.DomainMessages, linkedCustomers, customerStatus));
                }
            }
        }

        // In case of NoData, call Import to update JobStatus=Completed and LastRuntime.
        var result = await personalMessageService.Import(notifications, job.Environment, maxMsgTargetId);

        if (result.IsFailed)
        {
            await loggerService.Error(job, result.Message);
        }
    }

    private static PersonalMessageByContentType SplitPersonalMessages(IEnumerable<PersonalMessage> personalMessages)
    {
        var normalMessages = new List<PersonalMessage>();
        var custIdsBuilder = new StringBuilder();
        var domainMessages = new List<PersonalMessage>();
        var sitesBuilder = new StringBuilder();

        foreach (var item in personalMessages)
        {
            if (item.MsgContentsType == DomainsMsgContentsType)
            {
                domainMessages.Add(item);
                sitesBuilder.Append(item.Remark).Append(',');
            }
            else
            {
                normalMessages.Add(item);
                custIdsBuilder.Append(item.TargetId).Append(',');
            }
        }

        return new PersonalMessageByContentType(
            normalMessages,
            custIdsBuilder.ToString(),
            domainMessages,
            sitesBuilder.ToString());
    }

    private static IEnumerable<PersonalMessageNotification> BuildNormalMessages(
        ScheduleJob job,
        IEnumerable<PersonalMessage> normalMessages,
        IEnumerable<LinkedCustomer> linkedCustomers,
        IEnumerable<CustomerStatus> customerStatus)
    {
        var notifications = new List<PersonalMessageNotification>();

        if (normalMessages.Any())
        {
            var query =
                from message in normalMessages
                join customer in linkedCustomers on message.TargetId equals customer.CustId
                join status in customerStatus on new { customer.CustId, customer.SubAccId }
                    equals new { status.CustId, status.SubAccId }
                where status.IsValid(job.SubAccPermission)
                select new
                {
                    PersonalMessage = message,
                    Customer = customer
                };

            foreach (var item in query)
            {
                notifications.AddRange(BuildPersonalMessageNotifications(job, item.PersonalMessage, item.Customer));
            }
        }

        return notifications;
    }

    private static IEnumerable<PersonalMessageNotification> BuildPersonalMessageNotifications(
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
                yield return new PersonalMessageNotification(
                    customer.UserId,
                    customer.BotId,
                    message.MsgTargetId,
                    message.MessageId,
                    BuildNormalMessage(customer.Language, message.GetContentByLanguage(customer.Language), transId),
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
                BuildNormalMessage(customer.Language, message.GetContentByLanguage(customer.Language)),
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

    private static string BuildNormalMessage(string language, string content, string transId = "")
    {
        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine(TemplateHelper.BuildTitleWithEmoji(language, "PersonalMessage", EmojiUnicode.Envelope))
            .Append(transId.Length == 0 ? content : content.Replace("@@", transId));

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }

    private static IEnumerable<PersonalMessageNotification> BuildDomainMessages(
        ScheduleJob job,
        IEnumerable<PersonalMessage> domainMessages,
        IEnumerable<LinkedCustomer> linkedCustomers,
        IEnumerable<CustomerStatus> customerStatus)
    {
        if (domainMessages.Any())
        {
            var query =
                from message in domainMessages
                join customer in linkedCustomers on message.Remark equals customer.Site
                join status in customerStatus on new { customer.CustId, customer.SubAccId }
                    equals new { status.CustId, status.SubAccId }
                where status.IsValid(job.SubAccPermission)
                select new
                {
                    PersonalMessage = message,
                    Customer = customer
                };

            foreach (var item in query)
            {
                yield return new PersonalMessageNotification(
                    item.Customer.UserId,
                    item.Customer.BotId,
                    item.PersonalMessage.MsgTargetId,
                    item.PersonalMessage.MessageId,
                    BuildDomainMessage(item.Customer.Language, item.PersonalMessage.GetContentByLanguage(item.Customer.Language)),
                    item.Customer.IsRealUser,
                    job.IsUAT);
            }
        }
    }

    private static string BuildDomainMessage(string language, string content)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine(TemplateHelper.BuildTitleWithEmoji(language, nameof(Template.NormalAnnouncement), EmojiUnicode.Envelope))
            .Append(content);

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }
}