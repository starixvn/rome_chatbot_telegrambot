using System.Text;
using Newtonsoft.Json;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Modules.Action.Constants;
using TelegramBot.Modules.Message.Models;
using TelegramBot.Modules.Resource;
using TelegramBot.Modules.Resource.constants;
using TelegramBot.Shared.Constants;
using TelegramBot.Shared.Resources;
using TelegramBot.Utilities.Extensions;

namespace TelegramBot.Modules.Message
{
    public interface IMessageBuilder
    {
        string BuildIntroductionMessage(string system);

        string BuildUnderMaintenanceMessage(string from, string to, string system);

        string BuildChangePasswordAlertMessage(string language);

        string BuildChangeSecurityCodeAlertMessage(string language);

        string BuildWrongPasswordAlertMessage(string language, string ip);

        string BuildAccountAlreadyLinkedMessage(string language);

        string BuildTelegramAccountAlreadyLinkedMessage(string language);

        string BuildSettingOTPMessage(string language, string otp);

        string BuildMenuMessage(string language);

        string BuildWelcomeMessage(string language, string username);

        string BuildNotRecognizedCommandMessage(string language);

        string BuildActionSelectedMessage(string language, string actionName);

        string BuildInvalidNumberOfDownlineRange(string language, string userResponse, int maxTopQuery);

        string BuildInvalidCustNameMessage(string language, string invalidCustName);

        string BuildNoDataMessage(string language);

        string BuildAccountClosedMessage(string language);

        string BuildAccountLockedMessage(string language);

        string BuildUnlinkMessage(string language, string botName);

        Models.Message BuildMessage(string text, IList<MessageButton> buttons, string language);

        Models.Message BuildMessageWithBackButton(string text, IList<MessageButton> buttons, short backToActionId, string language, bool isEndAction = false);

        Models.Message BuildStopUsingChatbotConfirmMessage(string language, string chatbotName);

        IMessageFormatter GetFormatter();

        string BuildInvalidUsernameMessage(string language);

        string BuildInvalidDownlinesMessage(string language, int max);

        string BuildUpdateNotificationSettingMessage(string language, bool enabled);

        string BuildStopUsingChatbotMessage(string language);

        string BuildMainMenuButtonText(string language);

        string BuildBackToMainMenuButtonText(string language);

        public string BuildGeneralCommandsHelpMessage(string language, string botName);

        public string BuildLanguageUpdatedMessage(string language);

        public string BuildMinimizeUMMessage(string language, string startTime, string endTime);

        public string BuildOTPMessage(string language, string otp);
    }

    public class MessageBuilder : IMessageBuilder
    {
        private readonly string Site789 = "789Y";
        private readonly int NumberButtonInRow = 2;
        private readonly IMessageFormatter messageFormatter;
        private readonly IResourceService resourceService;

        public MessageBuilder(
            IMessageFormatter messageFormatter,
            IResourceService resourceService)
        {
            this.messageFormatter = messageFormatter;
            this.resourceService = resourceService;
        }

        public IMessageFormatter GetFormatter() => messageFormatter;

        public string BuildIntroductionMessage(string system)
            => string.Join(
                messageFormatter.NewLine(),
                ResourceHelper.Get(nameof(SharedResource.IntroductionMessage)),
                ResourceHelper.Get(nameof(SharedResource.IntroductionMessage), system == Site789 ? LanguageCodes.Vietnamese : LanguageCodes.ChineseSimplified));

        public string BuildAccountAlreadyLinkedMessage(string language)
            => ResourceHelper.Get(nameof(SharedResource.AccountAlreadyLinked), language);

        public string BuildChangePasswordAlertMessage(string language)
            => string.Join(
                messageFormatter.NewLine(),
                $"[{ResourceHelper.Get(nameof(SharedResource.ChangePasswordAlert), language)} - {DateTime.Now.ToSimpleDate()}]",
                ResourceHelper.Get(nameof(SharedResource.ChangePasswordAlertContent), language),
                ResourceHelper.Get(nameof(SharedResource.AlertChangePasswordMessage), language));

        public string BuildChangeSecurityCodeAlertMessage(string language)
            => string.Join(
                messageFormatter.NewLine(),
                $"[{ResourceHelper.Get(nameof(SharedResource.ChangeSecurityCodeAlert), language)} - {DateTime.Now.ToSimpleDate()}]",
                ResourceHelper.Get(nameof(SharedResource.ChangeSecurityCodeAlertContent), language),
                ResourceHelper.Get(nameof(SharedResource.AlertMessage), language));

