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

            textContainer.Add(NameLevelAccount(account.CustomerLevel, account.Language));

            textContainer.Add(
                $"{ResourceHelper.Get(nameof(SharedResource.Today), account.Language)}{messageFormatter.Point()}{ResourceHelper.Get(nameof(SharedResource.Yesterday), account.Language)}");
            textContainer.Add(messageFormatter.NewLine());

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
                textContainer.Add(messageBuilder.BuildHiddenDownlinesMessage(account.Language, topBalanceReport.HiddenDownlinesCount, (byte)(account.CustomerLevel - 1)));
            }

            var message = string.Join(messageFormatter.NewLine(), textContainer);
            return Result.CreateSuccessfulResult(message);
        }

        private async Task<Result> HandleMyBalance(CustBotAccount account)
        {
            var bal = await balanceInfoService.GetMyBalance(account.CustId, account.CustomerLevel);
            var lang = account.Language;
            var dict = new Dictionary<string, string> { };
            var textContainer = new List<string>();

            AddCurrencyMessage(dict, bal, lang);
            AddCashMessage(dict, bal, lang);
            AddBalMessage(dict, bal, lang);
            AddWinLossMessage(dict, bal, lang);
            AddPendingTransfer3rdMessage(dict, bal, lang);
            AddGivenCreditMessage(dict, bal, lang);
            AddDirectDownLineCreditMessage(dict, bal, lang, (AccountLevel)account.CustomerLevel - 1);
            AddMemberOutStandingMessage(dict, bal, lang);
            AddUserLevelOutStandingMessage(dict, bal, lang, (AccountLevel)account.CustomerLevel);

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

            textContainer.Add(
                $"{ResourceHelper.Get(nameof(SharedResource.Username), account.Language).ToUpperInvariant()}:");
            textContainer.Add(username);

            var level = (AccountLevel)downlineInfo.RoleId;
            var dict = level switch
            {
                AccountLevel.Master => BuildMasterDownLineUsernameMessage(downlineBalance, account.Language),
                AccountLevel.Agent => BuildAgentDownLineUsernameMessage(downlineBalance, account.Language),
                AccountLevel.Member => BuildMemberDownLineUsernameMessage(downlineBalance, account.Language),
                _ => new Dictionary<string, string>()
            };

            foreach (var keyValuePair in dict)
            {
                textContainer.Add(keyValuePair.Key);
                textContainer.Add($"    {keyValuePair.Value}");
            }

            var message = string.Join(messageFormatter.NewLine(), textContainer);
            return Result.CreateSuccessfulResult(message);
        }

        public Dictionary<string, string> BuildMasterDownLineUsernameMessage(DownlineBalance bal, string lang)
        {
            var dict = new Dictionary<string, string> { };

            AddCashMessage(dict, bal, lang);
            AddBalMessage(dict, bal, lang);
            AddDirectDownLineLevelOutStandingMessage(dict, bal, lang, AccountLevel.Master);
            AddPendingTransfer3rdMessage(dict, bal, lang);
            AddUsedCreditMessage(dict, bal, lang);
            AddGivenCreditMessage(dict, bal, lang);
            AddMemMaxCreditMessage(dict, bal, lang);
            AddAgent_Active_Close_Suspended_Message(dict, bal, lang);
            AddMem_Active_Close_Suspended_Message(dict, bal, lang);
            AddMemberOutStandingMessage(dict, bal, lang);
            AddUserLevelOutStandingMessage(dict, bal, lang, AccountLevel.Supper);

            return dict;
        }

        public Dictionary<string, string> BuildAgentDownLineUsernameMessage(DownlineBalance bal, string lang)
        {
            var dict = new Dictionary<string, string> { };

            AddCashMessage(dict, bal, lang);
            AddBalMessage(dict, bal, lang);
            AddDirectDownLineLevelOutStandingMessage(dict, bal, lang, AccountLevel.Member);
            AddPendingTransfer3rdMessage(dict, bal, lang);
            AddUsedCreditMessage(dict, bal, lang);
            AddGivenCreditMessage(dict, bal, lang);
            AddMemMaxCreditMessage(dict, bal, lang);
            AddMem_Active_Close_Suspended_Message(dict, bal, lang);
            AddMemberOutStandingMessage(dict, bal, lang);
            AddUserLevelOutStandingMessage(dict, bal, lang, AccountLevel.Master);

            return dict;
        }

        public Dictionary<string, string> BuildMemberDownLineUsernameMessage(DownlineBalance bal, string lang)
        {
            var dict = new Dictionary<string, string> { };

            AddBalMessage(dict, bal, lang);
            AddDirectDownLineLevelOutStandingMessage(dict, bal, lang, AccountLevel.Member);
            AddPendingTransfer3rdMessage(dict, bal, lang);
            AddGivenCreditMessage(dict, bal, lang);
            AddNetPositionMessage(dict, bal, lang);
            AddBetCreditMessage(dict, bal, lang);
            AddUserLevelOutStandingMessage(dict, bal, lang, AccountLevel.Agent);

            return dict;
        }

        private string BuildTitleRow(string language)
            => $"[{ResourceHelper.Get(nameof(SharedResource.Balance), language)} - {DateTime.Now:yyyyMMdd}]";


        public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => await balanceInfoService.GetTopUsername(account.CustId, account.CustomerLevel, top);

        private string NameLevelAccount(int level, string language) =>
            ResourceHelper.Get($"CustLevel_{level - 1}", language);

        private string BuildTotalACT_CLOSED_SUS(TotalDownline downline)
        {
            return downline == null
                ? "0/0/0"
                : $"{downline.TotalActive}/{downline.TotalClosed}/{downline.TotalSuspended}";
        }

        public void AddCashMessage(Dictionary<string, string> dict, DownlineBalance bal, string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.Cash), lang)}:", bal.TotalCash?.ToFormattedNumber());
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.YesterdayCash), lang)}:",
                bal.YesterdayCash?.ToFormattedNumber());
        }

        public void AddCurrencyMessage(Dictionary<string, string> dict, CustomerBalance bal, string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.Currency), lang)}:", bal.CurrencyName);
        }

        public void AddCashMessage(Dictionary<string, string> dict, CustomerBalance bal, string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.Cash), lang)}:", bal.CashBalance.ToFormattedNumber());
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.YesterdayCash), lang)}:",
                bal.YesterdayCashBalance.ToFormattedNumber());
        }

        public void AddBalMessage(Dictionary<string, string> dict, DownlineBalance bal, string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.Balance), lang)}:",
                bal.TotalBalance.ToFormattedNumber());
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.YesterdayBal), lang)}:",
                bal.YesterdayBalance.ToFormattedNumber());
        }

        public void AddBalMessage(Dictionary<string, string> dict, CustomerBalance bal, string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.Balance), lang)}:",
                bal.TotalBalance.ToFormattedNumber());
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.YesterdayBal), lang)}:",
                bal.YesterdayTotalBalance.ToFormattedNumber());
        }

        public void AddWinLossMessage(Dictionary<string, string> dict, CustomerBalance bal, string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.TodayWinLoss), lang)}:",
                bal.TodayWinLoss.ToFormattedNumber());
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.YesterdayWinLoss), lang)}:",
                bal.YesterdayWinLoss.ToFormattedNumber());
        }

        public void AddPendingTransfer3rdMessage(Dictionary<string, string> dict, DownlineBalance bal, string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.ThirdPartyPendingTransfer), lang)}:",
                bal.PendingTransfer3rd.ToFormattedNumber());
        }

        public void AddPendingTransfer3rdMessage(Dictionary<string, string> dict, CustomerBalance bal, string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.ThirdPartyPendingTransfer), lang)}:",
                bal.PendingTransfer3rd.ToFormattedNumber());
        }


        public void AddGivenCreditMessage(Dictionary<string, string> dict, DownlineBalance bal, string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.GivenCredit), lang)}:",
                bal.GivenCredit.ToFormattedNumber());
        }

        public void AddGivenCreditMessage(Dictionary<string, string> dict, CustomerBalance bal, string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.GivenCredit), lang)}:",
                bal.GivenCredit.ToFormattedNumber());
        }

        public void AddMemMaxCreditMessage(Dictionary<string, string> dict, DownlineBalance bal, string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.MemMaxCredit), lang)}:",
                bal.MemberMaxCredit?.ToFormattedNumber());
        }

        public void AddUsedCreditMessage(Dictionary<string, string> dict, DownlineBalance bal, string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.UsedCredit), lang)}:",
                bal.UsedCredit?.ToFormattedNumber());
        }

        public void AddMem_Active_Close_Suspended_Message(Dictionary<string, string> dict, DownlineBalance bal,
            string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.MemActClosedSus), lang)}:",
                $"{BuildTotalACT_CLOSED_SUS(bal.TotalDownlines.ToList().Find(e => e.RoleId == (int)AccountLevel.Member))}");
        }

        public void AddAgent_Active_Close_Suspended_Message(Dictionary<string, string> dict, DownlineBalance bal,
            string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.AgentActClosedSus), lang)}:",
                $"{BuildTotalACT_CLOSED_SUS(bal.TotalDownlines.ToList().Find(e => e.RoleId == (int)AccountLevel.Agent))}");
        }


        public void AddNetPositionMessage(Dictionary<string, string> dict, DownlineBalance bal, string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.NetPosition), lang)}:",
                bal.NetPosition?.ToFormattedNumber());
        }

        public void AddBetCreditMessage(Dictionary<string, string> dict, DownlineBalance bal, string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.BetCredit), lang)}:",
                bal.BetCredit?.ToFormattedNumber());
        }


        // MEMBER OUTSTANDING: =>  Decimal TotalOutstanding
        public void AddMemberOutStandingMessage(Dictionary<string, string> dict, DownlineBalance bal, string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.BetCredit), lang)}:",
                bal.TotalOutstanding.ToFormattedNumber());
        }

        public void AddMemberOutStandingMessage(Dictionary<string, string> dict, CustomerBalance bal, string lang)
        {
            dict.Add($"{ResourceHelper.Get(nameof(SharedResource.MemberOutStanding), lang)}:",
                bal.TotalOutstanding.ToFormattedNumber());
        }

        // [Direct Downline's level] OUTSTANDING: =>  Decimal Outstanding,
        public void AddDirectDownLineLevelOutStandingMessage(Dictionary<string, string> dict, DownlineBalance bal,
            string lang, AccountLevel level)
        {
            switch (level)
            {
                case AccountLevel.Master:
                    dict.Add(
                        $"{ResourceHelper.Get(nameof(SharedResource.MasterOutStanding), lang)}:",
                        bal.Outstanding.ToFormattedNumber());
                    break;
                case AccountLevel.Agent:
                    dict.Add(
                        $"{ResourceHelper.Get(nameof(SharedResource.AgentOutStanding), lang)}:",
                        bal.Outstanding.ToFormattedNumber());
                    break;
                case AccountLevel.Member:
                    dict.Add(
                        $"{ResourceHelper.Get(nameof(SharedResource.MemberOutStanding), lang)}:",
                        bal.Outstanding.ToFormattedNumber());
                    break;
                case AccountLevel.Supper:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        public void AddDirectDownLineCreditMessage(Dictionary<string, string> dict, CustomerBalance bal,
            string lang, AccountLevel level)
        {
            dict.Add(
                $"{ResourceHelper.Get($"{level}Credit", lang)}:",
                bal.MemberCredit.ToFormattedNumber());
        }

        // [User's level] OUTSTANDING: Decimal UplineOutstanding,
        public void AddUserLevelOutStandingMessage(Dictionary<string, string> dict, DownlineBalance bal,
            string lang, AccountLevel level)
        {
            switch (level)
            {
                case AccountLevel.Supper:
                    dict.Add(
                        $"{ResourceHelper.Get(nameof(SharedResource.SuperOutStanding), lang)}:",
                        bal.UplineOutstanding.ToFormattedNumber());
                    break;
                case AccountLevel.Master:
                    dict.Add(
                        $"{ResourceHelper.Get(nameof(SharedResource.MasterOutStanding), lang)}:",
                        bal.UplineOutstanding.ToFormattedNumber());
                    break;
                case AccountLevel.Agent:
                    dict.Add(
                        $"{ResourceHelper.Get(nameof(SharedResource.AgentOutStanding), lang)}:",
                        bal.UplineOutstanding.ToFormattedNumber());
                    break;
                case AccountLevel.Member:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        public void AddUserLevelOutStandingMessage(Dictionary<string, string> dict, CustomerBalance bal,
            string lang, AccountLevel level)
        {
            switch (level)
            {
                case AccountLevel.Supper:
                    dict.Add(
                        $"{ResourceHelper.Get(nameof(SharedResource.SuperOutStanding), lang)}:",
                        bal.Outstanding.ToFormattedNumber());
                    break;
                case AccountLevel.Master:
                    dict.Add(
                        $"{ResourceHelper.Get(nameof(SharedResource.MasterOutStanding), lang)}:",
                        bal.Outstanding.ToFormattedNumber());
                    break;
                case AccountLevel.Agent:
                    dict.Add(
                        $"{ResourceHelper.Get(nameof(SharedResource.AgentOutStanding), lang)}:",
                        bal.Outstanding.ToFormattedNumber());
                    break;
                case AccountLevel.Member:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
};