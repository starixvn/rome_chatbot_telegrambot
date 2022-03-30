using Fanex.Data.Repository;
using TelegramBot.Infrastructure.Shared;

namespace TelegramBot.Infrastructure.Setting.Criteria
{
    public class CustBotAccountStatusUpdCriteria : ResultCriteriaBase, INonQueryCommand
    {
        public CustBotAccountStatusUpdCriteria(int botId, int custId, int subAccId, bool status, bool isVerifyOTP, string ip)
        {
            BotId = botId;
            CustId = custId;
            SubAccId = subAccId;
            Status = status;
            IsVerifyOTP = isVerifyOTP;
            IP = ip;
        }

        public int BotId { get; }

        public int CustId { get; }

        public int SubAccId { get; }

        public bool Status { get; }

        public bool IsVerifyOTP { get; }

        public string IP { get; }

        public string GetSettingKey() => "CustBotAccountStatusUpd";

        public bool IsValid() => BotId > 0 && CustId > 0;
    }
}
