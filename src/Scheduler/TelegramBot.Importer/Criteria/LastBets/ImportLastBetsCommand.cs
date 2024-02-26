namespace TelegramBot.Importer.Criteria.LastBets;

public class ImportLastBetsCommand : NonQueryCommandBase
{
    public ImportLastBetsCommand(
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