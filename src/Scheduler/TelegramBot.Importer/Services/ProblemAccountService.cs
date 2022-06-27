using System.Text;
using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.ProblemAccounts;
using TelegramBot.Importer.Extensions;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.ProblemAccounts;
using TelegramBot.Importer.Resources;

namespace TelegramBot.Importer.Services;

public interface IProblemAccountService
{
    Task<Tuple<int, IEnumerable<ProblemAccount>>> GetProblemAccounts(int lastLogId, int batchSize);

    IEnumerable<GroupProblemAccount> BuildGroupProblemAccounts(IEnumerable<ProblemAccount> problemAccounts);

    string BuildMessage(string language, IEnumerable<Tuple<string, DateTime>> usernames);

    Task<Result> Import(
        IEnumerable<ProblemAccountNotification> notifications,
        string environment,
        int maxLogId);
}

public class ProblemAccountService : NotificationServiceBase, IProblemAccountService
{
    private const int MaxUsernames = 10;

    public ProblemAccountService(IDynamicRepository dynamicRepository)
        : base(dynamicRepository)
    {
    }

    public async Task<Tuple<int, IEnumerable<ProblemAccount>>> GetProblemAccounts(int lastLogId, int batchSize)
    {
        var command = new GetProblemAccountCommand(lastLogId, batchSize);
        var problemAccounts = await dynamicRepository.FetchAsync<ProblemAccount>(command);

        return new Tuple<int, IEnumerable<ProblemAccount>>(command.MaxLogId, problemAccounts);
    }

    public IEnumerable<GroupProblemAccount> BuildGroupProblemAccounts(IEnumerable<ProblemAccount> problemAccounts)
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
    
    private static GroupProblemAccount GetGroupProblemAccount(
        int custId,
        IEnumerable<ProblemAccount> problemAccounts)
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

    public string BuildMessage(string language, IEnumerable<Tuple<string, DateTime>> usernames)
    {
        var usernamesCount = usernames.Count();

        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine(TemplateHelper.BuildTitleWithoutStrikeThrough(language, "ProblemAccountAlert", GetDateTimeOnTitle(usernames)));

#pragma warning disable S3240
        if (usernamesCount <= MaxUsernames)
        {
            messageBuilder
                .AppendLine(
                    string.Format(TemplateHelper.Get("NewProblemAccounts", language),
                    usernames.Select(u => u.Item1).Join(", ")))
                .Append(TemplateHelper.Get("ProblemAccountShowAllDownlines", language));
        }
        else
        {
            messageBuilder
                .Append(string.Format(TemplateHelper.Get("ProblemAccountShowXDownlines", language), usernamesCount));
        }
#pragma warning restore S3240

        return TemplateHelper.EncodeXml(messageBuilder.ToString());
    }

    private static string GetDateTimeOnTitle(IEnumerable<Tuple<string, DateTime>> usernames)
    {
        var createdDates = usernames.Select(u => u.Item2).ToList();
        var minCreatedDate = createdDates.Min();
        var maxCreatedDate = createdDates.Max();

        if (minCreatedDate == maxCreatedDate)
        {
            return minCreatedDate.ToLongDateTime24hWithSlash();
        }

        if (minCreatedDate.Date == maxCreatedDate.Date)
        {
            return $"{minCreatedDate.ToLongDateTime24hWithSlash()} - {maxCreatedDate.ToTime24h()}";
        }

        return $"{minCreatedDate.ToLongDateTime24hWithSlash()} - {maxCreatedDate.ToLongDateTime24hWithSlash()}";
    }

    public Task<Result> Import(
        IEnumerable<ProblemAccountNotification> notifications,
        string environment,
        int maxLogId)
        => ExecuteNonQuery(new ImportProblemAccountCommand(
            BuildXmlData(notifications),
            BuildXmlDataLog(notifications),
            environment,
            maxLogId));
}