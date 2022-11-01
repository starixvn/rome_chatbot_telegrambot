using Alpha.API.Client.Models.BalanceInfo;
using Alpha.API.Client.Services;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Constants;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.Infrastructure.Shared.Extensions;
using TelegramBot.Infrastructure.Shared.Resources;
using Alpha.API.Client.Models.Customers;
using Alpha.API.Client.Models.Outstanding;
using TelegramBot.Utilities.Extensions;
using System.Text;

namespace TelegramBot.Infrastructure.Function.Handlers
{
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
            var parameter = parameters[0]?.ToLowerInvariant();
            if (string.IsNullOrEmpty(parameter))
            {
                return Result.CreateErrorResult();
            }

            if (parameter == "m")
            {
                return await HandleMyBalance(account);
            }

            var downlineInfo = await customerService.GetDownlineInfo(account.CustId, parameter);
            if (downlineInfo != null && downlineInfo.CustId > 0)
            {
                return await HandleBalanceDownLineUsername(account, parameter, downlineInfo);
            }

            return int.TryParse(parameter, out var topQuery)
                ? await HandleTopXBalance(account, topQuery)
                : Result.CreateErrorResult(messageBuilder.BuildInvalidUsernameMessage(account.Language));
        }

        private async Task<Result> HandleTopXBalance(CustBotAccount account, int topQuery)
        {
            if (topQuery <= 0 || topQuery > webAPIConfiguration.BalanceMaxTopQuery)
            {
                var errMsg = messageBuilder.BuildInvalidDownlinesMessage(account.Language, webAPIConfiguration.BalanceMaxTopQuery);
                return Result.CreateErrorResult(errMsg);
            }

            var topBalanceReport = await balanceInfoService.GetTopBalance(account.CustId, account.CustomerLevel, topQuery);
            var usernameText = ResourceHelper.Get(nameof(SharedResource.Username), account.Language);
            var todayText = ResourceHelper.Get(nameof(SharedResource.Today), account.Language);
            var textContainer = new List<string>
            {
                $"[{ResourceHelper.Get(nameof(SharedResource.DownlineTopHighestBalance), account.Language)} - {DateTime.Now.ToSimpleDate()}]",
                "<pre>",
                messageFormatter.FormatHeaderRow(usernameText, todayText, account.Language),
                "________________________________"
            };

            if (topBalanceReport == null || !topBalanceReport.Any())
            {
                textContainer.Add(messageBuilder.BuildNoDataMessage(account.Language));
            }
            else
            {
                foreach (var item in topBalanceReport)
                {
                    textContainer.Add(messageFormatter.FormatRow(item.Username, item.Balance.ToFormattedNumber(), LanguageCodes.English));
                }
            }

            textContainer.Add("</pre>");

            var message = string.Join(messageFormatter.NewLine(), textContainer);
            return Result.CreateSuccessfulResult(message);
        }

        private async Task<Result> HandleMyBalance(CustBotAccount account)
        {
            var bal = await balanceInfoService.GetMyBalance(account.CustId, account.CustomerLevel);
            var lang = account.Language;

            var textContainer = new List<string>
            {
                $"[{ResourceHelper.Get(nameof(SharedResource.YourBalance), lang)} - {DateTime.Now.ToSimpleDate()}]",
                "<pre>",
                messageFormatter.FormatRow(ResourceHelper.Get(nameof(SharedResource.Currency), lang), bal.CurrencyName, lang)
            };

            var dict = new Dictionary<string, decimal?>
            {
                {nameof(SharedResource.Cash), bal.CashBalance},
                {nameof(SharedResource.YesterdayCash), bal.YesterdayCashBalance},
                {nameof(SharedResource.BAL), bal.TotalBalance},
                {nameof(SharedResource.YesterdayBal), bal.YesterdayTotalBalance},
                {nameof(SharedResource.TodayWinLoss), bal.TodayWinLoss},
                {nameof(SharedResource.YesterdayWinLoss), bal.YesterdayWinLoss},
                {nameof(SharedResource.ThirdPartyPendingTransfer), bal.PendingTransfer3rd},
                {nameof(SharedResource.GivenCredit), bal.GivenCredit},
                {$"{(CustomerLevel)account.CustomerLevel - 1}Credit", bal.MemberCredit},
                {nameof(SharedResource.MemberOutStanding), bal.Outstanding.MemberOutstanding},
                {$"{(CustomerLevel)account.CustomerLevel}OutStanding", GetOutStanding(bal.Outstanding,account.CustomerLevel)},
            };
            foreach (var item in dict)
            {
                var title = ResourceHelper.Get(item.Key, account.Language).ToUpperInvariant();
                var value = item.Value?.ToFormattedNumber();
                textContainer.Add(messageFormatter.FormatRow(title, value, account.Language));
            }
            textContainer.Add("</pre>");
            var message = string.Join(messageFormatter.NewLine(), textContainer);
            return Result.CreateSuccessfulResult(message);
        }

        private async Task<Result> HandleBalanceDownLineUsername(CustBotAccount account, string username, DownlineInfo downlineInfo)
        {
            if (!IsValidDirectDownline(account.CustomerLevel, downlineInfo))
            {
                return Result.CreateErrorResult(messageBuilder.BuildInvalidUsernameMessage(account.Language));
            }

            var downlineBalance = await balanceInfoService.GetDownlineBalance(downlineInfo.CustId, downlineInfo.RoleId);
            var textContainer = new List<string>
            {
                $"[{ResourceHelper.Get(nameof(SharedResource.DownlineBalance),  account.Language)} - {DateTime.Now.ToSimpleDate()}]",
                "<pre>",
                 messageFormatter.FormatRow(ResourceHelper.Get(nameof(SharedResource.Username), account.Language).ToUpperInvariant(), username, account.Language),
            };

            var dict = (CustomerLevel)downlineInfo.RoleId switch
            {
                CustomerLevel.Member => BuildMemberDownLineUsernameMessage(downlineBalance),
                CustomerLevel.Agent => BuildAgentDownLineUsernameMessage(downlineBalance),
                CustomerLevel.Master => BuildMasterDownLineUsernameMessage(downlineBalance),
                _ => new Dictionary<string, decimal?>(),
            };

            foreach (var item in dict)
            {
                var title = ResourceHelper.Get(item.Key, account.Language).ToUpperInvariant();
                var value = item.Value?.ToFormattedNumber();
                textContainer.Add(messageFormatter.FormatRow(title, value, account.Language));
            }

            textContainer.Add("</pre>");

            var message = string.Join(messageFormatter.NewLine(), textContainer);
            return Result.CreateSuccessfulResult(message);
        }

        public static Dictionary<string, decimal?> BuildMasterDownLineUsernameMessage(DownlineBalance bal)
            => new()
            {
                {nameof(SharedResource.Cash), bal.TotalCash},
                {nameof(SharedResource.YesterdayCash), bal.YesterdayCash},
                {nameof(SharedResource.Balance), bal.TotalBalance},
                {nameof(SharedResource.YesterdayBal), bal.YesterdayBalance},
                {nameof(SharedResource.MasterOutStanding), bal.Outstanding.MasterOutstanding},
                {nameof(SharedResource.ThirdPartyPendingTransfer), bal.PendingTransfer3rd},
                {nameof(SharedResource.UsedCredit), bal.UsedCredit},
                {nameof(SharedResource.GivenCredit), bal.GivenCredit},
                {nameof(SharedResource.MemberOutStanding), bal.Outstanding.MemberOutstanding},
                {nameof(SharedResource.SuperOutStanding), bal.Outstanding.SuperOutstanding}
            };

        public static Dictionary<string, decimal?> BuildAgentDownLineUsernameMessage(DownlineBalance bal)
            => new()
            {
                {nameof(SharedResource.Cash), bal.TotalCash},
                {nameof(SharedResource.YesterdayCash), bal.YesterdayCash},
                {nameof(SharedResource.Balance), bal.TotalBalance},
                {nameof(SharedResource.YesterdayBal), bal.YesterdayBalance},
                {nameof(SharedResource.AgentOutStanding), bal.Outstanding.AgentOutstanding},
                {nameof(SharedResource.ThirdPartyPendingTransfer), bal.PendingTransfer3rd},
                {nameof(SharedResource.UsedCredit), bal.UsedCredit},
                {nameof(SharedResource.GivenCredit), bal.GivenCredit},
                {nameof(SharedResource.MemMaxCredit), bal.GivenCredit},
                {nameof(SharedResource.MemberOutStanding), bal.Outstanding.MemberOutstanding},
                {nameof(SharedResource.MasterOutStanding), bal.Outstanding.MasterOutstanding}
            };

        public static Dictionary<string, decimal?> BuildMemberDownLineUsernameMessage(DownlineBalance bal)
            => new()
            {
                {nameof(SharedResource.Balance), bal.TotalBalance},
                {nameof(SharedResource.YesterdayBal), bal.YesterdayBalance},
                {nameof(SharedResource.ThirdPartyPendingTransfer), bal.PendingTransfer3rd},
                {nameof(SharedResource.GivenCredit), bal.GivenCredit},
                {nameof(SharedResource.NetPosition), bal.NetPosition},
                {nameof(SharedResource.BetCredit), bal.BetCredit},
                {nameof(SharedResource.MemberOutStanding), bal.Outstanding.MemberOutstanding},
                {nameof(SharedResource.AgentOutStanding), bal.Outstanding.AgentOutstanding}
            };

        public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => await balanceInfoService.GetTopUsername(account.CustId, account.CustomerLevel, top);

        private static decimal GetOutStanding(CustomerOutstanding outstanding, byte customerLevel)
            => (CustomerLevel)customerLevel switch
            {
                CustomerLevel.Member => outstanding.MemberOutstanding,
                CustomerLevel.Agent => outstanding.AgentOutstanding,
                CustomerLevel.Master => outstanding.MasterOutstanding,
                CustomerLevel.Super => outstanding.SuperOutstanding,
                _ => throw new ArgumentOutOfRangeException(nameof(customerLevel), customerLevel, null)
            };
    }
};