using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alpha.API.Client.Services;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.Infrastructure.Shared.Resources;

namespace TelegramBot.Infrastructure.Function.Handlers
{
    public class YesterdayWinLossFunctionHandler : BaseWinLossFunctionHandler, IFunctionHandler
    {
        public YesterdayWinLossFunctionHandler(
            IMessageBuilder messageBuilder,
            IMessageFormatter messageFormatter,
            IWinLossService winLossService,
            ICustomerService customerService,
            WebAPIConfiguration webAPIConfiguration)
            : base(messageBuilder, messageFormatter, winLossService, customerService, webAPIConfiguration)
        {
        }

        public Task<Result> Handle(CustBotAccount account, string[] parameters)
            => HandleWinLoss(account, parameters, DateTime.Today.AddDays(-1));

        protected override void AddTitleRow(List<string> textContainer, string language)
            => textContainer.Add($"[{ResourceHelper.Get(nameof(SharedResource.YesterdayWinLoss), language)} - {DateTime.Today.AddDays(-1):yyyyMMdd}]");

        public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => await winLossService.GetTopUsername(account.CustId, account.CustomerLevel, DateTime.Today.AddDays(-1), top);
    }
}