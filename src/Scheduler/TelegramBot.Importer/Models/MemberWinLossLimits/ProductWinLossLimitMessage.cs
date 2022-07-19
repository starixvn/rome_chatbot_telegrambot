namespace TelegramBot.Importer.Models.MemberWinLossLimits;
public record ProductWinLossLimitMessage(
    string ProductName,
    decimal CurrentWinLoss,
    decimal MaximumWinLoss,
    decimal CurrentWinLossPercentage
);