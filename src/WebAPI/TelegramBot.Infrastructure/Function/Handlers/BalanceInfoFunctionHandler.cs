using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alpha.API.Client.Models.BalanceInfo;
using Alpha.API.Client.Services;
using TelegramBot.Infrastructure.Action;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.Infrastructure.Shared.Extensions;
using TelegramBot.Infrastructure.Shared.Resources;

namespace TelegramBot.Infrastructure.Function.Handlers
{
    public class BalanceInfoFunctionHandler : BaseFunctionHandler, IFunctionHandler
    {
        private readonly WebAPIConfiguration webAPIConfiguration;
        private readonly IBalanceInfoService balanceInfoService;
        private readonly IActionService actionService;
        private readonly ICustomerService customerService;

        public BalanceInfoFunctionHandler(
            IMessageBuilder messageBuilder,
            IMessageFormatter messageFormatter,
            ICustomerService customerService,
            IBalanceInfoService balanceInfoService,
            WebAPIConfiguration webAPIConfiguration,
            IActionService actionService
        ) :
            base(messageBuilder, messageFormatter)
        {
            this.balanceInfoService = balanceInfoService;
            this.customerService = customerService;
            this.webAPIConfiguration = webAPIConfiguration;
            this.actionService = actionService;
        }


        public async Task<Result> Handle(CustBotAccount account, string[] parameters)
        {
            var parameter = parameters[0];
            var isNumber = int.TryParse(parameter, out int topQuery);
            if (isNumber)
            {
                return await HandleTopXBalance(account, topQuery, parameter);
            }

            switch (parameter)
            {
                case null:
                case "":
                    return await HandleBalanceInfo(account);
                case "m":
                    return await HandleMyBalance(account);
                default:
                    return await HandleBalanceDownLineUsername(account, parameter);
            }
        }

        private async Task<Result> HandleTopXBalance(CustBotAccount account, int topQuery, string parameter)
        {
            if (topQuery <= 0 && topQuery > webAPIConfiguration.OutstandingMaxTopQuery)
            {
                return Result.CreateErrorResult(
                    messageBuilder.BuildInvalidNumerOfDownlineRange(account.Language, parameter,
                        webAPIConfiguration.OutstandingMaxTopQuery));
            }

            var data = await balanceInfoService.GetTopBalance(account.CustId, account.CustomerLevel, topQuery);
            return Result.CreateSuccessfulResult("you select /bal number");
        }

        private async Task<Result> HandleMyBalance(CustBotAccount account)
        {
            var myBalance = await balanceInfoService.GetMyBalance(account.CustId, account.CustomerLevel);

            var textContainer = new List<string>();
            AddTitleRow(textContainer, account.Language);
            textContainer.Add(messageFormatter.NewLine());

            const int tabCharacter = 15;

            textContainer.Add("CURRENCY:");
            textContainer.Add(myBalance.CurrencyName.PadLeft(tabCharacter));

            textContainer.Add("CASH:");
            textContainer.Add(myBalance.CashBalance.ToFormattedNumber().PadLeft(tabCharacter));

            textContainer.Add("YESTERDAY CASH:");
            textContainer.Add(myBalance.CashBalance.ToFormattedNumber().PadLeft(tabCharacter));

            textContainer.Add("BALANCE:");
            textContainer.Add(myBalance.TotalBalance.ToFormattedNumber().PadLeft(tabCharacter));

            textContainer.Add("YESTERDAY BAL:");
            textContainer.Add(myBalance.YesterdayTotalBalance.ToFormattedNumber().PadLeft(tabCharacter));

            textContainer.Add("TODAY WL:");
            textContainer.Add(myBalance.TodayWinLoss.ToFormattedNumber().PadLeft(tabCharacter));

            textContainer.Add("YESTERDAY WL:");
            textContainer.Add(myBalance.YesterdayWinLoss.ToFormattedNumber().PadLeft(tabCharacter));

            textContainer.Add("3RD PARTY PENDING TRANSFER:");
            textContainer.Add("I DONT KNOW ahuahu".PadLeft(tabCharacter));

            textContainer.Add("GIVEN CREDIT:");
            textContainer.Add(myBalance.GivenCredit.ToFormattedNumber().PadLeft(tabCharacter));

            textContainer.Add("[Direct Downline level] CREDIT:");
            textContainer.Add("I DONT KNOW ahuahu".PadLeft(tabCharacter));

            textContainer.Add("MEMBER OUTSTANDING:");
            textContainer.Add("I DONT KNOW ahuahu".PadLeft(tabCharacter));

            textContainer.Add("[User's level] OUTSTANDING:");
            textContainer.Add("I DONT KNOW ahuahu".PadLeft(tabCharacter));

            var message = string.Join(messageFormatter.NewLine(), textContainer);
            return Result.CreateSuccessfulResult(message);
        }

