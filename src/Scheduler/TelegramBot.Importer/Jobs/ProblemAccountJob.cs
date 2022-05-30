
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Extensions;
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
        // var (maxLogId, problemAccounts) = await problemAccountService.GetProblemAccounts(systemSetting.MaxLogId, systemSetting.BatchSize);

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

    // private static void GroupByMasters(IEnumerable<ProblemAccount> problemAccounts, List<string> notifications)
    // {
    //     var groupMaster = problemAccounts.GroupBy(pa => pa.MRecommend);

    //     foreach (var problemAccountList in groupMaster)
    //     {
    //         notifications.Add($"{problemAccountList.Key} - {string.Join(", ", problemAccountList.Select(pa => pa.Username))}");
    //     }
    // }
}