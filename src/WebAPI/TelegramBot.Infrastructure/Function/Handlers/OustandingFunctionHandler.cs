using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alpha.API.Client.Models.Outstanding;
using Alpha.API.Client.Services;
using TelegramBot.Infrastructure.Action;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Extensions;
using TelegramBot.Infrastructure.Shared.Resources;

namespace TelegramBot.Infrastructure.Function.Handlers
{
    public class OustandingFunctionHandler : BaseFunctionHandler, IFunctionHandler
    {
        private readonly IOutstandingService outstandingService;
        private readonly ICustomerService customerService;
        private readonly WebAPIConfiguration webAPIConfiguration;
        private readonly IActionService actionService;

        public OustandingFunctionHandler(
            IMessageBuilder messageBuilder,
            IMessageSender messageSender,
            IMessageFormatter messageFormatter,
            IOutstandingService outstandingService,
            ICustomerService customerService,
            WebAPIConfiguration webAPIConfiguration,
            IActionService actionService)
            : base(messageBuilder, messageSender, messageFormatter)
        {
            this.outstandingService = outstandingService;
            this.customerService = customerService;
            this.webAPIConfiguration = webAPIConfiguration;
            this.actionService = actionService;
        }

        public async Task Handle(CustBotAccount account, CustBotMessenger messenger, string command, string[] parameters)
        {
            var parameter = parameters[0];
            if (!string.IsNullOrWhiteSpace(parameter))
            {
                OutstandingReport data = null;
                var viewTopOutstanding = false;
                if (int.TryParse(parameter, out int topQuery))
                {
                    viewTopOutstanding = true;
                    if (topQuery <= 0 && topQuery > webAPIConfiguration.OutstandingMaxTopQuery)
                    {
                        await BuildAndSendInvalidNumerOfDownlineRangeMessage(messenger, topQuery);
                        return;
                    }

                    data = await outstandingService.GetTopOutstanding(account.CustId, account.CustomerLevel, topQuery);
                }
                else
                {
                    var downlineInfo = await customerService.GetDownlineInfo(account.CustId, parameter);
                    if (!IsValidDirectDownline(account.CustomerLevel, downlineInfo))
                    {
                        await BuildAndSendInvalidCustNameMessage(messenger, parameter);
                        return;
                    }

                    data = await outstandingService.GetDownlineOutstanding(account.CustId, account.CustomerLevel, downlineInfo.CustId, messenger.Language);
                }

                if (data == null)
                {
                    await BuildAndSendNoDataMessage(messenger);
                    return;
                }

                var message = viewTopOutstanding
                    ? BuildTopOutstandingMessage(messenger.Language, data, account.CustomerLevel)
                    : BuildDownlineOutstandingMessage(messenger.Language, data, account.CustomerLevel, parameter);

                var action = await actionService.GetActionByCommand(command);
                await actionService.InsertUserAction(account.UserId, action.ActionId, string.Empty);
                var buttons = await actionService.BuildNextActionButtons(account, action);
                await messageSender.SendMessageWithBackToMainMenuButton(messenger, message, buttons, action.FunctionId);
            }
        }

        private string BuildTopOutstandingMessage(string language, OutstandingReport data, byte custLevel)
        {
            var headerRow = string.Format(
                "{0} / {1} {2} {3}",
                ResourceHelper.Get($"CustLevel_{custLevel - 1}", language),
                ResourceHelper.Get(nameof(SharedResource.Member), language),
                messageFormatter.Point(),
                ResourceHelper.Get($"CustLevel_{custLevel}", language));

            var textContainer = new List<string>();
            AddTitleRow(textContainer, language);
            textContainer.Add($"{headerRow}{messageFormatter.NewLine()}");
            AddTotalRow(textContainer, data.Total, language);
            AddBodyMessage(textContainer, data.Outstandings);

            if (data.HiddenDownlinesCount > 0)
            {
                textContainer.Add(messageBuilder.BuildHiddenDownlinesMessage(language, data.HiddenDownlinesCount, custLevel));
            }

            return string.Join(messageFormatter.NewLine(), textContainer);
        }

        private string BuildDownlineOutstandingMessage(string language, OutstandingReport data, byte custLevel, string downlineUsername)
        {
            var headerRow = string.Format(
                "{0} / {1} {2} {3}",
                ResourceHelper.Get(nameof(SharedResource.Products), language),
                ResourceHelper.Get(nameof(SharedResource.Member), language),
                messageFormatter.Point(),
                ResourceHelper.Get($"CustLevel_{custLevel}", language));

            var textContainer = new List<string>();
            AddTitleRow(textContainer, language);
            textContainer.Add($"{ResourceHelper.Get(nameof(SharedResource.For), language)} {downlineUsername}");
            textContainer.Add($"{headerRow}{messageFormatter.NewLine()}");
            AddTotalRow(textContainer, data.Total, language);
            AddBodyMessage(textContainer, data.Outstandings);

            return string.Join(messageFormatter.NewLine(), textContainer);
        }

        private void AddTitleRow(List<string> textContainer, string language)
            => textContainer.Add($"[{ResourceHelper.Get(nameof(SharedResource.Outstanding), language)} - {DateTime.Now:yyyyMMdd}]");

        private void AddTotalRow(List<string> textContainer, OutstandingEntity total, string language)
        {
            textContainer.Add(ResourceHelper.Get(nameof(SharedResource.Total), language).ToUpperInvariant());
            textContainer.Add($"    {total.MemberOutstanding.ToFormattedNumber()} {messageFormatter.Point()} {total.Outstanding.ToFormattedNumber()}");
        }

        private void AddBodyMessage(List<string> textContainer, IEnumerable<OutstandingEntity> outstandings)
        {
            var index = 1;
            foreach (var outstanding in outstandings)
            {
                textContainer.Add($"{index}. {outstanding.Name}:");
                textContainer.Add($"    {outstanding.MemberOutstanding.ToFormattedNumber()} {messageFormatter.Point()} {outstanding.Outstanding.ToFormattedNumber()}");
                index++;
            }
        }

        public async Task<IEnumerable<string>> GetTopDownlineUsername(CustBotAccount account, int top = 5)
            => await outstandingService.GetTopUsername(account.CustId, account.CustomerLevel, top);

    }
}