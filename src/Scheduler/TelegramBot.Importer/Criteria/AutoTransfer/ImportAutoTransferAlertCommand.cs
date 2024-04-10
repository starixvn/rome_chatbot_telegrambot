namespace TelegramBot.Importer.Criteria.AutoTransfer;

public class ImportAutoTransferAlertCommand : NonQueryCommandBase
{
    public ImportAutoTransferAlertCommand(
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