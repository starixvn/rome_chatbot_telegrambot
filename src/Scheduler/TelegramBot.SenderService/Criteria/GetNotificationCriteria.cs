using Fanex.Data.Repository;

namespace TelegramBot.SenderService.Criteria;

public sealed class GetNotificationCriteria : CriteriaBase
{
    public GetNotificationCriteria(string environment)
        => Environment = environment;

    public string Environment { get; }

    public override string GetSettingKey()
        => "Age_Chatbot_SendNotification";

    public override bool IsValid() => true;
}