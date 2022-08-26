using Fanex.Data.Repository;
using TelegramBot.Importer.Criteria.ProblemAccounts;
using TelegramBot.Importer.Models;
using TelegramBot.Importer.Models.ProblemAccounts;

namespace TelegramBot.Importer.Services;

public interface IProblemAccountService
{
    Task<Tuple<int, IEnumerable<ProblemAccount>>> GetProblemAccounts(int lastLogId, int batchSize);

    Task<Result> Import(
        IEnumerable<ProblemAccountNotification> notifications,
        string environment,
        int maxLogId);
}

public class ProblemAccountService : NotificationServiceBase, IProblemAccountService
{
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