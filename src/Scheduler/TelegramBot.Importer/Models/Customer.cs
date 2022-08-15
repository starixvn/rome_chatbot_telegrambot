namespace TelegramBot.Importer.Models;

public class Customer
{
    public Customer()
    {
    }

    public Customer(
        int custId,
        string userName,
        byte roleId,
        byte currencyId,
        int aRecommendId,
        int mRecommendId,
        int sRecommendId,
        string site,
        int siteCode,
        bool enabled)
    {
        CustId = custId;
        UserName = userName;
        RoleId = roleId;
        CurrencyId = currencyId;
        ARecommendId = aRecommendId;
        MRecommendId = mRecommendId;
        SRecommendId = sRecommendId;
        Site = site;
        SiteCode = siteCode;
        Enabled = enabled;
    }

    public int CustId { get; }

    public string UserName { get; }

    public byte RoleId { get; }

    public byte CurrencyId { get; }

    public int ARecommendId { get; }

    public int MRecommendId { get; }

    public int SRecommendId { get; }

    public string Site { get; }

    public bool IsIMBC => Site.Equals("IMBC");

    public bool IsAllDayBalTransfer { get; }

    public int SiteCode { get; }

    public bool Enabled { get; }
}