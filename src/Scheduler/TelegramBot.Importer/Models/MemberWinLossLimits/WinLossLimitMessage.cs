namespace TelegramBot.Importer.Models.MemberWinLossLimits;

public record WinLossLimitMessage(
    string Username,
    string AUsername,
    string MUsername,
    string SUsername,
    IEnumerable<ProductWinLossLimitMessage> Products);