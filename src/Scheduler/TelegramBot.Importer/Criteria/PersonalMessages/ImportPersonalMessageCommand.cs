using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.PersonalMessages;

public class ImportPersonalMessageCommand : NonQueryCommand
{
    public ImportPersonalMessageCommand(string xmlData, string environment, int maxMsgTargetId)
    {
        XmlData = xmlData;
        Environment = environment;
        MaxMsgTargetId = maxMsgTargetId;
    }

    public string XmlData { get; }

    public string Environment { get; }

    public int MaxMsgTargetId { get; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; } = default!;

    public override string GetSettingKey()
        => "Age_Chatbot_PersonalMessageImport";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(XmlData);
}