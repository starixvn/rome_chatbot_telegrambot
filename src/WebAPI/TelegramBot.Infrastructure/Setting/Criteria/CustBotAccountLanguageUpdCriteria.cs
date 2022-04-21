using Fanex.Data.Repository;
using TelegramBot.Infrastructure.Shared;

namespace TelegramBot.Infrastructure.Setting.Criteria
{
    public class CustBotAccountLanguageUpdCriteria : ResultCriteriaBase, INonQueryCommand
    {
        public CustBotAccountLanguageUpdCriteria(int userId, string language, string ip)
        {
            UserId = userId;
            Language = language;
            IP = ip;
        }

        public int UserId { get; }

        public string Language { get; }

        public string IP { get; }

        public string GetSettingKey() => "CustBotAccountLanguageUpd";

        public bool IsValid() => UserId > 0 && !string.IsNullOrWhiteSpace(Language);
    }
}