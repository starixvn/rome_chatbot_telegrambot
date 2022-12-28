using System.Text;
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.MemberWinLossLimits;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Resources;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.Jobs;

public sealed class MemberWinLossLimitJob : NotificationBaseJob
{
    private readonly IMemberWinLossLimitService memberWinLostLimitService;
    private readonly ICacheRefService cacheRefService;
    private const int ChunkSize = 20;

    public MemberWinLossLimitJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService,
        IMemberWinLossLimitService memberWinLostLimitService,
        ICacheRefService cacheRefService)
        : base(maintenanceService, loggerService, systemSettingService, customerService)
    {
        this.memberWinLostLimitService = memberWinLostLimitService;
        this.cacheRefService = cacheRefService;
    }

    public override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        var notifications = Enumerable.Empty<MemberWinLossLimitNotification>();

        var (maxLogId, memberWinLossLimits) = await memberWinLostLimitService.GetMemberWinLossLimits(
            systemSetting.BatchSize,
            systemSetting.MaxLogIdLong
        );

        if (memberWinLossLimits.Any())
        {
            var custIds = memberWinLossLimits.SelectMany(m => new[] { m.ACustId, m.MCustId, m.SCustId }).Distinct().ToList();
            var strCustIds = custIds.JoinWithComma();
            var linkedCustomers = await customerService.GetLinkedCustomers(FunctionKey.RiskAlert, strCustIds, job.Environment);

            if (linkedCustomers.Any())
            {
                var customerStatus = await GetCustomerStatus(linkedCustomers);

                notifications = BuildMemberWinLossLimitNotifications(
                    job,
                    memberWinLossLimits,
                    linkedCustomers,
                    customerStatus);
            }
        }

        var result = await memberWinLostLimitService.Import(notifications, job.Environment, maxLogId);

        if (result.IsFailed)
        {
            await loggerService.Error(job, result.Message);
        }
    }

    private IEnumerable<MemberWinLossLimitNotification> BuildMemberWinLossLimitNotifications(
        ScheduleJob job,
        IEnumerable<MemberWinLossLimit> memberWinLossLimits,
        IEnumerable<LinkedCustomer> linkedCustomers,
        IEnumerable<CustomerStatus> customerStatus)
    {
        var notifications = new List<MemberWinLossLimitNotification>();

        var verifiedCustomers = from customer in linkedCustomers
                                join status in customerStatus
                                   on new { customer.CustId, customer.SubAccId } equals new { status.CustId, status.SubAccId }
                                where status.IsValid(job.SubAccPermission)
                                select customer;

        foreach (var customer in verifiedCustomers)
        {
            notifications.AddRange(BuildMemberWinLossLimitNotification(job, memberWinLossLimits, customer));
        }

        return notifications;
    }

    private IEnumerable<MemberWinLossLimitNotification> BuildMemberWinLossLimitNotification(
        ScheduleJob job,
        IEnumerable<MemberWinLossLimit> memberWinLossLimits,
        LinkedCustomer customer)
    {
        var membersWithLinkedUser = memberWinLossLimits
                                    .Where(m => m.ACustId == customer.CustId || m.MCustId == customer.CustId || m.SCustId == customer.CustId);
        var winLossLimitMessages = membersWithLinkedUser
                                    .GroupBy(
                                        m => m.CustId,
                                        (_, m) =>
                                        {
                                            var member = m.First();

                                            return new WinLossLimitMessage(
                                                member.Username,
                                                member.AUsername,
                                                member.MUsername,
                                                member.SUsername,
                                                m.Select(p => new ProductWinLossLimitMessage(
                                                            cacheRefService.GetProductName(p.ProductId, customer.Language),
                                                            p.WinLost,
                                                            p.WinLossLimit,
                                                            p.WinLossPercent)
                                                        ).ToList());
                                        }
                                    );
        var logIds = membersWithLinkedUser.Select(m => m.LogId);

        foreach (var winLossLimitMessage in winLossLimitMessages)
        {
            var usernames = BuildUsernames(winLossLimitMessage, customer.CustomerLevel);
            var products = winLossLimitMessage.Products.OrderBy(p => p.ProductName).ThenByDescending(p => p.CurrentWinLoss > 0);
            var chunks = products.Chunk(ChunkSize);
            foreach (var chunk in chunks)
            {
                yield return new MemberWinLossLimitNotification(
                    customer.UserId,
                    customer.BotId,
                    BuildMessage(chunk, customer.Language, usernames),
                    customer.IsRealUser,
                    job.IsUAT,
                    logIds
                );
            }

        }
    }

    private static string BuildMessage(ProductWinLossLimitMessage[] products, string language, string usernames)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine($"[{TemplateHelper.Get("MemberWinLossLimitAlert", language)}]")
            .AppendLine()
            .AppendLine($"<b>{usernames}</b>");

        messageBuilder.Append("<pre>");
        BuildProducts(messageBuilder, products, language);
        messageBuilder.AppendLine("</pre>");

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }

    private static string BuildUsernames(WinLossLimitMessage memberMessage, byte custLevel)
    {
        var usernames = custLevel switch
        {
            CustomerLevel.Agent => memberMessage.Username,
            CustomerLevel.Master => $"{memberMessage.AUsername} | {memberMessage.Username}",
            CustomerLevel.Super => $"{memberMessage.MUsername} | {memberMessage.AUsername} | {memberMessage.Username}",
            _ => throw new ArgumentOutOfRangeException(
                nameof(custLevel),
                $"Not expected {nameof(custLevel)} value: {custLevel}")
        };

        return usernames;
    }

    private static void BuildProducts(StringBuilder messageBuilder, IEnumerable<ProductWinLossLimitMessage> products, string language)
    {
        foreach (var product in products)
        {
            messageBuilder.AppendLine();
            messageBuilder.AppendLine(product.ProductName);
            messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.CurrentWinLoss), product.CurrentWinLoss.FormatDecimal(), language));
            var X = product.CurrentWinLoss > 0 ? "X" : string.Empty;
            messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.MaximumWinLoss), $"{product.MaximumWinLoss.FormatDecimalToInt()}{X}", language));
            messageBuilder.AppendLine(TemplateHelper.FormatRow(nameof(Template.CurrentWinLossPercent), $"{product.CurrentWinLossPercentage.FormatDecimalUptoTwoDigit()}%", language));
        }
    }
}