        public string BuildSettingOTPMessage(string language, string otp)
            => string.Format(ResourceHelper.Get(nameof(SharedResource.Setting_OTP_Help), language), otp);

        public string BuildTelegramAccountAlreadyLinkedMessage(string language)
            => ResourceHelper.Get(nameof(SharedResource.TelegramAccountAlreadyLinked), language);

        public string BuildUnderMaintenanceMessage(string from, string to, string system)
        {
            return string.Join(
                messageFormatter.NewLine(),
                string.Format(ResourceHelper.Get(nameof(SharedResource.UnderMaintenance)), from, to),
                string.Format(ResourceHelper.Get(nameof(SharedResource.UnderMaintenance), system == Site789 ? LanguageCodes.Vietnamese : LanguageCodes.ChineseSimplified), from, to));
        }

        public string BuildGeneralCommandsHelpMessage(string language, string botName)
        {
            var generalCommands = string.Join(
                messageFormatter.NewLine(),
                $"/menu - {string.Format(ResourceHelper.Get(nameof(SharedResource.OpenMainMenu), language), botName)}",
                $"/help - {ResourceHelper.Get(nameof(SharedResource.ListAllCommands), language)}",
                $"/settings, /setting - {ResourceHelper.Get(nameof(SharedResource.OpenSettingsMenu), language)}",
                $"/stop, /unlink - {ResourceHelper.Get(nameof(SharedResource.Unlink), language)} {botName}");

            var stringBuilder = new StringBuilder($"{string.Format(ResourceHelper.Get(nameof(SharedResource.HelpCommandIntro), language), botName)}");
            stringBuilder
                .Append(messageFormatter.NewLine())
                .Append(messageFormatter.NewLine())
                .Append(generalCommands);

            return stringBuilder.ToString();
        }

        public string BuildMenuMessage(string language)
            => $"{ResourceHelper.Get(nameof(SharedResource.PleaseSelectAnAction), language)}:";

        public string BuildWelcomeMessage(string language, string username)
            => string.Format(ResourceHelper.Get(nameof(SharedResource.WelcomeMessage), language), username);

        public string BuildNotRecognizedCommandMessage(string language)
            => ResourceHelper.Get(nameof(SharedResource.NotRecognizeCommandMessage), language);

        public string BuildActionSelectedMessage(string language, string actionName)
            => $"{ResourceHelper.Get(nameof(SharedResource.ActionSelected), language)}: {actionName}";

        public string BuildInvalidNumberOfDownlineRange(string language, string userResponse, int maxTopQuery)
            => string.Format(ResourceHelper.Get(nameof(SharedResource.InvalidNumberOfDownlinesRange), language), userResponse, maxTopQuery);

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

        public Models.Message BuildMessage(string text, IList<MessageButton> buttons, string language)
            => new(text, BuildReplyMarkup(language, buttons));

        public Models.Message BuildMessageWithBackButton(string text, IList<MessageButton> buttons, short backToActionId, string language, bool isEndAction = false)
            => new(text, BuildReplyMarkup(language, buttons, backToActionId, isEndAction));

        private InlineKeyboardMarkup BuildReplyMarkup(string language, IList<MessageButton> buttons, short backToActionId = -1, bool isEndAction = false)
        {
            var hasBackButton = backToActionId >= 0;
            var numberOfRows = (buttons.Count % NumberButtonInRow > 0 ? (buttons.Count / NumberButtonInRow) + 1 : (buttons.Count / NumberButtonInRow)) + (hasBackButton ? 1 : 0);
            var buttonRows = new InlineKeyboardButton[numberOfRows][];

            for (var i = 0; i < numberOfRows; i++)
            {
                if (hasBackButton && i == numberOfRows - 1)
                {
                    var mainMenuButton = BuildMainMenuButton(isEndAction, language);
                    if (backToActionId > 0)
                    {
                        var backToAction = $"{Unicode.Back} {ResourceHelper.Get($"BackTo_{backToActionId}", language)}";
                        buttonRows[i] = new[]
                        {
                            InlineKeyboardButton.WithCallbackData(backToAction, JsonConvert.SerializeObject(new { Id = backToActionId })),
                            InlineKeyboardButton.WithCallbackData(mainMenuButton.Text, mainMenuButton.CallBackData),
                        };
                    }
                    else
                    {
                        buttonRows[i] = new[]
                        {
                            InlineKeyboardButton.WithCallbackData(mainMenuButton.Text, mainMenuButton.CallBackData),
                        };
                    }
                }
                else
                {
                    var actionIndex = i * NumberButtonInRow;
                    buttonRows[i] = actionIndex == buttons.Count - 1
                        ? new[] { InlineKeyboardButton.WithCallbackData(buttons[actionIndex].Text, buttons[actionIndex].CallBackData) }
                        : new[]
                        {
                        InlineKeyboardButton.WithCallbackData(buttons[actionIndex].Text, buttons[actionIndex].CallBackData),
                        InlineKeyboardButton.WithCallbackData(buttons[actionIndex + 1].Text, buttons[actionIndex + 1].CallBackData)
                        };
                }
            }

            return buttonRows;
        }

