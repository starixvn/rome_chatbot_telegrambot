namespace TelegramBot.Importer.Criteria.Announment;

public class ImportAnnouncementCommand : NonQueryCommandBase
{
    public ImportAnnouncementCommand(string xmlData, string environment, DateTime lastQueryTime)
        : base(xmlData, environment)
    {
        LastQueryTime = lastQueryTime;
    }

    public DateTime LastQueryTime { get; }

    public override string GetSettingKey() => "Age_Chatbot_AnnouncementNotificationImport";
}