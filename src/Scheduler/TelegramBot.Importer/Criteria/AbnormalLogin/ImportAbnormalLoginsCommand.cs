namespace TelegramBot.Importer.Criteria.AbnormalLogin;

public class ImportAbnormalLoginsCommand : NonQueryCommandBase
{
    public ImportAbnormalLoginsCommand(
        string xmlData,
        string environment,
        string functionKey)
        : base(xmlData, environment)
    {
        FunctionKey = functionKey;
    }

    public string FunctionKey { get; }

    public override string GetSettingKey() => "Age_Chatbot_ServiceNotificationImport";
}