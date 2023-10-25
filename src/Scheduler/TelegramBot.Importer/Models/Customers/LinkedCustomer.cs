namespace TelegramBot.Importer.Models.Customers;

public record LinkedCustomer
{
    public int CustId { get; init; }

    public int SubAccId { get; init; }

    public string Language { get; init; }

    public byte CustomerLevel { get; init; }

    public int UserId { get; init; }

    public bool IsRealUser { get; init; }

    public int BotId { get; init; }

    public string Site { get; init; }
}
