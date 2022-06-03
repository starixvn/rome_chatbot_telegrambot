
namespace TelegramBot.Importer.Criteria.MemberWinLossLimits;

public class ImportMemberWinLossLimitCommand : NonQueryCommandBase
{
    public ImportMemberWinLossLimitCommand(
        string xmlData,
        string xmlDataLog,
        string environment,
        long maxLogId) : base(xmlData, environment)
    {
        XmlDataLog = xmlDataLog;
        MaxLogId = maxLogId;
    }

    public string XmlDataLog { get; }

    public long MaxLogId { get; }

    public override string GetSettingKey()
        => "Age_Chatbot_WinLossLimitNotificationImport";

    public override bool IsValid()
        => base.IsValid()
        && !string.IsNullOrWhiteSpace(XmlDataLog);
}
