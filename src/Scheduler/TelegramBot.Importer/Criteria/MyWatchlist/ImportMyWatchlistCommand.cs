namespace TelegramBot.Importer.Criteria.MyWatchlist;

public class ImportMyWatchlistCommand : NonQueryCommandBase
{
    public ImportMyWatchlistCommand(string xmlData, string xmlDataLog, string lastDataRow, string serverName, int maxLogId, string environment)
        : base(xmlData, environment)
    {
        XmlDataLog = xmlDataLog;
        LastDataRow = lastDataRow ?? string.Empty;
        ServerName = serverName;
        MaxLogId = maxLogId;
    }

    public string XmlDataLog { get; }

    public string LastDataRow { get; }

    public string ServerName { get; }

    public int MaxLogId { get; }

    public override string GetSettingKey() => "Age_Chatbot_MyWatchlistImport";

    public override bool IsValid() => base.IsValid() && !string.IsNullOrWhiteSpace(XmlDataLog);
}