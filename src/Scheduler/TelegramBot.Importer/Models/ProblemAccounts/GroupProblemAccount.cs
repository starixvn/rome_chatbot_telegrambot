namespace TelegramBot.Importer.Models.ProblemAccounts;

public record GroupProblemAccount(
    int CustId,
    DateTime CreatedDate,
    List<string> Usernames,
    List<long> LogIds);
