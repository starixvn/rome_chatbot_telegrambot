using Fanex.Data.Repository;

namespace TelegramBot.Modules.Action.Criteria
{
    public class CustBotActionInChainSelCriteria : CriteriaBase
    {
        public CustBotActionInChainSelCriteria(int userId)
        {
            UserId = userId;
        }

        public int UserId { get; }

        public override string GetSettingKey() => "CustBotActionInChainSel";

        public override bool IsValid() => UserId > 0;
    }
}