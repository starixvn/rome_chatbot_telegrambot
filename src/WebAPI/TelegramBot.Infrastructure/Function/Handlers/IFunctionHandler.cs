using TelegramBot.Infrastructure.Bot.Models;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Models;

namespace TelegramBot.Infrastructure.Function.Handlers
{
    public interface IFunctionHandler
    {
        Task<Result> Handle(CustBotAccount account, string[] parameters, BotEntity bot);

        Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5);
    }
}