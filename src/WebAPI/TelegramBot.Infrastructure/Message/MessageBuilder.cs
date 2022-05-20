using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Infrastructure.Action.Constants;
using TelegramBot.Infrastructure.Message.Models;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Resource.constants;
using TelegramBot.Infrastructure.Shared.Constants;
using TelegramBot.Infrastructure.Shared.Resources;

namespace TelegramBot.Infrastructure.Message
{
    public interface IMessageBuilder
    {
        string BuildUnderMaintenanceMessage(string from, string to);

        string BuildChangePasswordAlertMessage(string language, string ip);

        string BuildChangeSecurityCodeAlertMessage(string language, string ip);

        string BuildAccountAlreadyLinkedMessage(string language);

        string BuildTelegramAccountAlreadyLinkedMessage(string language);

        string BuildOTPMessage(string language, string otp);

        string BuildHelpMessage(string language, string botName, bool hasReportPermission);

        string BuildMenuMessage(string language);

        string BuildWelcomeMessage(string language, long chatId);

        string BuildNotRecognizedCommandMessage(string language);

        string BuildActionSelectedMessage(string language, string actionName);

        string BuildInvalidNumerOfDownlineRange(string language, string userResponse, int maxTopQuery);

        string BuildHiddenDownlinesMessage(string language, int count, byte custLevel);

        string BuildInvalidCustNameMessage(string language, string invalidCustName);

        string BuildNoDataMessage(string language);

        string BuildAccountClosedMessage(string language);

        string BuildAccountLockedMessage(string language);

        string BuildUnlinkMessage(string language, string botName);

        Task<Models.Message> BuildMessage(string text, IList<MessageButton> buttons, string language);

        Task<Models.Message> BuildMessageWithBackButton(string text, IList<MessageButton> buttons, short backToActionId, string language);

        Task<Models.Message> BuildStopUsingChatbotConfirmMessage(string language, string chatbotName);
    }

    public class MessageBuilder : IMessageBuilder
    {
        private static readonly List<string> underMaintenanceLanguageCodes = new()
        {
            LanguageCodes.English,
            LanguageCodes.Chinese,
            LanguageCodes.Taiwan
        };
        private readonly int NumberButtonInRow = 2;
        private readonly IMessageFormatter messageFormatter;
        private readonly IResourceService resourceService;

        public MessageBuilder(IMessageFormatter messageFormatter, IResourceService resourceService)
        {
            this.messageFormatter = messageFormatter;
            this.resourceService = resourceService;
        }

        public string BuildAccountAlreadyLinkedMessage(string language)
            => ResourceHelper.Get(nameof(SharedResource.AccountAlreadyLinked), language);

        public string BuildChangePasswordAlertMessage(string language, string ip)
            => string.Join(
                messageFormatter.NewLine(),
                $"[{ResourceHelper.Get(nameof(SharedResource.ChangePasswordAlert), language)} - {DateTime.Now:yyyyMMdd}]",
                ResourceHelper.Get(nameof(SharedResource.ChangePasswordAlertContent), language),
                $"<b>{ResourceHelper.Get(nameof(SharedResource.RequestedIP), language)}</b>: {ip}",
                ResourceHelper.Get(nameof(SharedResource.AlertMessage), language));

        public string BuildChangeSecurityCodeAlertMessage(string language, string ip)
            => string.Join(
                messageFormatter.NewLine(),
                $"[{ResourceHelper.Get(nameof(SharedResource.ChangeSecurityCodeAlert), language)} - {DateTime.Now:yyyyMMdd}]",
                ResourceHelper.Get(nameof(SharedResource.ChangeSecurityCodeAlertContent), language),
                $"<b>{ResourceHelper.Get(nameof(SharedResource.RequestedIP), language)}</b>: {ip}",
                ResourceHelper.Get(nameof(SharedResource.AlertMessage), language));

        public string BuildOTPMessage(string language, string otp)
            => string.Format(ResourceHelper.Get(nameof(SharedResource.Setting_OTP_Help)), otp);

        public string BuildTelegramAccountAlreadyLinkedMessage(string language)
            => ResourceHelper.Get(nameof(SharedResource.TelegramAccountAlreadyLinked), language);

        public string BuildUnderMaintenanceMessage(string from, string to)
        {
            return string.Join(
                messageFormatter.NewLine(),
                underMaintenanceLanguageCodes.Select(langCode =>
                    string.Format(ResourceHelper.Get(nameof(SharedResource.UnderMaintenance), langCode), from, to)));
        }

        public string BuildHelpMessage(string language, string botName, bool hasReportPermission)
        {
            var generalCommands = string.Join(
                messageFormatter.NewLine(),
                $"/menu - {string.Format(ResourceHelper.Get(nameof(SharedResource.OpenMainMenu), language), botName)}",
                $"/help - {ResourceHelper.Get(nameof(SharedResource.ListAllCommands), language)}",
                $"/settings - {ResourceHelper.Get(nameof(SharedResource.OpenSettingsMenu), language)}",
                $"/stop - Unlink {botName}");

            var stringBuilder = new StringBuilder($"{string.Format(ResourceHelper.Get(nameof(SharedResource.HelpCommandIntro), language), botName)}");
            stringBuilder
                .AppendLine()
                .AppendLine(generalCommands);


            return stringBuilder.ToString();
        }

        public string BuildMenuMessage(string language)
            => $"{ResourceHelper.Get(nameof(SharedResource.PleaseSelectAnAction), language)}:";

