using Alpha.API.Client.Models.WinLoss;
using Alpha.API.Client.Services;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
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

        protected BaseWinLossFunctionHandler(
            IMessageBuilder messageBuilder,
            IMessageFormatter messageFormatter,
            IWinLossService winLossService,
            ICustomerService customerService)
            : base(messageBuilder, messageFormatter)
        {
            this.winLossService = winLossService;
            this.customerService = customerService;
        }

        public async Task<Result> HandleWinLoss(CustBotAccount account, string[] parameters, DateTime dateTime)
        {
            var parameter = parameters[0]?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return Result.CreateErrorResult();
            }

            if (parameter == "m")
            {
                var data = await winLossService.GetWinLoss(account.CustId, account.CustomerLevel, dateTime);
                return Result.CreateSuccessfulResult(BuildWinLossMessage(account.Language, data, account.CustomerLevel));
            }
            else
            {
                var downlineInfo = await customerService.GetDownlineInfo(account.CustId, parameter);
                if (!IsValidDirectDownline(account.CustomerLevel, downlineInfo))
                {
                    return Result.CreateErrorResult(messageBuilder.BuildInvalidUsernameMessage(account.Language));
                }

                var data = await winLossService.GetDownlineWinLoss(account.CustId, account.CustomerLevel, dateTime, downlineInfo.CustId);
                return Result.CreateSuccessfulResult(BuildDownlineWinLossMessage(account.Language, data, parameter, downlineInfo.RoleId));
            }
        }

        private string BuildWinLossMessage(string language, WinLossBase winLoss, byte custLevel)
        {
            var textContainer = new List<string>();
            AddMyWinLossTitleRow(textContainer, language);
            textContainer.Add("<pre>");
            AddBodyMessage(textContainer, winLoss, language, custLevel);
            textContainer.Add("</pre>");
            return string.Join(messageFormatter.NewLine(), textContainer);
        }

        private string BuildDownlineWinLossMessage(string language, WinLossBase winLoss, string downlineUsername, byte custLevel)
        {
            var textContainer = new List<string>();
            AddDownlineWinLossTitleRow(textContainer, language);
            textContainer.Add("<pre>");
            textContainer.Add(messageFormatter.FormatRow(ResourceHelper.Get(nameof(SharedResource.Username), language).ToUpperInvariant(), downlineUsername, language));
            AddBodyMessage(textContainer, winLoss, language, custLevel);
            textContainer.Add("</pre>");
            return string.Join(messageFormatter.NewLine(), textContainer);
        }

        protected abstract void AddMyWinLossTitleRow(List<string> textContainer, string language);

        protected abstract void AddDownlineWinLossTitleRow(List<string> textContainer, string language);

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

            FormatMessages(textContainer, messages, language);
        }

        private static Dictionary<string, decimal> AddWinLossMaster(WinLossMaster winLossMaster)
            => new()
            {
                {nameof(winLossMaster.Turnover), winLossMaster.Turnover},
                {nameof(winLossMaster.GrossComm), winLossMaster.GrossComm},
                {nameof(winLossMaster.MasterTotal), winLossMaster.MasterTotal},
                {nameof(winLossMaster.SuperWinLoss), winLossMaster.SuperWinLoss},
                {nameof(winLossMaster.SuperComm), winLossMaster.SuperComm},
                {nameof(winLossMaster.SuperTotal), winLossMaster.SuperTotal},
                {nameof(winLossMaster.CompanyWinLoss), winLossMaster.CompanyWinLoss},
            };


        private static Dictionary<string, decimal> AddWinLossAgent(WinLossAgent winLossAgent)
            => new()
            {
                {nameof(winLossAgent.Turnover), winLossAgent.Turnover},
                {nameof(winLossAgent.GrossComm), winLossAgent.GrossComm},
                {nameof(winLossAgent.AgentTotal), winLossAgent.AgentTotal},
                {nameof(winLossAgent.MasterWinLoss), winLossAgent.MasterWinLoss},
                {nameof(winLossAgent.MasterComm), winLossAgent.MasterComm},
                {nameof(winLossAgent.MasterTotal), winLossAgent.MasterTotal},
                {nameof(winLossAgent.CompanyWinLoss), winLossAgent.CompanyWinLoss},
            };

        private static Dictionary<string, decimal> AddWinLossMember(WinLossMember winLossMember)
            => new()
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


        private static Dictionary<string, decimal> AddWinLossMemberDownline(WinLossMember winLossMember)
            => new()
            {
                {nameof(winLossMember.MemberWinLoss), winLossMember.MemberWinLoss},
                {nameof(winLossMember.MemberComm), winLossMember.MemberComm},
                {nameof(winLossMember.MemberTotal), winLossMember.MemberTotal}
            };

        private void FormatMessages(List<string> textContainer, Dictionary<string, decimal> messages, string language)
        {
            foreach (var message in messages)
            {
                var key = ResourceHelper.Get(message.Key, language).ToUpperInvariant();
                var value = message.Value.ToFormattedNumber();
                textContainer.Add(messageFormatter.FormatRow(key, value, language));
            }
        }
    }
}