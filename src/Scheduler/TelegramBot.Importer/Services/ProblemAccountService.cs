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

        return new Tuple<int, IEnumerable<ProblemAccount>>(command.MaxLogId, problemAccounts);
    }

    public string BuildMessage(string language, string usernames)
    {
        throw new NotImplementedException();
    }
}