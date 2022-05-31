using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.ProblemAccounts;
using TelegramBot.Importer.Models.ProblemAccounts;

namespace TelegramBot.Importer.Services;

public interface IProblemAccountService
{
    Task<Tuple<int, IEnumerable<ProblemAccount>>> GetProblemAccounts(int lastLogId, int batchSize);
    
    string BuildMessage(string language, string usernames);
}

public class ProblemAccountService : IProblemAccountService
{
    private readonly IDynamicRepository dynamicRepository;

    public ProblemAccountService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public async Task<Tuple<int, IEnumerable<ProblemAccount>>> GetProblemAccounts(int lastLogId, int batchSize)
    {
        var command = new GetProblemAccountCommand(lastLogId, batchSize);
        var problemAccounts = await dynamicRepository.FetchAsync<ProblemAccount>(command);

        if (problemAccounts.Any())
        {
            var notifications = GroupBySupers(problemAccounts);
        }

        return new Tuple<int, IEnumerable<ProblemAccount>>(command.MaxLogId, problemAccounts);
    }

    private static IEnumerable<Tuple<int, IEnumerable<string>>> GroupBySupers(IEnumerable<ProblemAccount> problemAccounts)
    {
        var notifications = new List<Tuple<int, IEnumerable<string>>>();

        notifications.AddRange(problemAccounts
            .Where(pa => pa.SRecommend != pa.CustId)
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
                        var agentNotifications = GroupByAgents(problemAccountList);
                        notifications.AddRange(agentNotifications);

                        return new Tuple<int, IEnumerable<string>>(
                            masterId,
                            problemAccountList.Select(pa => pa.Username));
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

    public string BuildMessage(string language, string usernames)
    {
        throw new NotImplementedException();
    }
}