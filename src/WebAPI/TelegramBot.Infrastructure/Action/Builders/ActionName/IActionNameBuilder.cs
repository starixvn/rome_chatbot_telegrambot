using TelegramBot.Infrastructure.Action.Models;
using TelegramBot.Infrastructure.Setting.Models;

namespace TelegramBot.Infrastructure.Action.Builders.ActionName
{
    public interface IActionNameBuilder
    {
        Task<string> BuildActionName(BotAction action, CustBotAccount account);
    }
}