using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alpha.API.Client.Models.BalanceInfo;
using Alpha.API.Client.Services;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.Infrastructure.Shared.Extensions;
using TelegramBot.Infrastructure.Shared.Resources;

namespace TelegramBot.Infrastructure.Function.Handlers
{
    public enum AccountLevel
    {
        Member = 1,
        Agent = 2,
        Master = 3,
        Supper = 4
    }

    public class BalanceInfoFunctionHandler : BaseFunctionHandler, IFunctionHandler
    {
        private readonly WebAPIConfiguration webAPIConfiguration;
        private readonly IBalanceInfoService balanceInfoService;
        private readonly ICustomerService customerService;

        public BalanceInfoFunctionHandler(
            IMessageBuilder messageBuilder,
            IMessageFormatter messageFormatter,
            ICustomerService customerService,
            IBalanceInfoService balanceInfoService,
            WebAPIConfiguration webAPIConfiguration
        ) :
            base(messageBuilder, messageFormatter)
        {
            this.balanceInfoService = balanceInfoService;
            this.customerService = customerService;
            this.webAPIConfiguration = webAPIConfiguration;
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

            textContainer.Add(ResourceHelper.Get($"CustLevel_{account.CustomerLevel - 1}", account.Language));

            textContainer.Add(
                $"{ResourceHelper.Get(nameof(SharedResource.Today), account.Language)}{messageFormatter.Point()}{ResourceHelper.Get(nameof(SharedResource.Yesterday), account.Language)}{messageFormatter.NewLine()}");

            textContainer.Add(ResourceHelper.Get(nameof(SharedResource.Total), account.Language).ToUpperInvariant());
            textContainer.Add(
                $"    {topBalanceReport.Total.Balance.ToFormattedNumber()}{messageFormatter.Point()}{topBalanceReport.Total.YesterdayBalance.ToFormattedNumber()}");
            var index = 1;
            foreach (var item in topBalanceReport.TopBalances)
            {
                textContainer.Add($"{index++}.{item.Username}:");
                textContainer.Add(
                    $"    {item.Balance.ToFormattedNumber()}{messageFormatter.Point()}{item.YesterdayBalance.ToFormattedNumber()}");
            }

            if (topBalanceReport.HiddenDownlinesCount > 0)
            {
                textContainer.Add(messageBuilder.BuildHiddenDownlinesMessage(account.Language,
                    topBalanceReport.HiddenDownlinesCount, (byte)(account.CustomerLevel - 1)));
            }

            var message = string.Join(messageFormatter.NewLine(), textContainer);
            return Result.CreateSuccessfulResult(message);
        }

