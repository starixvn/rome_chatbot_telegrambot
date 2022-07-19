using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Models;

namespace TelegramBot.Infrastructure.Action.Validators
{
    public interface IActionValidator
    {
        Task<Result> Validate(CustBotAccount account, string userResponse);
    }
}