using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramBot.Infrastructure.Setting.Models;

namespace TelegramBot.Infrastructure.Function.Handlers
{
    public interface IFunctionHandler
    {
        Task Handle(CustBotAccount account, CustBotMessenger messenger, string[] parameters);

        Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5);
    }
}