        public string BuildWelcomeMessage(string language, long chatId)
            => string.Format(ResourceHelper.Get(nameof(SharedResource.WelcomeMessage), language), chatId);

        public string BuildNotRecognizedCommandMessage(string language)
        => ResourceHelper.Get(nameof(SharedResource.NotRecognizeCommandMessage), language);

        public string BuildActionSelectedMessage(string language, string actionName)
            => $"{ResourceHelper.Get(nameof(SharedResource.ActionSelected), language)}: {actionName}";

        public string BuildInvalidNumerOfDownlineRange(string language, string userResponse, int maxTopQuery)
            => string.Format(ResourceHelper.Get(nameof(SharedResource.InvalidNumerOfDownlinesRange), language), userResponse, maxTopQuery);

        public string BuildHiddenDownlinesMessage(string language, int count, byte custLevel)
            => string.Format(
                ResourceHelper.Get(nameof(SharedResource.HiddenDownlinesCount), language),
                count,
                $"{ResourceHelper.Get($"CustLevel_{custLevel}", language)}(s)");

        public string BuildInvalidCustNameMessage(string language, string invalidCustName)
            => string.Format(
                ResourceHelper.Get(nameof(SharedResource.InvalidCustName), language),
                invalidCustName);

        public async Task<string> BuildActionName(string language, short actionId)
            => (await resourceService.GetResource(actionId.ToString(), ResourceType.ActionName, language)).Value;

        public string BuildNoDataMessage(string language) => ResourceHelper.Get(nameof(SharedResource.NoData), language);

        public string BuildAccountClosedMessage(string language)
            => ResourceHelper.Get(nameof(SharedResource.YourAccountIsClosed), language);

        public string BuildAccountLockedMessage(string language)
            => ResourceHelper.Get(nameof(SharedResource.YourAccountIsLocked), language);



        public async Task<Models.Message> BuildMessage(string text, IList<MessageButton> buttons, string language)
            => new Models.Message(text, await BuildReplyMarkup(language, buttons));

        public async Task<Models.Message> BuildMessageWithBackButton(string text, IList<MessageButton> buttons, short backToActionId, string language)
            => new Models.Message(text, await BuildReplyMarkup(language, buttons, backToActionId));

        private async Task<InlineKeyboardMarkup> BuildReplyMarkup(string language, IList<MessageButton> buttons, short backToActionId = -1)
        {
            var hasBackButton = backToActionId >= 0;
            var numberOfRows = (buttons.Count % NumberButtonInRow > 0 ? (buttons.Count / NumberButtonInRow) + 1 : (buttons.Count / NumberButtonInRow)) + (hasBackButton ? 1 : 0);
            var buttonRows = new InlineKeyboardButton[numberOfRows][];

            for (var i = 0; i < numberOfRows; i++)
            {
                if (hasBackButton && i == numberOfRows - 1)
                {
                    var backToMainMenu = $"<< {ResourceHelper.Get(nameof(SharedResource.BackToMainMenu), language)}";
                    if (backToActionId > 0)
                    {
                        var backToAction = $"<< {ResourceHelper.Get(nameof(SharedResource.BackTo), language)} {await BuildActionName(language, backToActionId)}";
                        buttonRows[i] = new InlineKeyboardButton[]
                        {
                            InlineKeyboardButton.WithCallbackData(backToAction, JsonConvert.SerializeObject(new { ActionId = backToActionId, Value = backToAction })),
                            InlineKeyboardButton.WithCallbackData(backToMainMenu, JsonConvert.SerializeObject(new { ActionId = ActionIds.MainMenu, Value = backToMainMenu })),
                        };
                    }
                    else
                    {
                        buttonRows[i] = new InlineKeyboardButton[]
                        {
                            InlineKeyboardButton.WithCallbackData(backToMainMenu, JsonConvert.SerializeObject(new { ActionId = ActionIds.MainMenu, Value = backToMainMenu })),
                        };
                    }


                    break;
                }

                var actionIndex = i * NumberButtonInRow;
                if (actionIndex == buttons.Count - 1)
                {
                    buttonRows[i] = new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData(buttons[actionIndex].Text, buttons[actionIndex].CallBackData)
                    };
                }
                else
                {
                    buttonRows[i] = new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData(buttons[actionIndex].Text, buttons[actionIndex].CallBackData),
                        InlineKeyboardButton.WithCallbackData(buttons[actionIndex + 1].Text, buttons[actionIndex + 1].CallBackData)
                    };
                }
            }

            return buttonRows;
        }

        public async Task<Models.Message> BuildStopUsingChatbotConfirmMessage(string language, string chatbotName)
        {
            var text = string.Format(ResourceHelper.Get(nameof(SharedResource.StopUsingChatbotConfirmMessage), language), chatbotName);
            var buttons = new List<MessageButton>()
            {
                new MessageButton(ResourceHelper.Get(nameof(SharedResource.No), language), JsonConvert.SerializeObject(new { ActionId = ActionIds.Settings })),
                new MessageButton(ResourceHelper.Get(nameof(SharedResource.StopUsingChatbot), language), JsonConvert.SerializeObject(new { IsStopUsingChatbot = true }))
            };

            return new Models.Message(text, await BuildReplyMarkup(language, buttons));
        }

        public string BuildUnlinkMessage(string language, string botName)
            => string.Format(ResourceHelper.Get(nameof(SharedResource.UnlinkMessage), language), botName);
    }
}