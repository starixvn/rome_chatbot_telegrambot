using System.Collections.Generic;

namespace TelegramBot.Infrastructure.Setting.Models
{
    public class CustBotMessenger
    {
        public CustBotMessenger(int userId, int botId, string botName, long chatId, bool status, bool isVerifyOTP, string language)
        {
            UserId = userId;
            BotId = botId;
            BotName = botName;
            ChatId = chatId;
            Status = status;
            IsVerifyOTP = isVerifyOTP;
            Language = language;
        }

        public int UserId { get; }

        public int BotId { get; }

        public string BotName { get; }

        public long ChatId { get; }

        public bool Status { get; }

        public bool IsVerifyOTP { get; }

        public string Language { get; }

        public IEnumerable<Function> Functions { get; set; }
    }
}