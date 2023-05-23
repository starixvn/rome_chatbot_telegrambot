using Fanex.Data.Repository;

namespace TelegramBot.Modules.Setting.Criteria
{
    public class CustBotUserSelCriteria : CriteriaBase
    {
        public CustBotUserSelCriteria(long chatId, int botId, bool isTest)
        {
            ChatId = chatId;
            BotId = botId;
            IsTest = isTest;
        }

        public long ChatId { get; }

        public int BotId { get; }

        public bool IsTest { get; }

        public override string GetSettingKey() => "CustBotUserSel";

        public override bool IsValid() => ChatId > 0;
    }
}