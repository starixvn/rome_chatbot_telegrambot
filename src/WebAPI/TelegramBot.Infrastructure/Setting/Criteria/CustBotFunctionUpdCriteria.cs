using Fanex.Data.Repository;
using TelegramBot.Infrastructure.Shared;

namespace TelegramBot.Infrastructure.Setting.Criteria
{
    public class CustBotFunctionUpdCriteria : ResultCriteriaBase, INonQueryCommand
    {
        public CustBotFunctionUpdCriteria(int custId, int subAccId, string setting, int creator, string ip)
        {
            CustId = custId;
            SubAccId = subAccId;
            Setting = setting;
            Creator = creator;
            IP = ip;
        }

        public int CustId { get; }

        public int SubAccId { get; } = 0;

        public string Setting { get; }

        public int Creator { get; }

        public string IP { get; }

        public string GetSettingKey() => "CustBotFunctionUpd";

        public bool IsValid() => CustId > 0 && !string.IsNullOrEmpty(Setting);
    }
}