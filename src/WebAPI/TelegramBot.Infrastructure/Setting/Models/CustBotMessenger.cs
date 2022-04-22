using System.Collections.Generic;
using TelegramBot.Infrastructure.Shared.Constants;

namespace TelegramBot.Infrastructure.Setting.Models
{
    public class CustBotMessenger
    {
        public CustBotMessenger()
        {
        }

        public CustBotMessenger(
            int userId,
            int botId,
            long chatId,
            string botName = "",
            bool enabled = false,
            bool isVerifyOTP = false,
            string language = LanguageCodes.English)
        {
            UserId = userId;
            BotId = botId;
            BotName = botName;
            ChatId = chatId;
            Enabled = enabled;
            IsVerifyOTP = isVerifyOTP;
            Language = language;
        }

        public int UserId { get; }

        public int BotId { get; }

        public string BotName { get; }

        public long ChatId { get; }

        public bool Enabled { get; }

        public bool IsVerifyOTP { get; }

        public string Language { get; }

        public IEnumerable<Function> Functions { get; set; }
    }
}