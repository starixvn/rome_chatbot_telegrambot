namespace TelegramBot.Importer.Models.AutoTransfer;

public record AutoTransferResultModel
{
    public int CustId { get; set; }
    public string CustName { get; set; }
    public int UserId { get; set; }
    public string Language { get; set; }

    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public int CancelledCount { get; set; }

    public string FailedCusts { get; set; }
    public string CancelledCusts { get; set; }
}
