using Fanex.Data.Repository;

namespace TelegramBot.Importer.Criteria.SystemSettings;

public class UpdateSystemSettingCommand : NonQueryCommand
{
    public UpdateSystemSettingCommand(
        string serviceName,
        string environment,
        string setting)
    {
        ServiceName = serviceName;
        Environment = environment;
        Setting = setting;
    }

    public string ServiceName { get; }

    public string Environment { get; }

    public string Setting { get; }

    public override string GetSettingKey()
        => "Age_Chatbot_BotSystemSetting_Upd";

    public override bool IsValid()
        => !string.IsNullOrWhiteSpace(ServiceName)
        && !string.IsNullOrWhiteSpace(Setting);
}