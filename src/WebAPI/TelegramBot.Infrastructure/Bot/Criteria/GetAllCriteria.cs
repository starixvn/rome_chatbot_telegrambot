using Fanex.Data.Repository;

namespace TelegramBot.Infrastructure.Bot.Criteria
{
    public class GetAllCriteria : CriteriaBase
    {
        public GetAllCriteria(bool isTest)
        {
            IsTest = isTest;
        }

        public bool IsTest { get; }

        public override string GetSettingKey() => "BotSelAll";

        public override bool IsValid() => true;
    }
}
