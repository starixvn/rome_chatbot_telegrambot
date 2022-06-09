namespace WLMonitored.Shared.Models;

public record AccumulatedWinLoss
{
    public int CustId { get; init; }

    public decimal AccumWL { get; init; }

    public DateTime UpdatedTime { get; init; }
}
