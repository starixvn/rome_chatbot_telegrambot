using TelegramBot.Modules.Function.Models;
using TelegramBot.Shared.Constants;

namespace TelegramBot.Modules.Setting.Models
{
    public class CustBotMessenger
    {
        public int UserId { get; init; }

        public int BotId { get; init; }

        public string BotName { get; init; } = string.Empty;

        public string System { get; init; } = string.Empty;

        public long ChatId { get; init; }

        public bool Enabled { get; init; }

        public bool IsVerifyOTP { get; init; }

        public string Language { get; init; } = LanguageCodes.English;

        public byte CustomerLevel { get; init; }

        public IEnumerable<BotFunction> Functions { get; set; }

        public bool IsFunctionAvailable(short functionId)
            => Functions.Any(f => f.FunctionId == functionId && f.Enabled);
    }
}