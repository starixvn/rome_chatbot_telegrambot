using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Models;

namespace TelegramBot.Modules.Action.Validators
{
    public interface IActionValidator
    {
        Task<Result> Validate(CustBotAccount account, string userResponse);
    }
}