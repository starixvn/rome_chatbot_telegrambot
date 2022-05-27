using Fanex.Data.Repository;

namespace TelegramBot.SenderService.Criteria;

public class GetAllBotCriteria : CriteriaBase
{
    public GetAllBotCriteria(bool isTest)
    {
        IsTest = isTest;
    }

    public bool IsTest { get; }

    public override string GetSettingKey() => "BotSelAll";

    public override bool IsValid() => true;
}
