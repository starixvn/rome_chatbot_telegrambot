using TelegramBot.Modules.Action.Models;
using TelegramBot.Modules.Setting.Models;

namespace TelegramBot.Modules.Action.Builders.ActionName
{
    public interface IActionNameBuilder
    {
        Task<string> BuildActionName(BotAction action, CustBotAccount account);
    }
}