namespace TelegramBot.Importer.Models.ProblemAccounts;

public record GroupProblemAccount(
    int CustId,
    List<Tuple<string, DateTime>> Usernames,
    List<long> LogIds);
