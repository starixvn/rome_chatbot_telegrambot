namespace TelegramBot.Importer.Models.Customers;

public record LinkedCustomer(
    int CustId,
    int SubAccId,
    string Language,
    byte CustomerLevel,
    int UserId,
    bool IsRealUser);
