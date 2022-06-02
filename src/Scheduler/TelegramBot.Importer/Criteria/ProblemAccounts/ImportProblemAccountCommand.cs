namespace TelegramBot.Importer.Criteria.ProblemAccounts;

public class ImportProblemAccountCommand : NonQueryCommandBase
{
    public ImportProblemAccountCommand(
        string xmlData,
        string xmlDataLog,
        string environment,
        int maxLogId) : base(xmlData, environment)
    {
        XmlDataLog = xmlDataLog;
        MaxLogId = maxLogId;
    }

    public string XmlDataLog { get; }

    public int MaxLogId { get; }

    public override string GetSettingKey()
        => "Age_Chatbot_ProblemAccountsImport";

    public override bool IsValid()
        => base.IsValid()
        && !string.IsNullOrWhiteSpace(XmlDataLog);
}