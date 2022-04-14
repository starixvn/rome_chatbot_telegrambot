using Fanex.Data.Repository;

namespace TelegramBot.Infrastructure.Setting.Criteria
{
    public class CustBotMessengerSelCriteria : CriteriaBase
    {
        public CustBotMessengerSelCriteria(int custId, int subAccId, bool isTest)
        {
            CustId = custId;
            SubAccId = subAccId;
            IsTest = isTest;
        }

        public int CustId { get; }

        public int SubAccId { get; }

        public bool IsTest { get; }

        public override string GetSettingKey() => "CustBotMessengerSel";

        public override bool IsValid() => CustId > 0;
    }
}
