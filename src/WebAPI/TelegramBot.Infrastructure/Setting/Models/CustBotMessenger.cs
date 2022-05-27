using System.Collections.Generic;
using System.Linq;
using TelegramBot.Infrastructure.Function.Models;
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
            string system = "",
            bool enabled = false,
            bool isVerifyOTP = false,
            string language = LanguageCodes.English)
        {
            UserId = userId;
            BotId = botId;
            BotName = botName;
            System = system;
            ChatId = chatId;
            Enabled = enabled;
            IsVerifyOTP = isVerifyOTP;
            Language = language;
        }

        public int UserId { get; }

        public int BotId { get; }

        public string BotName { get; }

        public string System { get; }

        public long ChatId { get; }

        public bool Enabled { get; }

        public bool IsVerifyOTP { get; }

        public string Language { get; }

        public IEnumerable<BotFunction> Functions { get; set; }

        public bool IsFunctionAvailable(short functionId)
            => Functions.Any(f => f.FunctionId == functionId && f.Enabled);
    }
}