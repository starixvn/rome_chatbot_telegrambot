using TelegramBot.Modules.Function.Models;

namespace TelegramBot.Modules.Setting.Models
{
    public class CustBotAccount
    {
        public int UserId { get; set; }

        public int CustId { get; set; }

        public int SubAccId { get; set; }

        public byte CustomerLevel { get; set; }

        public string Language { get; set; }

        public int BotId { get; set; }

        public string System { get; set; }

        public string Username { get; set; }

        public IEnumerable<BotFunction> Functions { get; set; }
    }
}