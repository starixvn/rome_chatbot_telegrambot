namespace TelegramBot.Importer.Models.ProblemAccounts;

public record GroupProblemAccount(
    int CustId,
    List<string> Usernames,
    List<long> LogIds);
