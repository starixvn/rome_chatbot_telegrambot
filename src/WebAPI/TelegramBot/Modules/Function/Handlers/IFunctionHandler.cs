using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Models;
using TelegramBot.Modules.Bot.Models;

namespace TelegramBot.Modules.Function.Handlers
{
    public interface IFunctionHandler
    {
        Task<Result> Handle(CustBotAccount account, string[] parameters, BotEntity bot);

        Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5);
    }
}