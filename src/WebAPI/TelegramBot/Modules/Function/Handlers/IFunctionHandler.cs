using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Models;

namespace TelegramBot.Modules.Function.Handlers
{
    public interface IFunctionHandler
    {
        Task<Result> Handle(CustBotAccount account, string[] parameters);

        Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5);
    }
}