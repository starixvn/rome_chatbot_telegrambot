using Fanex.Data.Repository;

namespace TelegramBot.Modules.Setting.Criteria
{
    public class CustBotMessengerSelCriteria : CriteriaBase
    {
        public CustBotMessengerSelCriteria(int custId, int subAccId, bool isTest, bool status = true, bool isVerifyOTP = true)
        {
            CustId = custId;
            SubAccId = subAccId;
            IsTest = isTest;
            Status = status;
            IsVerifyOTP = isVerifyOTP;
        }

        public int CustId { get; }

        public int SubAccId { get; }

        public bool IsTest { get; }

        public bool Status { get; }

        public bool IsVerifyOTP { get; }

        public override string GetSettingKey() => "CustBotMessengerSel";

        public override bool IsValid() => CustId > 0;
    }
}
