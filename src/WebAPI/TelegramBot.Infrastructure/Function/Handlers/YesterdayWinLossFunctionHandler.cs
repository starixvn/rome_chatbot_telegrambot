using Alpha.API.Client.Services;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.Infrastructure.Shared.Resources;
using TelegramBot.Utilities.Extensions;

namespace TelegramBot.Infrastructure.Function.Handlers
{
    public class YesterdayWinLossFunctionHandler : BaseWinLossFunctionHandler, IFunctionHandler
    {
        public YesterdayWinLossFunctionHandler(
            IMessageBuilder messageBuilder,
            IMessageFormatter messageFormatter,
            IWinLossService winLossService,
            ICustomerService customerService)
            : base(messageBuilder, messageFormatter, winLossService, customerService)
        {
        }

        public Task<Result> Handle(CustBotAccount account, string[] parameters)
            => HandleWinLoss(account, parameters, DateTime.Today.AddDays(-1));

        protected override void AddMyWinLossTitleRow(List<string> textContainer, string language)
            => textContainer.Add($"[{ResourceHelper.Get(nameof(SharedResource.MyYesterdayWinLoss), language)} - {DateTime.Today.AddDays(-1).ToSimpleDate()}]");

        public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => await winLossService.GetTopUsername(account.CustId, account.CustomerLevel, DateTime.Today.AddDays(-1), top);

        protected override void AddDownlineWinLossTitleRow(List<string> textContainer, string language)
            => textContainer.Add($"[{ResourceHelper.Get(nameof(SharedResource.DownlineYesterdayWinLoss), language)} - {DateTime.Today.AddDays(-1).ToSimpleDate()}]");
    }
}