        private async Task<Result> HandleBalanceDownLineUsername(CustBotAccount account, string username)
        {
            // 1176070 => custID role acc supper => =>4
            var downlineInfo = await customerService.GetDownlineInfo(account.CustId, username);
            if (!IsValidDirectDownline(account.CustomerLevel, downlineInfo))
            {
                return Result.CreateErrorResult(
                    messageBuilder.BuildInvalidCustNameMessage(account.Language, username));
            }

            var downlineBalance = await balanceInfoService.GetDownlineBalance(account.CustId,
                account.CustomerLevel,
                downlineInfo.CustId);

            var textContainer = new List<string>();
            AddTitleRow(textContainer, account.Language);
            
            const int tabCharacter = 15;
            
            textContainer.Add(messageFormatter.NewLine());
            textContainer.Add("CASH:");
            textContainer.Add(downlineBalance.TotalCash?.ToFormattedNumber().PadLeft(tabCharacter));

            textContainer.Add("YESTERDAY CASH:");
            textContainer.Add(downlineBalance.YesterdayCash?.ToFormattedNumber().PadLeft(tabCharacter));

            textContainer.Add("BALANCE:");
            textContainer.Add(downlineBalance.TotalBalance.ToFormattedNumber().PadLeft(tabCharacter));

            textContainer.Add("YESTERDAY BAL");
            textContainer.Add(downlineBalance.YesterdayBalance.ToFormattedNumber().PadLeft(tabCharacter));

            textContainer.Add("OUTSTANDING:");
            textContainer.Add(downlineBalance.TotalOutstanding.ToFormattedNumber().PadLeft(tabCharacter));

            textContainer.Add("3RD PARTY PENDING TRANSFER:");
            textContainer.Add(downlineBalance.PendingTransfer3rd.ToFormattedNumber().PadLeft(tabCharacter));

            textContainer.Add("USED CREDIT:");
            textContainer.Add(downlineBalance.UsedCredit?.ToFormattedNumber().PadLeft(tabCharacter));

            textContainer.Add("GIVEN CREDIT:");
            textContainer.Add(downlineBalance.GivenCredit.ToFormattedNumber().PadLeft(tabCharacter));

            textContainer.Add("AG.ACT/CLOSED/SUS:");
            textContainer.Add("I DONT KNOW ahuahu".PadLeft(tabCharacter));
            
            textContainer.Add("MEM.ACT/CLOSED/SUS:");
            textContainer.Add("I DONT KNOW ahuahu".PadLeft(tabCharacter));

            textContainer.Add("MEMBER OUTSTANDING:");
            textContainer.Add("I DONT KNOW ahuahu".PadLeft(tabCharacter));

            textContainer.Add("[User's level] OUTSTANDING:");
            textContainer.Add("I DONT KNOW ahuahu".PadLeft(tabCharacter));

            var message = string.Join(messageFormatter.NewLine(), textContainer);

            return Result.CreateSuccessfulResult(message);
        }

        private async Task<Result> HandleBalanceInfo(CustBotAccount account)
        {
            var actionSelectedMessage = messageBuilder.BuildActionSelectedMessage(account.Language,
                await messageBuilder.BuildActionName(account.Language, 2));
            var pleaseSelectNextAction = messageBuilder.BuildMenuMessage(account.Language);

            var textContainer = new List<string>();
            textContainer.Add(actionSelectedMessage);
            textContainer.Add(pleaseSelectNextAction);

            return Result.CreateSuccessfulResult(string.Join(messageFormatter.NewLine(), textContainer));
        }

        private void AddTitleRow(List<string> textContainer, string language)
            => textContainer.Add(
                $"[{ResourceHelper.Get(nameof(SharedResource.Balance), language)} - {DateTime.Now:yyyyMMdd}]");


        public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => await balanceInfoService.GetTopUsername(account.CustId, account.CustomerLevel, top);
    }
};