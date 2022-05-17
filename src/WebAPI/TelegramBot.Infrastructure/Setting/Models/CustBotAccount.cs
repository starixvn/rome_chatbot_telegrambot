using System.Collections.Generic;
using TelegramBot.Infrastructure.Function.Models;

namespace TelegramBot.Infrastructure.Setting.Models
{
    public class CustBotAccount
    {

        public int UserId { get; set; }

        public int CustId { get; set; }

        public int SubAccId { get; set; }

        public byte CustomerLevel { get; set; }

        public string Language { get; set; }

        public bool IsSubAccount => SubAccId > 0;


        public IEnumerable<BotFunction> Functions { get; set; }
    }
}