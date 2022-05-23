using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alpha.API.Client.Models.BalanceInfo;
using Alpha.API.Client.Services;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Constants;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.Infrastructure.Shared.Extensions;
using TelegramBot.Infrastructure.Shared.Resources;

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
            IBalanceInfoService balanceInfoService
        ) :
            base(messageBuilder, messageFormatter)
        {
            this.balanceInfoService = balanceInfoService;
            this.customerService = customerService;
        }

        public async Task<Result> Handle(CustBotAccount account, string[] parameters)
        {
            var parameter = parameters[0];
            var isNumber = int.TryParse(parameter, out int topQuery);
            if (isNumber)
            {
                return await HandleTopXBalance(account, topQuery);
            }

            var action = parameter switch
            {
                "m" => HandleMyBalance(account),
                _ => HandleBalanceDownLineUsername(account, parameter)
            };

            return await action;
        }


        private async Task<Result> HandleTopXBalance(CustBotAccount account, int topQuery)
        {
            var topBalanceReport = await balanceInfoService.GetTopBalance(account.CustId, account.CustomerLevel, topQuery);

            var textContainer = new List<string>()
            {
                $"[{ResourceHelper.Get(nameof(SharedResource.Balance), account.Language)} - {DateTime.Now:yyyyMMdd}]",
                ResourceHelper.Get($"CustLevel_{account.CustomerLevel - 1}", account.Language),
                $"{ResourceHelper.Get(nameof(SharedResource.Today), account.Language)}{messageFormatter.Point()}{ResourceHelper.Get(nameof(SharedResource.Yesterday), account.Language)}{messageFormatter.NewLine()}",
            
            };
            var index = 1;
            foreach (var item in topBalanceReport)
            {
                textContainer.Add($"{index++}.{item.Username}:");
                textContainer.Add($"    {item.Balance.ToFormattedNumber()}{messageFormatter.Point()}{item.YesterdayBalance.ToFormattedNumber()}");
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
                {$"[{ResourceHelper.Get(nameof(SharedResource.Balance), lang)} - {DateTime.Now:yyyyMMdd}]",""},
                {$"{ResourceHelper.Get(nameof(SharedResource.Currency), lang)}:", bal.CurrencyName},
                {$"{ResourceHelper.Get(nameof(SharedResource.Cash), lang)}:", bal.CashBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayCash), lang)}:", bal.YesterdayTotalBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.TodayWinLoss), lang)}:", bal.TodayWinLoss.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayWinLoss), lang)}:", bal.YesterdayWinLoss.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.ThirdPartyPendingTransfer), lang)}:", bal.PendingTransfer3rd.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.GivenCredit),lang)}:", bal.GivenCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get($"{(CustomerLevel)account.CustomerLevel - 1}Credit",lang)}:", bal.MemberCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemberOutStanding), lang)}:", bal.TotalOutstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get($"{(CustomerLevel)account.CustomerLevel}OutStanding", lang)}:", bal.Outstanding.ToFormattedNumber()},
            };
            var textContainer = new List<string>();
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
            var downlineBalance = await balanceInfoService.GetDownlineBalance(account.CustId, account.CustomerLevel, downlineInfo.CustId);

            var textContainer = new List<string>()
            {
                $"[{ResourceHelper.Get(nameof(SharedResource.Balance),  account.Language)} - {DateTime.Now:yyyyMMdd}]",
                $"{ResourceHelper.Get(nameof(SharedResource.Username), account.Language).ToUpperInvariant()}:",
                $"{username}{messageFormatter.NewLine()}"
            };
            
            var dict = (CustomerLevel)downlineInfo.RoleId switch
            {
                CustomerLevel.Member => BuildMemberDownLineUsernameMessage(downlineBalance, account.Language),
                CustomerLevel.Agent => BuildAgentDownLineUsernameMessage(downlineBalance, account.Language),
                CustomerLevel.Master => BuildMasterDownLineUsernameMessage(downlineBalance, account.Language),
                _ => new Dictionary<string,string>(),
            };
            
            foreach (var keyValuePair in dict)
            {
                textContainer.Add(keyValuePair.Key);
                textContainer.Add($"    {keyValuePair.Value}");
            }

            var message = string.Join(messageFormatter.NewLine(), textContainer);
            return Result.CreateSuccessfulResult(message);
        }

        public Dictionary<string, string> BuildMasterDownLineUsernameMessage(DownlineBalance bal, string lang )
        {
            return new Dictionary<string, string>
            {
                {$"{ResourceHelper.Get(nameof(SharedResource.Cash), lang)}:", bal.TotalCash?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayCash), lang)}:", bal.YesterdayCash?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.Balance), lang)}:", bal.TotalBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayBal), lang)}:", bal.YesterdayBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MasterOutStanding), lang)}:", bal.Outstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.ThirdPartyPendingTransfer), lang)}:", bal.PendingTransfer3rd.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.UsedCredit),lang)}:", bal.UsedCredit?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.GivenCredit),lang)}:", bal.GivenCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.AgentActClosedSus), lang)}:", $"{BuildTotalACT_CLOSED_SUS(bal.TotalDownlines,CustomerLevel.Agent)}"},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemberActClosedSus), lang)}:", $"{BuildTotalACT_CLOSED_SUS(bal.TotalDownlines,CustomerLevel.Member)}"},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemberOutStanding), lang)}:", bal.TotalOutstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.SuperOutStanding), lang)}:", bal.UplineOutstanding.ToFormattedNumber()}
            };
        }
        
        public Dictionary<string, string> BuildAgentDownLineUsernameMessage(DownlineBalance bal, string lang)
        {
            return new Dictionary<string, string>
            {
                {$"{ResourceHelper.Get(nameof(SharedResource.Cash), lang)}:", bal.TotalCash?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayCash), lang)}:", bal.YesterdayCash?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.Balance), lang)}:", bal.TotalBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayBal), lang)}:", bal.YesterdayBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.AgentOutStanding), lang)}:", bal.Outstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.ThirdPartyPendingTransfer), lang)}:", bal.PendingTransfer3rd.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.UsedCredit),lang)}:", bal.UsedCredit?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.GivenCredit),lang)}:", bal.GivenCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemMaxCredit),lang)}:", bal.GivenCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemberActClosedSus), lang)}:",$"{BuildTotalACT_CLOSED_SUS(bal.TotalDownlines,CustomerLevel.Member)}"},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemberOutStanding), lang)}:", bal.TotalOutstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MasterOutStanding), lang)}:", bal.UplineOutstanding.ToFormattedNumber()}
            };
        }
        
        public Dictionary<string, string> BuildMemberDownLineUsernameMessage(DownlineBalance bal, string lang)
        {
            return new Dictionary<string, string>
            {
                {$"{ResourceHelper.Get(nameof(SharedResource.Balance), lang)}:", bal.TotalBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.YesterdayBal), lang)}:", bal.YesterdayBalance.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.ThirdPartyPendingTransfer), lang)}:", bal.PendingTransfer3rd.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.GivenCredit),lang)}:", bal.GivenCredit.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.NetPosition), lang)}:", bal.NetPosition?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.BetCredit), lang)}:", bal.BetCredit?.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.MemberOutStanding), lang)}:", bal.Outstanding.ToFormattedNumber()},
                {$"{ResourceHelper.Get(nameof(SharedResource.AgentOutStanding), lang)}:", bal.UplineOutstanding.ToFormattedNumber()}
            };
        }
        public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => await balanceInfoService.GetTopUsername(account.CustId, account.CustomerLevel, top);

        private string BuildTotalACT_CLOSED_SUS(IEnumerable<TotalDownline> totalDownline, CustomerLevel level)
        {
            var downline = totalDownline.ToList().Find(e => e.RoleId == (byte)level);
            return downline == null ? "0/0/0" : $"{downline.TotalActive}/{downline.TotalClosed}/{downline.TotalSuspended}";
        }
    }
};