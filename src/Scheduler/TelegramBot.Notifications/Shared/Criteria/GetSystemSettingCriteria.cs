using Fanex.Data.Repository;

namespace TelegramBot.Notifications.Shared.Criteria;

public class GetSystemSettingCriteria : CriteriaBase
{
    public GetSystemSettingCriteria(string serviceName, string environment)
    {
        ServiceName = serviceName;
        Environment = environment;
    }

    public string ServiceName { get; }

    public string Environment { get; }

    public override string GetSettingKey()
        => "Age_Chatbot_BotSystemSetting_Sel";

    public override bool IsValid()
        => !string.IsNullOrWhiteSpace(ServiceName);
}