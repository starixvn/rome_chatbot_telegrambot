namespace TelegramBot.Importer.Models.ProblemAccounts;

public record GroupProblemAccount(
    int Id,
    int CustId,
    List<string> Usernames,
    List<long> LogIds);
