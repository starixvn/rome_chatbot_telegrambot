namespace TelegramBot.Importer.Models.ProblemAccounts;

public record ProblemAccount(
    int LogId,
    int CustId,
    string Username,
    int Recommend,
    int MRecommend,
    int SRecommend);
