namespace TelegramBot.Importer.Models.LastBets;

public record LastBetsModel
{
    public string FilterName { get; set; }

    public int TicketCount { get; set; }

    public string Tickets { get; set; }

    public long LastTransId { get; set; }
}