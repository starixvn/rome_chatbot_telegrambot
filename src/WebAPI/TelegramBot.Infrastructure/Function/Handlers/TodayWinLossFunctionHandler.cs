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
    public class TodayWinLossFunctionHandler : WinLossFunctionHandler, IFunctionHandler
    {
        public TodayWinLossFunctionHandler(
            IMessageBuilder messageBuilder,
            IMessageFormatter messageFormatter,
            IWinLossService winLossService,
            ICustomerService customerService,
            WebAPIConfiguration webAPIConfiguration)
            : base(messageBuilder, messageFormatter, winLossService, customerService, webAPIConfiguration)
        {
        }

        public Task<Result> Handle(CustBotAccount account, string[] parameters)
         => HandleWinLoss(account, parameters, DateTime.Now);

        protected override void AddTitleRow(List<string> textContainer, string language)
            => textContainer.Add($"[{ResourceHelper.Get(nameof(SharedResource.TodayWinLoss), language)} - {DateTime.Now:yyyyMMdd}]");

        public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => await winLossService.GetTopUsername(account.CustId, account.CustomerLevel, DateTime.Now, top);
    }
}