using Alpha.API.Client.Services;
using TelegramBot.Modules.Message;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Models;
using TelegramBot.Shared.Resources;
using TelegramBot.Utilities.Extensions;

namespace TelegramBot.Modules.Function.Handlers
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

        protected override void AddMyWinLossTitleRow(List<string> textContainer, string language)
            => textContainer.Add($"[{ResourceHelper.Get(nameof(SharedResource.MyTodayWinLoss), language)} - {DateTime.Today.ToSimpleDate()}]");

        public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => await winLossService.GetTopUsername(account.CustId, account.CustomerLevel, DateTime.Today, top);

        protected override void AddDownlineWinLossTitleRow(List<string> textContainer, string language)
            => textContainer.Add($"[{ResourceHelper.Get(nameof(SharedResource.DownlineTodayWinLoss), language)} - {DateTime.Today.ToSimpleDate()}]");
    }
}