        private MessageButton BuildMainMenuButton(bool isEndAction, string language)
        {
            return isEndAction
                ? new MessageButton(BuildMainMenuButtonText(language), JsonConvert.SerializeObject(new { IsMainMenu = true }))
                : new MessageButton(BuildBackToMainMenuButtonText(language), JsonConvert.SerializeObject(new { Id = ActionIds.MainMenu }));
        }

        public string BuildMainMenuButtonText(string language)
            => $"{Unicode.MainMenu} {ResourceHelper.Get(nameof(SharedResource.MainMenu), language)}";

        public string BuildBackToMainMenuButtonText(string language)
            => $"{Unicode.Back} {ResourceHelper.Get(nameof(SharedResource.BackToMainMenu), language)}";

        public Models.Message BuildStopUsingChatbotConfirmMessage(string language, string chatbotName)
        {
            var text = string.Format(ResourceHelper.Get(nameof(SharedResource.StopUsingChatbotConfirmMessage), language), chatbotName);
            var buttons = new List<MessageButton>
            {
                new MessageButton(ResourceHelper.Get(nameof(SharedResource.No), language), JsonConvert.SerializeObject(new { Id = ActionIds.Settings })),
                new MessageButton(BuildStopUsingChatbotMessage(language), JsonConvert.SerializeObject(new { IsStopUsingChatbot = true }))
            };

            return new Models.Message(text, BuildReplyMarkup(language, buttons));
        }

        public string BuildStopUsingChatbotMessage(string language)
            => ResourceHelper.Get(nameof(SharedResource.StopUsingChatbot), language);

        public string BuildUnlinkMessage(string language, string botName)
            => string.Format(ResourceHelper.Get(nameof(SharedResource.UnlinkMessage), language), botName);

        public string BuildInvalidUsernameMessage(string language)
            => ResourceHelper.Get(nameof(SharedResource.InvalidUsername), language);

        public string BuildInvalidDownlinesMessage(string language, int max)
            => string.Format(ResourceHelper.Get(nameof(SharedResource.InvalidNumberOfDownlines), language), max);

        public string BuildUpdateNotificationSettingMessage(string language, bool enabled)
            => ResourceHelper.Get(enabled ? nameof(SharedResource.TurnOnNotification) : nameof(SharedResource.TurnOffNotification), language);

        public string BuildLanguageUpdatedMessage(string language)
            => ResourceHelper.Get(nameof(SharedResource.LanguageUpdated), language);

        public string BuildMinimizeUMMessage(string language, string startTime, string endTime)
            => string.Format(ResourceHelper.Get(nameof(SharedResource.ApiException), language), startTime, endTime);

        public string BuildWrongPasswordAlertMessage(string language, string ip)
            => string.Join(
                messageFormatter.NewLine(),
                $"[{ResourceHelper.Get(nameof(SharedResource.WrongPasswordAlert), language)} - {DateTime.Now.ToSimpleDate()}]",
                ResourceHelper.Get(nameof(SharedResource.WrongPasswordAlertContent), language),
                $"{ResourceHelper.Get(nameof(SharedResource.RequestedIP), language)}: {ip}",
                ResourceHelper.Get(nameof(SharedResource.AlertChangePasswordMessage), language));

        public string BuildOTPMessage(string language, string otp)
            => string.Format(ResourceHelper.Get(nameof(SharedResource.OTPMessage), language), otp);
    }
}