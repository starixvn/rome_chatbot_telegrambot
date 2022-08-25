using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.MyWatchlist;

public class GetMyWatchlistCriteria : CriteriaBase
{
    public GetMyWatchlistCriteria(int lastLogId, int batchSize, string lastDataRow, string serverName)
    {
        LastLogId = lastLogId;
        BatchSize = batchSize;
        LastDataRow = lastDataRow;
        ServerName = serverName;
    }

    public int LastLogId { get; }

    public int BatchSize { get; }

    public string LastDataRow { get; set; }

    public string ServerName { get; set; }

    public int MaxLogId { get; set; }

    public override string GetSettingKey() => "Age_Chatbot_MyWatchlist_Sel";

    public override bool IsValid() => true;
}