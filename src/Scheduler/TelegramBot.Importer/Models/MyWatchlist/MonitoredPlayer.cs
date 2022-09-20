
namespace TelegramBot.Importer.Models.MyWatchlist;

public record MonitoredPlayer
{
    public int LogId { get; init; }

    public int CustId { get; init; }

    public string Username { get; init; }

    public decimal AccumWL { get; init; }

    public decimal WinningThreshold { get; init; }

    public decimal LosingThreshold { get; init; }

    public byte SettingType { get; init; }

    public DateTime CreatedDate { get; init; }

    public int Recommend { get; init; }

    public int MRecommend { get; init; }

    public int SRecommend { get; init; }
}