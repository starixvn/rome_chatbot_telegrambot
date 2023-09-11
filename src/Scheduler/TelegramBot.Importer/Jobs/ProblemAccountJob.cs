
using System.Text;
using ONEbook.UM.Services;
using TelegramBot.Importer.Configuration;
using TelegramBot.Importer.Constants;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models.Customers;
using TelegramBot.Importer.Models.ProblemAccounts;
using TelegramBot.Importer.Models.SystemSettings;
using TelegramBot.Importer.Resources;
using TelegramBot.Importer.Services;

namespace TelegramBot.Importer.Jobs;

public sealed class ProblemAccountJob : NotificationBaseJob
{
    private const int MaxUsernames = 10;
    private readonly IProblemAccountService problemAccountService;

    public ProblemAccountJob(
        IMaintenanceService maintenanceService,
        ILoggerService loggerService,
        ISystemSettingService systemSettingService,
        IProblemAccountService problemAccountService,
        ICustomerService customerService)
        : base(maintenanceService, loggerService, systemSettingService, customerService)
        => this.problemAccountService = problemAccountService;

    public override async Task Execute(ScheduleJob job, SystemSetting systemSetting)
    {
        var notifications = Enumerable.Empty<ProblemAccountNotification>();

        var (maxLogId, problemAccounts) = await problemAccountService.GetProblemAccounts(systemSetting.MaxLogId, systemSetting.BatchSize);

        if (problemAccounts.Any())
        {
            var groupProblemAccounts = BuildGroupProblemAccounts(problemAccounts);

            var custIds = groupProblemAccounts.Select(pa => pa.CustId).JoinWithComma();
            var linkedCustomers = await GetLinkedCustomers(custIds, job);

            if (linkedCustomers.Any())
            {
                var customerStatus = await GetCustomerStatus(linkedCustomers);

                notifications = BuildProblemAccountNotifications(
                    job,
                    groupProblemAccounts,
                    linkedCustomers,
                    customerStatus);
            }
        }

        // In case of NoData, call Import to update JobStatus=Completed and LastRuntime.
        var result = await problemAccountService.Import(
            notifications,
            job.Environment,
            maxLogId);

        if (result.IsFailed)
        {
            await loggerService.Error(job, result.Message);
        }
    }

    private static IEnumerable<ProblemAccountNotification> BuildProblemAccountNotifications(
        ScheduleJob job,
        IEnumerable<GroupProblemAccount> groupProblemAccounts,
        IEnumerable<LinkedCustomer> linkedCustomers,
        IEnumerable<CustomerStatus> customerStatus)
    {
        var identity = 1;

        var notifications =
            from gpa in groupProblemAccounts
            join customer in linkedCustomers on gpa.CustId equals customer.CustId
            join status in customerStatus on new { customer.CustId, customer.SubAccId }
                equals new { status.CustId, status.SubAccId }
            where status.IsValid(job.SubAccPermission)
            select BuildProblemAccountNotification(job, identity++, gpa, customer);

        return notifications.ToList();
    }

    private static ProblemAccountNotification BuildProblemAccountNotification(
        ScheduleJob job,
        int identity,
        GroupProblemAccount groupProblemAccount,
        LinkedCustomer customer)
        => new(
            identity,
            customer.UserId,
            customer.BotId,
            BuildMessage(customer.Language, groupProblemAccount.Usernames),
            customer.IsRealUser,
            job.IsUAT,
            groupProblemAccount.LogIds);

    private static IEnumerable<GroupProblemAccount> BuildGroupProblemAccounts(IEnumerable<ProblemAccount> problemAccounts)
    {
        var groupProblemAccounts = new List<GroupProblemAccount>();

        var groupSuperList = problemAccounts
            .Where(pa => pa.SRecommend != pa.CustId)
            .GroupBy(pa => pa.SRecommend);

        foreach (var groupSuper in groupSuperList)
        {
            var master = GetGroupProblemAccount(
                groupSuper.Key,
                groupSuper.Where(pa => pa.MRecommend == pa.CustId));
            groupProblemAccounts.Add(master);

            var groupMasterList = groupSuper
                .Where(pa => pa.MRecommend != pa.CustId)
                .GroupBy(pa => pa.MRecommend);

            foreach (var groupMaster in groupMasterList)
            {
                var agent = GetGroupProblemAccount(
                    groupMaster.Key,
                    groupMaster.Where(pa => pa.Recommend == pa.CustId));
                groupProblemAccounts.Add(agent);
                master.Usernames.AddRange(agent.Usernames);
                master.LogIds.AddRange(agent.LogIds);

                var groupAgentList = groupMaster
                    .Where(pa => pa.Recommend != pa.CustId)
                    .GroupBy(pa => pa.Recommend);

                foreach (var groupAgent in groupAgentList)
                {
                    var member = GetGroupProblemAccount(
                        groupAgent.Key,
                        groupAgent);
                    groupProblemAccounts.Add(member);
                    agent.Usernames.AddRange(member.Usernames);
                    agent.LogIds.AddRange(member.LogIds);
                    master.Usernames.AddRange(member.Usernames);
                    master.LogIds.AddRange(member.LogIds);
                }
            }
        }

        return groupProblemAccounts;
    }

    private static GroupProblemAccount GetGroupProblemAccount(int custId, IEnumerable<ProblemAccount> problemAccounts)
    {
        var usernames = new Dictionary<string, Tuple<string, DateTime>>();
        var logIds = new List<long>();

        foreach (var item in problemAccounts)
        {
            usernames[item.Username] = new Tuple<string, DateTime>(item.Username, item.CreatedDate);
            logIds.Add(item.LogId);
        }

        return new GroupProblemAccount(
            custId,
            usernames.Values.OrderBy(username => username).ToList(),
            logIds);
    }

    private static string BuildMessage(string language, IEnumerable<Tuple<string, DateTime>> usernames)
    {
        var usernamesCount = usernames.Count();

        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine(TemplateHelper.BuildTitleWithEmoji(language, "ProblemAccountAlert", EmojiUnicode.Warning))
            .AppendLine();

        if (usernamesCount <= MaxUsernames)
        {
            messageBuilder.AppendLine(
                string.Format(TemplateHelper.Get("NewProblemAccounts", language),
                usernamesCount,
                usernames.Select(u => u.Item1).Join(", ")));
        }
        else
        {
            messageBuilder.Append(string.Format(TemplateHelper.Get("ProblemAccountShowXDownlines", language), usernamesCount));
        }

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }
}