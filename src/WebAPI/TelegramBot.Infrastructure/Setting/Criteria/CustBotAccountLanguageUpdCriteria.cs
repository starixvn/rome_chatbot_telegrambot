using Fanex.Data.Repository;
using TelegramBot.Infrastructure.Shared;

namespace TelegramBot.Infrastructure.Setting.Criteria
{
    public class CustBotAccountLanguageUpdCriteria : ResultCriteriaBase, INonQueryCommand
    {
        public CustBotAccountLanguageUpdCriteria(int custId, int subAccId, string language, string ip)
        {
            CustId = custId;
            SubAccId = subAccId;
            Language = language;
            IP = ip;
        }

        public int CustId { get; }

        public int SubAccId { get; }

        public string Language { get; }

        public string IP { get; }

        public string GetSettingKey() => "CustBotAccountLanguageUpd";

        public bool IsValid() => CustId > 0 && !string.IsNullOrWhiteSpace(Language);
    }
}
