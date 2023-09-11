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
        var verifiedCustomers = from customer in linkedCustomers
                                join status in customerStatus
                                   on new { customer.CustId, customer.SubAccId } equals new { status.CustId, status.SubAccId }
                                where status.IsValid(job.SubAccPermission)
                                select customer;

        foreach (var customer in verifiedCustomers)
        {
            var membersWithLinkedUser = memberWinLossLimits
                .Where(m => m.ACustId == customer.CustId || m.MCustId == customer.CustId || m.SCustId == customer.CustId);

            foreach (var winLossLimitMessage in membersWithLinkedUser)
            {
                yield return new MemberWinLossLimitNotification(
                        customer.UserId,
                        customer.BotId,
                        BuildMessage(winLossLimitMessage, customer.Language),
                        customer.IsRealUser,
                        job.IsUAT,
                        winLossLimitMessage.LogId);
            }
        }
    }

    private string BuildMessage(MemberWinLossLimit memberWinLossLimit, string language)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine(string.Format(
                TemplateHelper.BuildTitleWithEmoji(language, nameof(Template.MemberWinLossLimitAlert), EmojiUnicode.Warning),
                cacheRefService.GetProductName(memberWinLossLimit.ProductId, language)))
            .AppendLine()
            .Append("<pre>");
        if (memberWinLossLimit.WinLost >= 0)
        {
            messageBuilder.AppendFormat(
                TemplateHelper.Get(nameof(Template.MemberWinLossLimitAlertWinContent), language),
                memberWinLossLimit.Username,
                memberWinLossLimit.WinLost.FormatDecimal(),
                $"{memberWinLossLimit.WinLossLimit.FormatDecimalToInt()}X");
        }
        else
        {
            messageBuilder.AppendFormat(
                TemplateHelper.Get(nameof(Template.MemberWinLossLimitAlertLossContent), language),
                memberWinLossLimit.Username,
                memberWinLossLimit.WinLost.FormatDecimal(),
                memberWinLossLimit.WinLossLimit.FormatDecimalToInt());
        }

        messageBuilder.Append("</pre>");

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }
}
