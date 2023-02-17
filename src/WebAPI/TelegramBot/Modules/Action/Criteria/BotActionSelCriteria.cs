using Fanex.Data.Repository;

namespace TelegramBot.Modules.Action.Criteria
{
    public class BotActionSelCriteria : CriteriaBase
    {
        public BotActionSelCriteria(bool isTest)
        {
            IsTest = isTest;
        }

        public bool IsTest { get; }

        public override string GetSettingKey() => "BotActionSel";

        public override bool IsValid() => true;
    }
}