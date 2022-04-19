using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }

    public class BuilderService : IBuilderService
    {
        private static readonly string newLineCharacter = "\n";

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
                $"/stop - Unlink {botName}");

            var stringBuilder = new StringBuilder();
            stringBuilder
                .Append(string.Format(ResourceHelper.Get(nameof(SharedResource.HelpCommandIntro), language), botName))
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
                    $"/os <#> - ${ResourceHelper.Get(nameof(SharedResource.GetOutstandingReportOfTopMembers), language)}");

                var balanceInfoCommands = string.Join(
                    newLineCharacter,
                    $"{ResourceHelper.Get(nameof(SharedResource.BalanceInfoReports), language)}:",
                    $"/bal - {ResourceHelper.Get(nameof(SharedResource.OpenBalanceInfoMenu), language)}",
                    $"/mbal - {ResourceHelper.Get(nameof(SharedResource.GetYourBalanceInfoReport), language)}",
                    $"/bal <Username> - {ResourceHelper.Get(nameof(SharedResource.GetBalanceInfoOfSpecificMember), language)}",
                    $"/bal <#> - {ResourceHelper.Get(nameof(SharedResource.GetBalanceInfoOfTopMembers), language)}");

                var winlossCommands = string.Join(
                     newLineCharacter,
                     $"{ResourceHelper.Get(nameof(SharedResource.WinLossReports), language)}:",
                     $"/wl - {ResourceHelper.Get(nameof(SharedResource.OpenTodayWinLossReportMenu), language)}",
                     $"/mwl - {ResourceHelper.Get(nameof(SharedResource.GetYourTodayWinLossReport), language)}",
                     $"/wl <Username> - {ResourceHelper.Get(nameof(SharedResource.GetTodayWinLossReportOfSpecficMember), language)}",
                     $"/ywl - {ResourceHelper.Get(nameof(SharedResource.OpenYesterdayWinLossReportMenu), language)}",
                     $"/mywl - {ResourceHelper.Get(nameof(SharedResource.GetYourYesterdayWinLossReport), language)}",
                     $"/ywl <Username> - {ResourceHelper.Get(nameof(SharedResource.GetYesterdayWinLossReportOfSpecificMember), language)}");

                stringBuilder
                    .Append(newLineCharacter)
                    .Append(outstandingCommands)
                    .Append(newLineCharacter)
                    .Append(balanceInfoCommands)
                    .Append(newLineCharacter)
                    .Append(winlossCommands);
            }

            return stringBuilder.ToString();
        }
    }
}