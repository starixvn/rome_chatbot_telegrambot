using TelegramBot.Infrastructure.Function.Models;
using TelegramBot.Infrastructure.Shared.Constants;

namespace TelegramBot.Infrastructure.Setting.Models
{
    public class CustBotAccount
    {
        public int UserId { get; set; }

        public int CustId { get; set; }

        public int SubAccId { get; set; }

        public byte CustomerLevel { get; set; }

        public string Language { get; set; }

        public IEnumerable<BotFunction> Functions { get; set; }
    }
}