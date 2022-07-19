using ONEbook.UM.Services;
using Quartz;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.MemberWinLossLimits;
using TelegramBot.Importer.Models.SystemSettings;
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

    protected override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
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
            var linkedCustomers = await GetLinkedCustomers(strCustIds, job);

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

        var currentDate = DateTime.Today.ToSimpleDate();
        foreach (var customer in verifiedCustomers)
        {
            notifications.Add(BuildMemberWinLossLimitNotification(job, memberWinLossLimits, customer, currentDate));
        }

        return notifications;
    }

    private MemberWinLossLimitNotification BuildMemberWinLossLimitNotification(
        ScheduleJob job,
        IEnumerable<MemberWinLossLimit> memberWinLossLimits,
        LinkedCustomer customer,
        string currentDate)
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

        return new MemberWinLossLimitNotification(
           customer.UserId,
           customer.BotId,
           memberWinLostLimitService.BuildMessage(
               winLossLimitMessages,
               customer.Language,
               customer.CustomerLevel,
               currentDate
           ),
           customer.IsRealUser,
           job.IsUAT,
           logIds
       );
    }
}
