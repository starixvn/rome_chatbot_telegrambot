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

    string BuildMessage(string language, IEnumerable<string> usernames, string currentDate);

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
            .GroupBy(pa => new { pa.SRecommend, CreatedDate = pa.CreatedDate.Date });

        foreach (var groupSuper in groupSuperList)
        {
            var master = GetGroupProblemAccount(
                groupSuper.Key.SRecommend,
                groupSuper.Key.CreatedDate,
                groupSuper.Where(pa => pa.MRecommend == pa.CustId));
            groupProblemAccounts.Add(master);

            var groupMasterList = groupSuper
                .Where(pa => pa.MRecommend != pa.CustId)
                .GroupBy(pa => new { pa.MRecommend, CreatedDate = pa.CreatedDate.Date });

            foreach (var groupMaster in groupMasterList)
            {
                var agent = GetGroupProblemAccount(
                    groupMaster.Key.MRecommend,
                    groupSuper.Key.CreatedDate,
                    groupMaster.Where(pa => pa.Recommend == pa.CustId));
                groupProblemAccounts.Add(agent);
                master.Usernames.AddRange(agent.Usernames);
                master.LogIds.AddRange(agent.LogIds);

                var groupAgentList = groupMaster
                    .Where(pa => pa.Recommend != pa.CustId)
                    .GroupBy(pa => new { pa.Recommend, CreatedDate = pa.CreatedDate.Date });

                foreach (var groupAgent in groupAgentList)
                {
                    var member = GetGroupProblemAccount(
                        groupAgent.Key.Recommend,
                        groupSuper.Key.CreatedDate,
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
        DateTime createdDate,
        IEnumerable<ProblemAccount> problemAccounts)
    {
        var usernames = new List<string>();
        var logIds = new List<long>();
        
        foreach (var item in problemAccounts)
        {
            usernames.Add(item.Username);
            logIds.Add(item.LogId);
        }

        return new GroupProblemAccount(
            custId,
            createdDate,
            usernames.Distinct().OrderBy(username => username).ToList(),
            logIds);
    }

    public string BuildMessage(string language, IEnumerable<string> usernames, string currentDate)
    {
        var usernamesCount = usernames.Count();

        var messageBuilder = new StringBuilder();
        messageBuilder
            .AppendLine(TemplateHelper.BuildTitle(language, "ProblemAccountAlert", currentDate));

#pragma warning disable S3240
        if (usernamesCount <= MaxUsernames)
        {
            messageBuilder
                .AppendLine(string.Format(TemplateHelper.Get("NewProblemAccounts", language), usernames.Join(", ")))
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