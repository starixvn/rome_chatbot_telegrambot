namespace TelegramBot.Importer.Models.Announcement;

public interface IParam
{
    int UserId { get; }

    string UserName { get; }

    string Language { get; }

    byte LevelId { get; }

    int AdminId { get; }

    bool IsInternal { get; }

    string LangKey { get; }

    byte CurrencyId { get; }

    int ARecommendId { get; }

    int MRecommendId { get; }

    int SRecommendId { get; }

    bool IsSuper { get; }

    bool IsMaster { get; }

    bool IsAgent { get; }

    string Site { get; }

    IEnumerable<byte> OfferedProductIds { get; }

    bool IsSubAcc { get; }

    bool IsSyncedCasino { get; }

    bool EnableYesterdayTransfer { get; }

    IDictionary<string, object> DynamicParams { get; }
}