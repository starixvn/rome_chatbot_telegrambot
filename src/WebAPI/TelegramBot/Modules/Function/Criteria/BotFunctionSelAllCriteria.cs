using Fanex.Data.Repository;

namespace TelegramBot.Modules.Function.Criteria
{
    public class BotFunctionSelAllCriteria : CriteriaBase
    {
        public BotFunctionSelAllCriteria(bool isTest)
        {
            IsTest = isTest;
        }

        public bool IsTest { get; }

        public override string GetSettingKey() => "BotFunctionSelAll";

        public override bool IsValid() => true;
    }
}