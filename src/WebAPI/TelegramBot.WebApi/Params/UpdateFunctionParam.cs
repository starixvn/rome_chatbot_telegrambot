using TelegramBot.Infrastructure.Function.Models;

namespace TelegramBot.WebApi.Params
{
    public class UpdateFunctionParam
    {
        public int Creator { get; set; }

        public int BotId { get; set; }

        public BotFunction Function { get; set; }

        public string IP { get; set; }
    }
}