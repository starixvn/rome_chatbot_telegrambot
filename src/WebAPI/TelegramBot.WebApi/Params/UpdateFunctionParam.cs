using TelegramBot.Infrastructure.Setting.Models;

namespace TelegramBot.WebApi.Params
{
    public class UpdateFunctionParam
    {
        public int Creator { get; set; }

        public Function Function { get; set; }

        public string IP { get; set; }
    }
}