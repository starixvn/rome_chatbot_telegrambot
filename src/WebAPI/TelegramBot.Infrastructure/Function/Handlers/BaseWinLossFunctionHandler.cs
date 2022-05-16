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

        public BaseWinLossFunctionHandler(
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

            WinLossBase data = null;
            string message = string.Empty;
            if (parameter == "m")
            {
                data = await winLossService.GetWinLoss(account.CustId, account.CustomerLevel, dateTime);
                message = BuildWinLossMessage(account.Language, data, account.CustomerLevel);
            }
            else
            {
                var downlineInfo = await customerService.GetDownlineInfo(account.CustId, parameter);
                if (!IsValidDirectDownline(account.CustomerLevel, downlineInfo))
                {
                    return Result.CreateErrorResult(messageBuilder.BuildInvalidCustNameMessage(account.Language, parameter));
                }

                data = await winLossService.GetDownlineWinLoss(account.CustId, account.CustomerLevel, dateTime, downlineInfo.CustId);
                message = BuildDownlineWinLossMessage(account.Language, data, parameter, (byte)downlineInfo.RoleId);
            }

            return Result.CreateSuccessfulResult(message);
        }

        protected string BuildWinLossMessage(string language, WinLossBase winLoss, byte custLevel)
        {
            var textContainer = new List<string>();
            AddTitleRow(textContainer, language);
            AddBodyMessage(textContainer, winLoss, language, custLevel);
            return string.Join(messageFormatter.NewLine(), textContainer);
        }

        protected string BuildDownlineWinLossMessage(string language, WinLossBase winLoss, string downlineUsername, byte custLevel)
        {
            var textContainer = new List<string>();
            AddTitleRow(textContainer, language);
            textContainer.Add($"{ResourceHelper.Get(nameof(SharedResource.Username), language).ToUpperInvariant()}:");
            textContainer.Add($"    {downlineUsername}{messageFormatter.NewLine()}");
            AddBodyMessage(textContainer, winLoss, language, custLevel);
            return string.Join(messageFormatter.NewLine(), textContainer);
        }

        protected abstract void AddTitleRow(List<string> textContainer, string language);

        protected void AddBodyMessage(List<string> textContainer, WinLossBase winLoss, string language, byte custLevel)
        {
            if (winLoss == null)
            {
                textContainer.Add(messageBuilder.BuildNoDataMessage(language));
                return;
            }

            switch (custLevel)
            {
                case CustomerLevel.Super:
                    AddWinLossMaster(textContainer, (WinLossMaster)winLoss, language);
                    break;
                case CustomerLevel.Master:
                    AddWinLossAgent(textContainer, (WinLossAgent)winLoss, language);
                    break;
                case CustomerLevel.Agent:
                    AddWinLossMember(textContainer, (WinLossMember)winLoss, language);
                    break;
                case CustomerLevel.Member:
                    AddWinLossMemberDownline(textContainer, (WinLossMember)winLoss, language);
                    break;
                default:
                    break;
            }
        }

        private void AddWinLossMaster(List<string> textContainer, WinLossMaster winLossMaster, string language)
        {
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMaster.Turnover), language).ToUpperInvariant()}: {winLossMaster.Turnover.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMaster.GrossComm), language).ToUpperInvariant()}: {winLossMaster.GrossComm.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMaster.MasterTotal), language).ToUpperInvariant()}: {winLossMaster.MasterTotal.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMaster.SuperWinLoss), language).ToUpperInvariant()}: {winLossMaster.SuperWinLoss.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMaster.SuperComm), language).ToUpperInvariant()}: {winLossMaster.SuperComm.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMaster.SuperTotal), language).ToUpperInvariant()}: {winLossMaster.SuperTotal.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMaster.CompanyWinLoss), language).ToUpperInvariant()}: {winLossMaster.CompanyWinLoss.ToFormattedNumber()}");
        }

        private void AddWinLossAgent(List<string> textContainer, WinLossAgent winLossAgent, string language)
        {
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossAgent.Turnover), language).ToUpperInvariant()}: {winLossAgent.Turnover.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossAgent.GrossComm), language).ToUpperInvariant()}: {winLossAgent.GrossComm.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossAgent.AgentTotal), language).ToUpperInvariant()}: {winLossAgent.AgentTotal.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossAgent.MasterWinLoss), language).ToUpperInvariant()}: {winLossAgent.MasterWinLoss.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossAgent.MasterComm), language).ToUpperInvariant()}: {winLossAgent.MasterComm.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossAgent.MasterTotal), language).ToUpperInvariant()}: {winLossAgent.MasterTotal.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossAgent.CompanyWinLoss), language).ToUpperInvariant()}: {winLossAgent.CompanyWinLoss.ToFormattedNumber()}");
        }

        private void AddWinLossMember(List<string> textContainer, WinLossMember winLossMember, string language)
        {
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMember.Turnover), language).ToUpperInvariant()}: {winLossMember.Turnover.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMember.GrossComm), language).ToUpperInvariant()}: {winLossMember.GrossComm.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMember.MemberWinLoss), language).ToUpperInvariant()}: {winLossMember.MemberWinLoss.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMember.MemberComm), language).ToUpperInvariant()}: {winLossMember.MemberComm.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMember.MemberTotal), language).ToUpperInvariant()}: {winLossMember.MemberTotal.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMember.AgentWinLoss), language).ToUpperInvariant()}: {winLossMember.AgentWinLoss.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMember.AgentComm), language).ToUpperInvariant()}: {winLossMember.AgentComm.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMember.AgentTotal), language).ToUpperInvariant()}: {winLossMember.AgentTotal.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMember.CompanyWinLoss), language).ToUpperInvariant()}: {winLossMember.CompanyWinLoss.ToFormattedNumber()}");
        }

        private void AddWinLossMemberDownline(List<string> textContainer, WinLossMember winLossMember, string language)
        {
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMember.MemberWinLoss), language).ToUpperInvariant()}: {winLossMember.MemberWinLoss.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMember.MemberComm), language).ToUpperInvariant()}: {winLossMember.MemberComm.ToFormattedNumber()}");
            textContainer.Add($"{ResourceHelper.Get(nameof(winLossMember.MemberTotal), language).ToUpperInvariant()}: {winLossMember.MemberTotal.ToFormattedNumber()}");
        }
    }
}