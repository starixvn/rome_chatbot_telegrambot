using Fanex.Data.Repository;

namespace TelegramBot.Infrastructure.Setting.Criteria
{
    public class CustBotUserSelCriteria : CriteriaBase
    {
        public CustBotUserSelCriteria(long chatId, int botId)
        {
            ChatId = chatId;
            BotId = botId;
        }

        public long ChatId { get; }

        public int BotId { get; }

        public override string GetSettingKey() => "CustBotUserSel";

        public override bool IsValid() => ChatId > 0 && BotId > 0;
    }
}