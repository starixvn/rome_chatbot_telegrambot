namespace TelegramBot.Importer.Criteria.Announment;

public class ImportSystemAnnouncementCommand : NonQueryCommandBase
{
    public ImportSystemAnnouncementCommand(string xmlData, string environment, DateTime lastQueryTime)
        : base(xmlData, environment)
    {
        LastQueryTime = lastQueryTime;
    }

    public DateTime LastQueryTime { get; }
}