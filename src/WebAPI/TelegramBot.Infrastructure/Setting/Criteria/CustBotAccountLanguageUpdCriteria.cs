using Fanex.Data.Repository;
using TelegramBot.Infrastructure.Shared;

namespace TelegramBot.Infrastructure.Setting.Criteria
{
    public class CustBotAccountLanguageUpdCriteria : ResultCriteriaBase, INonQueryCommand
    {
        public CustBotAccountLanguageUpdCriteria(int userId, int botId, string language, int creator, string ip)
        {
            UserId = userId;
            BotId = botId;
            Language = language;
            Creator = creator;
            IP = ip;
        }

        public int UserId { get; }

        public int BotId { get; }

        public string Language { get; }

        public int Creator { get; }

        public string IP { get; }

        public string GetSettingKey() => "CustBotAccountLanguageUpd";

        public bool IsValid() => UserId > 0 && !string.IsNullOrWhiteSpace(Language);
    }
}