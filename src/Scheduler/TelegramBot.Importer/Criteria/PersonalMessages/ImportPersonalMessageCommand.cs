namespace TelegramBot.Importer.Criteria.PersonalMessages;

public class ImportPersonalMessageCommand : NonQueryCommandBase
{
    public ImportPersonalMessageCommand(
        string xmlData,
        string environment,
        int maxMsgTargetId) : base(xmlData, environment)
        => MaxMsgTargetId = maxMsgTargetId;

    public int MaxMsgTargetId { get; }

    public override string GetSettingKey()
        => "Age_Chatbot_PersonalMessageImport";
}