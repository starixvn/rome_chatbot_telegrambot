
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Models.ProblemAccounts;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.Jobs;

public sealed class ProblemAccountJob : NotificationBaseJob
{
    private readonly IProblemAccountService problemAccountService;
    private readonly IMessageTemplate messageTemplate;
    private readonly IImportNotificationService importNotificationService;

    public ProblemAccountJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        IProblemAccountService problemAccountService,
        ICustomerService customerService,
        IMessageTemplate messageTemplate,
        IImportNotificationService importNotificationService)
        : base(maintenanceService, loggerService, systemSettingService, customerService)
    {
        this.problemAccountService = problemAccountService;
        this.messageTemplate = messageTemplate;
        this.importNotificationService = importNotificationService;
    }

    protected override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        var (maxLogId, problemAccounts) = await problemAccountService.GetProblemAccounts(systemSetting.MaxLogId, systemSetting.BatchSize);

        // if (problemAccounts.Any())
        // {
        //     var customerHashSet = new HashSet<int>();
        //     foreach (var item in problemAccounts)
        //     {
        //         customerHashSet.Add(item.Recommend);
        //         customerHashSet.Add(item.MRecommend);
        //         customerHashSet.Add(item.SRecommend);
        //     }

        //     var custIds = customerHashSet.AsEnumerable().JoinWithComma();
        //     var linkedCustomers = await GetLinkedCustomers(custIds, job);
        // }
    }

    private static IEnumerable<Tuple<int, IEnumerable<string>>> GroupBySupers(IEnumerable<ProblemAccount> problemAccounts)
    {
        var notifications = new List<Tuple<int, IEnumerable<string>>>();

        notifications.AddRange(problemAccounts
            .Where(pa => pa.MRecommend > 0)
            .OrderBy(pa => pa.Username)
            .GroupBy(
                pa => pa.SRecommend,
                (superId, problemAccountList) =>
                {
                    notifications.AddRange(GroupByMasters(problemAccountList));

                    return new Tuple<int, IEnumerable<string>>(
                        superId,
                        problemAccountList.Select(pa => pa.Username)); 
                }));

        return notifications;
    }

    private static IEnumerable<Tuple<int, IEnumerable<string>>> GroupByMasters(IEnumerable<ProblemAccount> problemAccounts)
    {
        var notifications = new List<Tuple<int, IEnumerable<string>>>();

        notifications.AddRange(problemAccounts
            .Where(pa => pa.Recommend > 0)
            .GroupBy(
                pa => pa.MRecommend,
                (masterId, problemAccountList) =>
                    {
                        var validProblemAccountList = problemAccountList.Where(pa => pa.Recommend > 0).ToList();

                        var agentNotifications = GroupByAgents(validProblemAccountList);
                        notifications.AddRange(agentNotifications);

                        return new Tuple<int, IEnumerable<string>>(
                            masterId,
                            validProblemAccountList.Select(pa => pa.Username));
                    }));

        return notifications;
    }

    private static IEnumerable<Tuple<int, IEnumerable<string>>> GroupByAgents(IEnumerable<ProblemAccount> problemAccounts)
        => problemAccounts
        .GroupBy(
            pa => pa.Recommend,
            (agentId, problemAccountList) => new Tuple<int, IEnumerable<string>>(
                agentId,
                problemAccountList.Select(pa => pa.Username)))
        .ToList();
}