        private async Task<Result> HandleMyBalance(CustBotAccount account)
        {
            var bal = await balanceInfoService.GetMyBalance(account.CustId, account.CustomerLevel);
            var lang = account.Language;
            var dict = new Dictionary<string, string>
            {
            };
            var textContainer = new List<string>();

            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.Currency), lang)}:", bal.CurrencyName);
            AddCashMessage(dict, lang, bal.CashBalance, bal.YesterdayCashBalance);
            AddBalMessage(dict, lang, bal.TotalBalance, bal.YesterdayTotalBalance);
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.TodayWinLoss), lang)}:",
                bal.TodayWinLoss.ToFormattedNumber());
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.YesterdayWinLoss), lang)}:",
                bal.YesterdayWinLoss.ToFormattedNumber());
            AddPendingTransfer3rdMessage(dict, bal.PendingTransfer3rd, lang);
            AddCreditMessage(dict, lang, nameof(SharedResource.GivenCredit), bal.GivenCredit);
            AddCreditMessage(dict, lang, $"{(AccountLevel)account.CustomerLevel - 1}Credit",
                bal.MemberCredit);
            AddOutStandingMessage(dict, lang, nameof(SharedResource.MemberOutStanding), bal.TotalOutstanding);
            AddOutStandingMessage(dict, lang, $"{(AccountLevel)account.CustomerLevel}OutStanding",
                bal.Outstanding);

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
            var level = (AccountLevel)downlineInfo.RoleId;

            var dict = level switch
            {
                AccountLevel.Member => BuildMemberDownLineUsernameMessage(downlineBalance, account.Language,
                    account.CustomerLevel),
                _ => BuildOtherDownLineUsernameMessage(downlineBalance, account.Language, account.CustomerLevel),
            };
            
            
            textContainer.Add(BuildTitleRow(account.Language));
            textContainer.Add(
                $"**{ResourceHelper.Get(nameof(SharedResource.Username), account.Language).ToUpperInvariant()}**:");
            textContainer.Add($"{username}{messageFormatter.NewLine()}");
            foreach (var keyValuePair in dict)
            {
                textContainer.Add(keyValuePair.Key);
                textContainer.Add($"    {keyValuePair.Value}");
            }

            var message = string.Join(messageFormatter.NewLine(), textContainer);
            return Result.CreateSuccessfulResult(message);
        }

        public Dictionary<string, string> BuildOtherDownLineUsernameMessage(DownlineBalance bal, string lang,
            int customerLevel)
        {
            var dict = new Dictionary<string, string> { };

            AddCashMessage(dict, lang, bal.TotalCash, bal.YesterdayCash);
            AddBalMessage(dict, lang, bal.TotalBalance, bal.YesterdayBalance);
            AddOutStandingMessage(dict, lang, $"{(AccountLevel)customerLevel - 1}OutStanding", bal.Outstanding);
            AddPendingTransfer3rdMessage(dict, bal.PendingTransfer3rd, lang);
            AddCreditMessage(dict, lang, nameof(SharedResource.UsedCredit), bal.UsedCredit);
            AddCreditMessage(dict, lang, nameof(SharedResource.GivenCredit), bal.GivenCredit);
            AddCreditMessage(dict, lang, nameof(SharedResource.MemMaxCredit), bal.MemberMaxCredit);
            foreach (var totalDownline in bal.TotalDownlines.ToList())
            {
                Add_Active_Close_Suspended_Message(dict, lang, $"{(AccountLevel)totalDownline.RoleId}ActClosedSus",
                    totalDownline);
            }

            AddOutStandingMessage(dict, lang, nameof(SharedResource.MemberOutStanding), bal.TotalOutstanding);
            AddOutStandingMessage(dict, lang, $"{(AccountLevel)customerLevel}OutStanding", bal.UplineOutstanding);
            return dict;
        }

        public Dictionary<string, string> BuildMemberDownLineUsernameMessage(DownlineBalance bal, string lang,
            int customerLevel)
        {
            var dict = new Dictionary<string, string> { };
            AddBalMessage(dict, lang, bal.TotalBalance, bal.YesterdayBalance);
            AddPendingTransfer3rdMessage(dict, bal.PendingTransfer3rd, lang);
            AddCreditMessage(dict, lang, nameof(SharedResource.GivenCredit), bal.GivenCredit);
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.NetPosition), lang)}:",
                bal.NetPosition?.ToFormattedNumber());
            AddCreditMessage(dict, lang, nameof(SharedResource.BetCredit), bal.BetCredit);
            AddOutStandingMessage(dict, lang, nameof(SharedResource.MemberOutStanding), bal.Outstanding);
            AddOutStandingMessage(dict, lang, $"{(AccountLevel)customerLevel}OutStanding", bal.UplineOutstanding);
            return dict;
        }

        private string BuildTitleRow(string language)
            => $"[{ResourceHelper.Get(nameof(SharedResource.Balance), language)} - {DateTime.Now:yyyyMMdd}]";
        
        public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => await balanceInfoService.GetTopUsername(account.CustId, account.CustomerLevel, top);

        private string BuildTotalACT_CLOSED_SUS(TotalDownline downline)
        {
            return downline == null
                ? "0/0/0"
                : $"{downline.TotalActive}/{downline.TotalClosed}/{downline.TotalSuspended}";
        }

        public void AddCashMessage(Dictionary<string, string> dict, string lang, decimal? todayCash,
            decimal? yesterdayCash)
        {
            
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.Cash), lang)}:", todayCash?.ToFormattedNumber());
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.YesterdayCash), lang)}:",
                yesterdayCash?.ToFormattedNumber());
        }

        public void AddBalMessage(Dictionary<string, string> dict, string lang, decimal todayBalance,
            decimal yesterdayBalance)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.Balance), lang)}:",
                todayBalance.ToFormattedNumber());
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.YesterdayBal), lang)}:",
                yesterdayBalance.ToFormattedNumber());
        }

        public void AddPendingTransfer3rdMessage(Dictionary<string, string> dict, decimal pendingTransfer3rd,
            string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.ThirdPartyPendingTransfer), lang)}:",
                pendingTransfer3rd.ToFormattedNumber());
        }

        public void AddCreditMessage(Dictionary<string, string> dict, string lang, string typeCredit,
            decimal? creditValue)
        {
            if (creditValue != null)
                dict.Add($"{ResourceHelper.Get(typeCredit, lang)}:",
                    creditValue?.ToFormattedNumber());
        }

        public void Add_Active_Close_Suspended_Message(Dictionary<string, string> dict,
            string lang, string typeAccountActCloseSus, TotalDownline totalDownline)
        {
            dict.Add($"{ResourceHelper.Get(typeAccountActCloseSus, lang)}:",
                $"{BuildTotalACT_CLOSED_SUS(totalDownline)}");
        }

        public void AddOutStandingMessage(Dictionary<string, string> dict, string lang, string typeOutStanding,
            decimal valueOutStanding)
        {
            dict.Add(
                $"{ResourceHelper.Get(typeOutStanding, lang)}:",
                valueOutStanding.ToFormattedNumber());
        }
    }
};