using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

        Task<string> BuildActionName(string language, short actionId);

        Models.MessageButton BuildCancelButton(string language, short backToActionId);

        string BuildNoDataMessage(string language);

        string BuildAccountClosedMessage(string language);

        string BuildAccountLockedMessage(string language);
    }

    public class MessageBuilder : IMessageBuilder
    {
        private static readonly List<string> underMaintenanceLanguageCodes = new()
        {
            LanguageCodes.English,
            LanguageCodes.Chinese,
            LanguageCodes.Taiwan
        };

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
                ResourceHelper.Get($"CustLevel_{custLevel}", language));

        public string BuildInvalidCustNameMessage(string language, string invalidCustName)
            => string.Format(
                ResourceHelper.Get(nameof(SharedResource.InvalidCustName), language),
                invalidCustName);

        public async Task<string> BuildActionName(string language, short actionId)
            => (await resourceService.GetResource(actionId.ToString(), ResourceType.ActionName, language)).Value;

        public Models.MessageButton BuildCancelButton(string language, short backToActionId)
            => new Models.MessageButton(ResourceHelper.Get(nameof(SharedResource.Cancel)), JsonConvert.SerializeObject(new { IsCancel = true, ActionId = backToActionId }));

        public string BuildNoDataMessage(string language) => ResourceHelper.Get(nameof(SharedResource.NoData), language);

        public string BuildAccountClosedMessage(string language)
            => ResourceHelper.Get(nameof(SharedResource.YourAccountIsClosed), language);

        public string BuildAccountLockedMessage(string language)
            => ResourceHelper.Get(nameof(SharedResource.YourAccountIsLocked), language);
    }
}