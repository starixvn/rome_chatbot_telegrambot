using TelegramBot.Shared.Models;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Modules.Message;
using Alpha.API.Client.Services;
using Alpha.API.Client.Models.Customers;
using TelegramBot.Shared.Resources;
using System.Text;

namespace TelegramBot.Modules.Function.Handlers
{
    public class TotalBetsFunctionHandler : BaseFunctionHandler, IFunctionHandler
    {
        private readonly ICustomerService customerService;

        public TotalBetsFunctionHandler(
            IMessageBuilder messageBuilder,
            IMessageFormatter messageFormatter,
            ICustomerService customerService) : base(messageBuilder, messageFormatter)
            => this.customerService = customerService;

        public async Task<Result> Handle(CustBotAccount account, string[] parameters)
        {
            return new Result(0, $"Total Bets Page");
        }
        
        public Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => throw new NotImplementedException();
    }
}