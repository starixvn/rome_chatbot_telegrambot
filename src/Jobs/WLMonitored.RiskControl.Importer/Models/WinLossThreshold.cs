namespace WLMonitored.RiskControl.Importer.Models;

public record WinLossThreshold
{
    public int CustId { get; init; }

    public decimal? WinningThreshold { get; init; }

    public decimal? LosingThreshold { get; init; }

    public DateTime? WinningUpdateTime { get; init; }

    public DateTime? LosingUpdateTime { get; init; }

    public byte SettingType { get; init; }

    public byte SportType { get; init; }

    public bool IsExclude { get; init; }
}