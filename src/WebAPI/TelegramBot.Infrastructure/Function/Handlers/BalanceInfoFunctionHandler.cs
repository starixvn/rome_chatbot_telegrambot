using System;
using System.Collections.Generic;
using System.Linq;
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
using Newtonsoft.Json;

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

            var action = parameter switch
            {
                "m" => HandleMyBalance(account),
                _ => HandleBalanceDownLineUsername(account, parameter)
            };

            return await action;
        }


        private async Task<Result> HandleTopXBalance(CustBotAccount account, int topQuery, string parameter)
        {
            if (topQuery <= 0 && topQuery > webAPIConfiguration.OutstandingMaxTopQuery)
            {
                return Result.CreateErrorResult(
                    messageBuilder.BuildInvalidNumerOfDownlineRange(account.Language, parameter,
                        webAPIConfiguration.OutstandingMaxTopQuery));
            }

            var topBalanceReport =
                await balanceInfoService.GetTopBalance(account.CustId, account.CustomerLevel, topQuery);

            var textContainer = new List<string>();
            textContainer.Add(BuildTitleRow(account.Language));

            textContainer.Add(NameLevelAccount(account.CustomerLevel, account.Language));
            textContainer.Add($"Today{messageFormatter.Point()}Yesterday");
            textContainer.Add(messageFormatter.NewLine());

            textContainer.Add("TOTAL:");
            textContainer.Add(
                $"      {topBalanceReport.Total.Balance.ToFormattedNumber()}{messageFormatter.Point()}{topBalanceReport.Total.YesterdayBalance.ToFormattedNumber()}");
            var index = 1;
            foreach (var item in topBalanceReport.TopBalances)
            {
                textContainer.Add($"{index++}.{item.Username}:");
                textContainer.Add(
                    $"      {item.Balance.ToFormattedNumber()}{messageFormatter.Point()}{item.YesterdayBalance.ToFormattedNumber()}");
            }

            if ((topBalanceReport.HiddenDownlinesCount) > 0)
            {
                textContainer.Add(
                    $"      {topBalanceReport.HiddenDownlinesCount} more {NameLevelAccount(account.CustomerLevel - 1, account.Language)}(s) not shown");
            }


            var message = string.Join(messageFormatter.NewLine(), textContainer);
            return Result.CreateSuccessfulResult(message);
        }

        private async Task<Result> HandleMyBalance(CustBotAccount account)
        {
            Fanex.Logging.Logger.Log.Info(JsonConvert.SerializeObject(account));
            var myBalance = await balanceInfoService.GetMyBalance(account.CustId, account.CustomerLevel);

            var dict = new Dictionary<string, string>
            {
                { "CURRENCY", myBalance.CurrencyName },
                { "CASH", myBalance.CashBalance.ToFormattedNumber() },
                { "YESTERDAY CASH", myBalance.CashBalance.ToFormattedNumber() },
                { "BALANCE", myBalance.TotalBalance.ToFormattedNumber() },
                { "YESTERDAY BAL:", myBalance.YesterdayTotalBalance.ToFormattedNumber() },
                { "TODAY WL:", myBalance.TodayWinLoss.ToFormattedNumber() },
                { "YESTERDAY WL:", myBalance.YesterdayWinLoss.ToFormattedNumber() },
                { "3RD PARTY PENDING TRANSFER:", myBalance.PendingTransfer3rd.ToFormattedNumber() },
                { "GIVEN CREDIT:", myBalance.GivenCredit.ToFormattedNumber() },
                {
                    $"{NameLevelAccount(account.CustomerLevel - 1, account.Language)} CREDIT:",
                    myBalance.MemberCredit.ToFormattedNumber()
                },
                { "MEMBER OUTSTANDING:", myBalance.TotalOutstanding.ToFormattedNumber() },
                {
                    $"{NameLevelAccount(account.CustomerLevel, account.Language)} OUTSTANDING:",
                    myBalance.Outstanding.ToFormattedNumber()
                },
            };

            var textContainer = new List<string>();
            textContainer.Add(BuildTitleRow(account.Language));

            foreach (var keyValuePair in dict)
            {
                textContainer.Add(keyValuePair.Key);
                textContainer.Add($"    {keyValuePair.Value}");
            }

            var message = string.Join(messageFormatter.NewLine(), textContainer);
            return Result.CreateSuccessfulResult(message);
        }


        private async Task<Result> HandleBalanceDownLineUsername(CustBotAccount account, string username)
        {
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
            textContainer.Add(BuildTitleRow(account.Language));

            textContainer.Add("USERNAME:");
            textContainer.Add(username);
            textContainer.Add(messageFormatter.NewLine());

            var dict = new Dictionary<string, string>();
            switch (downlineInfo.RoleId)
            {
                case 3:
                    dict = BuildAgentDownLineUsernameMessage(downlineBalance);
                    break;
                case 2:
                    dict = BuildMasterDownLineUsernameMessage(downlineBalance);
                    break;
                case 1:
                    dict = BuildMemberDownLineUsernameMessage(downlineBalance);
                    break;
            }

            foreach (var keyValuePair in dict)
            {
                textContainer.Add(keyValuePair.Key);
                textContainer.Add($"    {keyValuePair.Value}");
            }

            var message = string.Join(messageFormatter.NewLine(), textContainer);
            return Result.CreateSuccessfulResult(message);
        }

        public Dictionary<string, string> BuildMasterDownLineUsernameMessage(DownlineBalance downlineBalance)
        {
            var dictAgentLevel = new Dictionary<string, string>
            {
                { "CASH:", downlineBalance.TotalCash?.ToFormattedNumber() },
                { "YESTERDAY CASH:", downlineBalance.YesterdayCash?.ToFormattedNumber() },
                { "BALANCE:", downlineBalance.TotalBalance.ToFormattedNumber() },
                { "YESTERDAY BAL:", downlineBalance.YesterdayBalance.ToFormattedNumber() },
                { "MASTER OUTSTANDING:", downlineBalance.TotalOutstanding.ToFormattedNumber() },
                { "3RD PARTY PENDING TRANSFER:", downlineBalance.PendingTransfer3rd.ToFormattedNumber() },
                { "USED CREDIT:", downlineBalance.UsedCredit?.ToFormattedNumber() },
                { "GIVEN CREDIT:", downlineBalance.GivenCredit.ToFormattedNumber() },
                { "MEM. MAX CREDIT:", downlineBalance.MemberMaxCredit?.ToFormattedNumber() },
                {
                    "MEM.ACT/CLOSED/SUS:",
                    $"{BuildTotalACT_CLOSED_SUS(downlineBalance.TotalDownlines.ToList().Find(e => e.RoleId == 1))}"
                },
                { "MEMBER OUTSTANDING:", downlineBalance.TotalOutstanding.ToFormattedNumber() },
                { "SUPER OUTSTANDING:", downlineBalance.UplineOutstanding.ToFormattedNumber() },
            };
            return dictAgentLevel;
        }

        public Dictionary<string, string> BuildAgentDownLineUsernameMessage(DownlineBalance downlineBalance)
        {
            var dictAgentLevel = new Dictionary<string, string>
            {
                { "CASH:", downlineBalance.TotalCash?.ToFormattedNumber() },
                { "YESTERDAY CASH:", downlineBalance.YesterdayCash?.ToFormattedNumber() },
                { "BALANCE:", downlineBalance.TotalBalance.ToFormattedNumber() },
                { "YESTERDAY BAL:", downlineBalance.YesterdayBalance.ToFormattedNumber() },
                { "AGENT OUTSTANDING:", downlineBalance.TotalOutstanding.ToFormattedNumber() },
                { "3RD PARTY PENDING TRANSFER:", downlineBalance.PendingTransfer3rd.ToFormattedNumber() },
                { "USED CREDIT:", downlineBalance.UsedCredit?.ToFormattedNumber() },
                { "GIVEN CREDIT:", downlineBalance.GivenCredit.ToFormattedNumber() },
                { "MEM. MAX CREDIT:", downlineBalance.MemberMaxCredit?.ToFormattedNumber() },
                {
                    "MEM.ACT/CLOSED/SUS:",
                    $"{BuildTotalACT_CLOSED_SUS(downlineBalance.TotalDownlines.ToList().Find(e => e.RoleId == 1))}"
                },
                { "MEMBER OUTSTANDING:", downlineBalance.TotalOutstanding.ToFormattedNumber() },
                {
                    $"MASTER OUTSTANDING:",
                    downlineBalance.UplineOutstanding.ToFormattedNumber()
                },
            };
            return dictAgentLevel;
        }

        public Dictionary<string, string> BuildMemberDownLineUsernameMessage(DownlineBalance downlineBalance)
        {
            var dictMemberLevel = new Dictionary<string, string>
            {
                { "BALANCE:", downlineBalance.TotalBalance.ToFormattedNumber() },
                { "YESTERDAY BAL:", downlineBalance.YesterdayBalance.ToFormattedNumber() },
                { "AGENT OUTSTANDING:", downlineBalance.TotalOutstanding.ToFormattedNumber() },
                { "3RD PARTY PENDING TRANSFER:", downlineBalance.PendingTransfer3rd.ToFormattedNumber() },
                { "USED CREDIT:", downlineBalance.UsedCredit?.ToFormattedNumber() },
                { "GIVEN CREDIT:", downlineBalance.GivenCredit.ToFormattedNumber() },
                { "MEM. MAX CREDIT:", downlineBalance.MemberMaxCredit?.ToFormattedNumber() },
                {
                    "MEM.ACT/CLOSED/SUS:",
                    $"{BuildTotalACT_CLOSED_SUS(downlineBalance.TotalDownlines.ToList().Find(e => e.RoleId == 1))}"
                },
                { "MEMBER OUTSTANDING:", downlineBalance.TotalOutstanding.ToFormattedNumber() },
                {
                    "MASTER OUTSTANDING:", downlineBalance.UplineOutstanding.ToFormattedNumber()
                },
            };
            return dictMemberLevel;
        }

        private string BuildTitleRow(string language)
            => $"[{ResourceHelper.Get(nameof(SharedResource.Balance), language)} - {DateTime.Now:yyyyMMdd}]";


        public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => await balanceInfoService.GetTopUsername(account.CustId, account.CustomerLevel, top);

        private string NameLevelAccount(int level, string language) =>
            ResourceHelper.Get($"CustLevel_{level - 1}", language);

        private string BuildTotalACT_CLOSED_SUS(TotalDownline downline)
            => $"{downline.TotalActive}/{downline.TotalClosed}/{downline.TotalSuspended}";
    }
};