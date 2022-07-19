namespace TelegramBot.Importer.Models.ProblemAccounts;

public record ProblemAccount(
    long LogId,
    DateTime CreatedDate,
    int CustId,
    string Username,
    int Recommend,
    int MRecommend,
    int SRecommend);
