using Alpha.API.Client.Services;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.Infrastructure.Shared.Resources;

namespace TelegramBot.Infrastructure.Function.Handlers
{
    public class TodayWinLossFunctionHandler : BaseWinLossFunctionHandler, IFunctionHandler
    {
        public TodayWinLossFunctionHandler(
            IMessageBuilder messageBuilder,
            IMessageFormatter messageFormatter,
            IWinLossService winLossService,
            ICustomerService customerService)
            : base(messageBuilder, messageFormatter, winLossService, customerService)
        {
        }

        public Task<Result> Handle(CustBotAccount account, string[] parameters)
            => HandleWinLoss(account, parameters, DateTime.Today);

        protected override void AddTitleRow(List<string> textContainer, string language)
            => textContainer.Add($"[{ResourceHelper.Get(nameof(SharedResource.TodayWinLoss), language)} - {DateTime.Today:yyyyMMdd}]");

        public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => await winLossService.GetTopUsername(account.CustId, account.CustomerLevel, DateTime.Today, top);
    }
}