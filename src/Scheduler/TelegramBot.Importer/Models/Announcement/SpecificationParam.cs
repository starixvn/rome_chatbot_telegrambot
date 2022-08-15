using TelegramBot.Importer.Constants;

namespace TelegramBot.Importer.Models.Announcement;

public class SpecificationParam : IParam
{
    private readonly Customer customer;

    public SpecificationParam(Customer customer, bool isSubAcc, IEnumerable<byte> offeredProductIds)
    {
        this.customer = customer;
        IsSubAcc = isSubAcc;
        OfferedProductIds = offeredProductIds;
    }

    public int AdminId => 0;

    public int ARecommendId => customer.ARecommendId;

    public byte CurrencyId => customer.CurrencyId;

    public Dictionary<string, object> DynamicParams { get; }

    public bool EnableYesterdayTransfer => customer.IsAllDayBalTransfer;

    public bool IsAgent => customer.RoleId == CustomerLevel.Agent;

    public bool IsIMBCCustomer => customer.IsIMBC;

    public bool IsInternal => false;

    public bool IsMaster => customer.RoleId == CustomerLevel.Master;

    public bool IsSubAcc { get; }

    public bool IsSuper => customer.RoleId == CustomerLevel.Super;

    public string LangKey { get; }

    public string Language { get; }

    public byte LevelId => customer.RoleId;

    public int MRecommendId => customer.MRecommendId;

    public IEnumerable<byte> OfferedProductIds { get; }

    public string Site => customer.Site;

    public int SRecommendId => customer.SRecommendId;

    public int UserId => customer.CustId;

    public string UserName => customer.UserName;

    public bool IsSyncedCasino => throw new NotImplementedException();

    IDictionary<string, object> IParam.DynamicParams => throw new NotImplementedException();
}