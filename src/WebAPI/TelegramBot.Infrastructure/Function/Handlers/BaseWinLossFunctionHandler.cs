using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alpha.API.Client.Models.WinLoss;
using Alpha.API.Client.Services;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Models;
using TelegramBot.Infrastructure.Shared.Resources;
using TelegramBot.Infrastructure.Shared.Extensions;
using TelegramBot.Infrastructure.Shared.Constants;

namespace TelegramBot.Infrastructure.Function.Handlers
{
    public abstract class BaseWinLossFunctionHandler : BaseFunctionHandler
    {
        protected readonly IWinLossService winLossService;

        protected readonly ICustomerService customerService;

        protected readonly WebAPIConfiguration webAPIConfiguration;

        protected BaseWinLossFunctionHandler(
            IMessageBuilder messageBuilder,
            IMessageFormatter messageFormatter,
            IWinLossService winLossService,
            ICustomerService customerService,
            WebAPIConfiguration webAPIConfiguration)
            : base(messageBuilder, messageFormatter)
        {
            this.winLossService = winLossService;
            this.customerService = customerService;
            this.webAPIConfiguration = webAPIConfiguration;
        }

        public async Task<Result> HandleWinLoss(CustBotAccount account, string[] parameters, DateTime dateTime)
        {
            var parameter = parameters[0];
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return Result.CreateErrorResult();
            }

            var message = string.Empty;
            if (parameter == "m")
            {
                var data = await winLossService.GetWinLoss(account.CustId, account.CustomerLevel, dateTime);
                message = BuildWinLossMessage(account.Language, data, account.CustomerLevel);
            }
            else
            {
                var downlineInfo = await customerService.GetDownlineInfo(account.CustId, parameter);
                if (!IsValidDirectDownline(account.CustomerLevel, downlineInfo))
                {
                    return Result.CreateErrorResult(messageBuilder.BuildInvalidUsernameMessage(account.Language));
                }

                var data = await winLossService.GetDownlineWinLoss(account.CustId, account.CustomerLevel, dateTime, downlineInfo.CustId);
                message = BuildDownlineWinLossMessage(account.Language, data, parameter, downlineInfo.RoleId);
            }

            return Result.CreateSuccessfulResult(message);
        }

        private string BuildWinLossMessage(string language, WinLossBase winLoss, byte custLevel)
        {
            var textContainer = new List<string>();
            AddTitleRow(textContainer, language);
            AddBodyMessage(textContainer, winLoss, language, custLevel);
            return string.Join(messageFormatter.NewLine(), textContainer);
        }

        private string BuildDownlineWinLossMessage(string language, WinLossBase winLoss, string downlineUsername, byte custLevel)
        {
            var textContainer = new List<string>();
            AddTitleRow(textContainer, language);
            textContainer.Add($"{ResourceHelper.Get(nameof(SharedResource.Username), language).ToUpperInvariant()}:");
            textContainer.Add($"    {downlineUsername}{messageFormatter.NewLine()}");
            AddBodyMessage(textContainer, winLoss, language, custLevel);
            return string.Join(messageFormatter.NewLine(), textContainer);
        }

        protected abstract void AddTitleRow(List<string> textContainer, string language);

        private void AddBodyMessage(List<string> textContainer, WinLossBase winLoss, string language, byte custLevel)
        {
            if (winLoss == null)
            {
                textContainer.Add(messageBuilder.BuildNoDataMessage(language));
                return;
            }

            var messages = (CustomerLevel)custLevel switch
            {
                CustomerLevel.Super => AddWinLossMaster((WinLossMaster)winLoss),
                CustomerLevel.Master => AddWinLossAgent((WinLossAgent)winLoss),
                CustomerLevel.Agent => AddWinLossMember((WinLossMember)winLoss),
                _ => AddWinLossMemberDownline((WinLossMember)winLoss),
            };

            this.FormatMessages(textContainer, messages, language);
        }

        private static Dictionary<string, decimal> AddWinLossMaster(WinLossMaster winLossMaster)
        {
            return new Dictionary<string, decimal>
            {
                {nameof(winLossMaster.Turnover), winLossMaster.Turnover},
                {nameof(winLossMaster.GrossComm), winLossMaster.GrossComm},
                {nameof(winLossMaster.MasterTotal), winLossMaster.MasterTotal},
                {nameof(winLossMaster.SuperWinLoss), winLossMaster.SuperWinLoss},
                {nameof(winLossMaster.SuperComm), winLossMaster.SuperComm},
                {nameof(winLossMaster.SuperTotal), winLossMaster.SuperTotal},
                {nameof(winLossMaster.CompanyWinLoss), winLossMaster.CompanyWinLoss},
            };
        }

        private static Dictionary<string, decimal> AddWinLossAgent(WinLossAgent winLossAgent)
        {
            return new Dictionary<string, decimal>
            {
                {nameof(winLossAgent.Turnover), winLossAgent.Turnover},
                {nameof(winLossAgent.GrossComm), winLossAgent.GrossComm},
                {nameof(winLossAgent.AgentTotal), winLossAgent.AgentTotal},
                {nameof(winLossAgent.MasterWinLoss), winLossAgent.MasterWinLoss},
                {nameof(winLossAgent.MasterComm), winLossAgent.MasterComm},
                {nameof(winLossAgent.MasterTotal), winLossAgent.MasterTotal},
                {nameof(winLossAgent.CompanyWinLoss), winLossAgent.CompanyWinLoss},
            };
        }

        private static Dictionary<string, decimal> AddWinLossMember(WinLossMember winLossMember)
        {
            return new Dictionary<string, decimal>
            {
                {nameof(winLossMember.Turnover), winLossMember.Turnover},
                {nameof(winLossMember.GrossComm), winLossMember.GrossComm},
                {nameof(winLossMember.MemberWinLoss), winLossMember.MemberWinLoss},
                {nameof(winLossMember.MemberComm), winLossMember.MemberComm},
                {nameof(winLossMember.MemberTotal), winLossMember.MemberTotal},
                {nameof(winLossMember.AgentWinLoss), winLossMember.AgentWinLoss},
                {nameof(winLossMember.AgentComm), winLossMember.AgentComm},
                {nameof(winLossMember.AgentTotal), winLossMember.AgentTotal},
                {nameof(winLossMember.CompanyWinLoss), winLossMember.CompanyWinLoss}
            };
        }

        private static Dictionary<string, decimal> AddWinLossMemberDownline(WinLossMember winLossMember)
        {
            return new Dictionary<string, decimal>
            {
                {nameof(winLossMember.MemberWinLoss), winLossMember.MemberWinLoss},
                {nameof(winLossMember.MemberComm), winLossMember.MemberComm},
                {nameof(winLossMember.MemberTotal), winLossMember.MemberTotal}
            };
        }

        private void FormatMessages(List<string> textContainer, Dictionary<string, decimal> messages, string language)
        {
            foreach (var message in messages)
            {
                textContainer.Add($"{ResourceHelper.Get(message.Key, language).ToUpperInvariant()}: {message.Value.ToFormattedNumber()}");
            }
        }
    }
}