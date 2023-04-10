namespace TelegramBot.Importer.Models;

public class Customer
{
    public int CustId { get; init; }

    public string UserName { get; init; }

    public byte RoleId { get; init; }

    public byte CurrencyId { get; init; }

    public int ARecommendId { get; init; }

    public int MRecommendId { get; init; }

    public int SRecommendId { get; init; }

    public string Site { get; init; }

    public string SiteLabel { get; init; }

    public bool IsIMBC => Site.Equals("IMBC");

    public bool IsAllDayBalTransfer { get; init; }

    public int SiteCode { get; init; }

    public bool Enabled { get; init; }
}