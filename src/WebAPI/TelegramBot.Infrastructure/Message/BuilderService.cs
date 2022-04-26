using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fanex.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Infrastructure.Action.Models;
using TelegramBot.Infrastructure.Shared.Constants;
using TelegramBot.Infrastructure.Shared.Resources;

namespace TelegramBot.Infrastructure.Message
{
    public interface IBuilderService
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

        InlineKeyboardMarkup BuildActionButtions(string language, IList<BotAction> actions, bool withBackToMainMenuButton = false);

        string BuildActionSelectedMessage(string language, string actionName);

        InlineKeyboardMarkup BuildCancelButtion(string language, short parentActionId);
    }

    public class BuilderService : IBuilderService
    {
        private static readonly string newLineCharacter = Environment.NewLine;
        private static readonly int NumberButtonInRow = 2;

        private static readonly List<string> underMaintenanceLanguageCodes = new()
        {
            LanguageCodes.English,
            LanguageCodes.Chinese,
            LanguageCodes.Taiwan
        };

        public string BuildAccountAlreadyLinkedMessage(string language)
            => ResourceHelper.Get(nameof(SharedResource.AccountAlreadyLinked), language);

        public string BuildChangePasswordAlertMessage(string language, string ip)
            => string.Join(
                newLineCharacter,
                $"[{ResourceHelper.Get(nameof(SharedResource.ChangePasswordAlert), language)} - {DateTime.Now:yyyyMMdd}]",
                ResourceHelper.Get(nameof(SharedResource.ChangePasswordAlertContent), language),
                $"{ResourceHelper.Get(nameof(SharedResource.RequestedIP), language)}: {ip}",
                ResourceHelper.Get(nameof(SharedResource.AlertMessage), language));

        public string BuildChangeSecurityCodeAlertMessage(string language, string ip)
            => string.Join(
                newLineCharacter,
                $"[{ResourceHelper.Get(nameof(SharedResource.ChangeSecurityCodeAlert), language)} - {DateTime.Now:yyyyMMdd}]",
                ResourceHelper.Get(nameof(SharedResource.ChangeSecurityCodeAlertContent), language),
                $"{ResourceHelper.Get(nameof(SharedResource.RequestedIP), language)}: {ip}",
                ResourceHelper.Get(nameof(SharedResource.AlertMessage), language));

        public string BuildOTPMessage(string language, string otp)
            => string.Format(ResourceHelper.Get(nameof(SharedResource.Setting_OTP_Help)), otp);

        public string BuildTelegramAccountAlreadyLinkedMessage(string language)
            => ResourceHelper.Get(nameof(SharedResource.TelegramAccountAlreadyLinked), language);

        public string BuildUnderMaintenanceMessage(string from, string to)
        {
            return string.Join(
                newLineCharacter,
                underMaintenanceLanguageCodes.Select(langCode =>
                    string.Format(ResourceHelper.Get(nameof(SharedResource.UnderMaintenance), langCode), from, to)));
        }

        public string BuildHelpMessage(string language, string botName, bool hasReportPermission)
        {
            var generalCommands = string.Join(
                newLineCharacter,
                $"/menu - {string.Format(ResourceHelper.Get(nameof(SharedResource.OpenMainMenu), language), botName)}",
                $"/help - {ResourceHelper.Get(nameof(SharedResource.ListAllCommands), language)}",
                $"/settings - {ResourceHelper.Get(nameof(SharedResource.OpenSettingsMenu), language)}",
                $"/stop - Unlink {botName}",
                newLineCharacter);

            var stringBuilder = new StringBuilder();
            stringBuilder
                .Append($"{string.Format(ResourceHelper.Get(nameof(SharedResource.HelpCommandIntro), language), botName)}")
                .Append(newLineCharacter)
                .Append(newLineCharacter)
                .Append(generalCommands);

            if (hasReportPermission)
            {
                var outstandingCommands = string.Join(
                    newLineCharacter,
                    $"{ResourceHelper.Get(nameof(SharedResource.OutstandingReports), language)}:",
                    $"/os - ${ResourceHelper.Get(nameof(SharedResource.OpenOutstandingMenu), language)}",
                    $"/mos - ${ResourceHelper.Get(nameof(SharedResource.GetyourOutstandingReport), language)}",
                    $"/os <Username> - ${ResourceHelper.Get(nameof(SharedResource.GetOutstandingReportOfSpecificMember), language)}",
                    $"/os <#> - ${ResourceHelper.Get(nameof(SharedResource.GetOutstandingReportOfTopMembers), language)}",
                    newLineCharacter);

                var balanceInfoCommands = string.Join(
                    newLineCharacter,
                    $"{ResourceHelper.Get(nameof(SharedResource.BalanceInfoReports), language)}:",
                    $"/bal - {ResourceHelper.Get(nameof(SharedResource.OpenBalanceInfoMenu), language)}",
                    $"/mbal - {ResourceHelper.Get(nameof(SharedResource.GetYourBalanceInfoReport), language)}",
                    $"/bal <Username> - {ResourceHelper.Get(nameof(SharedResource.GetBalanceInfoOfSpecificMember), language)}",
                    $"/bal <#> - {ResourceHelper.Get(nameof(SharedResource.GetBalanceInfoOfTopMembers), language)}",
                    newLineCharacter);

                var winlossCommands = string.Join(
                     newLineCharacter,
                     $"{ResourceHelper.Get(nameof(SharedResource.WinLossReports), language)}:",
                     $"/wl - {ResourceHelper.Get(nameof(SharedResource.OpenTodayWinLossReportMenu), language)}",
                     $"/mwl - {ResourceHelper.Get(nameof(SharedResource.GetYourTodayWinLossReport), language)}",
                     $"/wl <Username> - {ResourceHelper.Get(nameof(SharedResource.GetTodayWinLossReportOfSpecficMember), language)}",
                     $"/ywl - {ResourceHelper.Get(nameof(SharedResource.OpenYesterdayWinLossReportMenu), language)}",
                     $"/mywl - {ResourceHelper.Get(nameof(SharedResource.GetYourYesterdayWinLossReport), language)}",
                     $"/ywl <Username> - {ResourceHelper.Get(nameof(SharedResource.GetYesterdayWinLossReportOfSpecificMember), language)}",
                     newLineCharacter);

                stringBuilder
                    .Append(outstandingCommands)
                    .Append(balanceInfoCommands)
                    .Append(winlossCommands);
            }

            return stringBuilder.ToString();
        }

        public string BuildMenuMessage(string language)
            => $"{ResourceHelper.Get(nameof(SharedResource.PleaseSelectAnAction), language)}:";

        public string BuildWelcomeMessage(string language, long chatId)
            => string.Format(ResourceHelper.Get(nameof(SharedResource.WelcomeMessage), language), chatId);

        public string BuildNotRecognizedCommandMessage(string language)
        => ResourceHelper.Get(nameof(SharedResource.NotRecognizeCommandMessage), language);

        public InlineKeyboardMarkup BuildActionButtions(string language, IList<BotAction> actions, bool withBackToMainMenuButton = false)
        {
            var numberOfRows = actions.Count % NumberButtonInRow > 0 ? (actions.Count / NumberButtonInRow) + 1 : (actions.Count / NumberButtonInRow);
            numberOfRows = withBackToMainMenuButton ? numberOfRows + 1 : numberOfRows;
            var buttonRows = new InlineKeyboardButton[numberOfRows][];

            for (var i = 0; i < numberOfRows; i++)
            {
                if (withBackToMainMenuButton && i == numberOfRows - 1)
                {
                    buttonRows[i] = new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData(
                            ResourceHelper.Get(nameof(SharedResource.BackToMainMenu),language),
                            JsonConvert.SerializeObject(new {IsBackToMainMenu=true}))
                    };
                    break;
                }

                var actionIndex = i * NumberButtonInRow;
                if (actionIndex == actions.Count - 1)
                {
                    buttonRows[i] = new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData(actions[actionIndex].ActionName, JsonConvert.SerializeObject(new { ActionId= actions[actionIndex].ActionId}))
                    };
                }
                else
                {
                    buttonRows[i] = new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData(actions[actionIndex].ActionName, JsonConvert.SerializeObject(new { ActionId= actions[actionIndex].ActionId})),
                        InlineKeyboardButton.WithCallbackData(actions[actionIndex + 1].ActionName, JsonConvert.SerializeObject(new { ActionId= actions[actionIndex + 1].ActionId}))
                    };
                }
            }

            return buttonRows;
        }

        public string BuildActionSelectedMessage(string language, string actionName)
            => $"{ResourceHelper.Get(nameof(SharedResource.ActionSelected), language)}: {actionName}";

        public InlineKeyboardMarkup BuildCancelButtion(string language, short parentActionId)
            => new(new[] { new[] { InlineKeyboardButton.WithCallbackData(ResourceHelper.Get(nameof(SharedResource.Cancel)), JsonConvert.SerializeObject(new { IsCancel = true, ActionId = parentActionId })) } });
    }
}