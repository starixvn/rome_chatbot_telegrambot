namespace TelegramBot.Notifications.Shared.Models;

public record LinkedCustomer(
    int CustId,
    int SubAccId,
    string Language,
    byte CustomerLevel,
    int UserId,
    bool IsRealUser);
