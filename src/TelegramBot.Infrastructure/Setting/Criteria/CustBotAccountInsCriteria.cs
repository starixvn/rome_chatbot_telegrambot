using Fanex.Data.Repository;

namespace TelegramBot.Infrastructure.Setting.Criteria
{
    public class CustBotAccountInsCriteria : INonQueryCommand
    {
        public CustBotAccountInsCriteria(int botId, long chatId, int custId, int subAccId, string messengerName, string language)
        {
            BotId = botId;
            ChatId = chatId;
            CustId = custId;
            SubAccId = subAccId;
            MessengerName = messengerName;
            Language = language;
        }

        public int BotId { get; }

        public long ChatId { get; }

        public int CustId { get; }

        public int SubAccId { get; }

        public string MessengerName { get; }

        public string Language { get; }

        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        public string GetSettingKey() => "CustBotAccountIns";

        public bool IsValid() => BotId > 0 && ChatId != 0 && CustId > 0;
    }
}
