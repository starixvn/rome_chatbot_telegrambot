using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.Notifications;

public class ImportProblemAccountCommand : NonQueryCommand
{
    public ImportProblemAccountCommand(
        string xmlData,
        string xmlDataLog,
        string environment,
        int maxLogId)
    {
        XmlData = xmlData;
        XmlDataLog = xmlDataLog;
        Environment = environment;
        MaxLogId = maxLogId;
    }

    public string XmlData { get; }

    public string XmlDataLog { get; }

    public string Environment { get; }

    public int MaxLogId { get; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; } = default!;

    public override string GetSettingKey()
        => "Age_Chatbot_ProblemAccountsImport";

    public override bool IsValid()
        => !string.IsNullOrWhiteSpace(XmlData)
        && !string.IsNullOrWhiteSpace(XmlDataLog);
}