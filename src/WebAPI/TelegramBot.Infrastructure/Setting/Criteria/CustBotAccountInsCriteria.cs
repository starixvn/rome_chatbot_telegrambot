using Fanex.Data.Repository;
using TelegramBot.Infrastructure.Shared;

namespace TelegramBot.Infrastructure.Setting.Criteria
{
    public class CustBotAccountInsCriteria : ResultCriteriaBase, INonQueryCommand
    {
        public CustBotAccountInsCriteria(int botId, long chatId, int custId, int subAccId, string language, string ip)
        {
            BotId = botId;
            ChatId = chatId;
            CustId = custId;
            SubAccId = subAccId;
            Language = language;
            IP = ip;
        }

        public int BotId { get; }

        public long ChatId { get; }

        public int CustId { get; }

        public int SubAccId { get; }

        public string Language { get; }

        public string IP { get; }

        public int UserId { get; set; }

        public string GetSettingKey() => "CustBotAccountIns";

        public bool IsValid() => BotId > 0 && ChatId != 0 && CustId > 0;
    }
}