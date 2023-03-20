using System.Net;
using System.Text;
using Alpha.API.Client.Exceptions;
using TelegramBot.Modules.Action;
using TelegramBot.Modules.Action.Constants;
using TelegramBot.Modules.Action.Models;
using TelegramBot.Modules.Action.Validators;
using TelegramBot.Modules.Bot.Models;
using TelegramBot.Modules.Command.Models;
using TelegramBot.Modules.Function.Constants;
using TelegramBot.Modules.Function.Handlers;
using TelegramBot.Modules.Function.Models;
using TelegramBot.Modules.Message;
using TelegramBot.Modules.Message.Constants;
using TelegramBot.Modules.Message.Models;
using TelegramBot.Modules.Setting;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Constants;
using TelegramBot.Shared.Models;

namespace TelegramBot.Modules.Command
{
    public interface ICommandService
    {
        Task HandleUserReplyByText(BotEntity bot, long chatId, string message);

        Task HandleUserReplyByButton(BotEntity bot, long chatId, int messageId, string callbackId, CallbackData callbackData);

        Task SendMainMenuMessage(CustBotAccount account, int messageId = 0);
    }

    public class CommandService : ICommandService
    {
        private readonly IMessageBuilder messageBuilder;
        private readonly IMessageSender messageSender;
        private readonly IMessageLogger messageLogger;
        private readonly ISettingService settingService;
        private readonly IActionService actionService;
        private readonly IFunctionService functionService;
        private readonly IFunctionHandlerFactory functionHandlerFactory;
        private readonly IActionValidatorFactory actionValidatorFactory;

        public CommandService(
            IMessageBuilder messageBuilder,
            IMessageSender messageSender,
            IMessageLogger messageLogger,
            ISettingService settingService,
            IActionService actionService,
            IFunctionService functionService,
            IFunctionHandlerFactory functionHandlerFactory,
            IActionValidatorFactory actionValidatorFactory)
        {
            this.messageBuilder = messageBuilder;
            this.messageSender = messageSender;
            this.messageLogger = messageLogger;
            this.settingService = settingService;
            this.actionService = actionService;
            this.functionService = functionService;
            this.functionHandlerFactory = functionHandlerFactory;
            this.actionValidatorFactory = actionValidatorFactory;
        }

        public async Task HandleUserReplyByText(BotEntity bot, long chatId, string message)
        {
            var userResponse = WebUtility.HtmlEncode(
                string.Join(
                    " ",
                    message.Split(
                        default(string[]),
                        StringSplitOptions.RemoveEmptyEntries)));

            var account = await settingService.GetCustBotAccount(chatId, bot.BotId);
            if (IsAccountInvalid(account))
            {
                return;
            }

            Messenger messenger = new(bot.BotId, chatId);
            try
            {
                if (IsCommand(userResponse))
                {
                    var function = GetCommandFunction(account, userResponse);
                    var handle = userResponse.ToLowerInvariant().Split(' ')[0] switch
                    {
                        "/help" => HandleHelpCommand(account, bot.DisplayName),
                        "/menu" => SendMainMenuMessage(account),
                        "/unlink" => HandleStopCommand(account, bot.DisplayName),
                        _ => HandleOtherCommand(account, userResponse)
                    };

                    await Task.WhenAll(
                        messageLogger.LogMessage(account.UserId, CommonId.BotUserId, messenger, message, MessageSendStatus.Success, functionId: function != null ? function.FunctionId : (short)0),
                        handle);
                }
                else
                {
                    await HandleOtherResponse(account, messenger, userResponse);
                }
            }
            catch (MinimizeUMException ex)
            {
                await SendMinimizeUMMessage(account, ex);
            }
        }

        private async Task HandleOtherCommand(CustBotAccount account, string command)
        {
            var function = GetCommandFunction(account, command.Split(' ')[0]);

            if (function == null)
            {
                await SendNotRecognizedCommandMessage(account);
                return;
            }

            var action = await actionService.GetActionByCommand(command, account.System);
            if (action != null)
            {
                await Task.WhenAll(
                    actionService.InsertUserAction(account.UserId, action.ActionId, string.Empty),
                    action.IsEndAction ? HandleCommand(account, command, function.FunctionId) : SendNextActionsMessage(account, action));
                return;
            }

            await HandleCommand(account, command, function.FunctionId);
        }

        private async Task HandleHelpCommand(CustBotAccount account, string botName)
        {
            var message = messageBuilder.BuildGeneralCommandsHelpMessage(account.Language, botName);
            var builder = new StringBuilder(message);
            var formatter = messageBuilder.GetFormatter();
            foreach (var report in account.Functions.Where(f => f.Type == FunctionTypes.Report))
            {
                builder.Append(formatter.NewLine());
                builder.Append($"{formatter.NewLine()}{formatter.Bold(await functionService.GetFunctionName(report.FunctionId, account.Language))}");
                var rootAction = await actionService.GetRootActionByFunctionId(account, report.FunctionId);
                builder.Append($"{formatter.NewLine()}{rootAction.Command} - {await actionService.GetActionDescription(rootAction.ActionId, account.Language)}");
                var childActions = (await actionService.GetChildActions(account, rootAction.ActionId)).Where(a => a.Type != ActionTypes.Downline && a.Type != ActionTypes.WebApp);
                foreach (var childAction in childActions)
                {
                    builder.Append($"{formatter.NewLine()}{rootAction.Command} {childAction.Command} - {await actionService.GetActionDescription(childAction.ActionId, account.Language)}");
                }
            }

            await messageSender.SendMessage(account, builder.ToString());
        }

        private static bool IsCommand(string userResponse)
            => userResponse.StartsWith("/");

        private static bool IsAccountInvalid(CustBotAccount account)
        {
            if (account == null)
            {
                return true;
            }

            return false;
        }

        private static BotFunction GetCommandFunction(CustBotAccount account, string command)
            => account.Functions.FirstOrDefault(f =>
                !string.IsNullOrEmpty(command)
                && !string.IsNullOrEmpty(f.Command)
                && command.ToLowerInvariant().Equals(f.Command.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase));

        private async Task HandleOtherResponse(CustBotAccount account, Messenger messenger, string userResponse)
        {
            var userActions = await actionService.GetUserActions(account.UserId);
            var lastUserAction = userActions.LastOrDefault();
            var currentAction = lastUserAction != null ? await actionService.GetActionById(lastUserAction.ActionId, account.System) : null;
            if (lastUserAction == null || currentAction == null)
            {
                await Task.WhenAll(
                    messageLogger.LogMessage(account.UserId, CommonId.BotUserId, messenger, userResponse, MessageSendStatus.Success),
                    SendNotRecognizedCommandMessage(account));
                return;
            }

            await Task.Run(() => messageLogger.LogMessage(account.UserId, CommonId.BotUserId, messenger, userResponse, MessageSendStatus.Success, functionId: currentAction.FunctionId));

            if (currentAction.Type == ActionTypes.Message)
            {
                await actionService.UpdateUserAction(lastUserAction.Id, userResponse);
                var validator = actionValidatorFactory.GetValidator(currentAction.ActionId);
                if (validator != null)
                {
                    var validationResult = await validator.Validate(account, userResponse);
                    if (validationResult.IsNotOk)
                    {
                        var prevAction = await actionService.GetActionById(currentAction.ParentId, account.System);
                        await messageSender.SendMessage(
                            account: account,
                            message: messageBuilder.BuildMessageWithBackButton(validationResult.Message, new List<MessageButton>(), prevAction.ParentId, account.Language),
                            functionId: currentAction.FunctionId);
                        return;
                    }
                }
            }

            if (currentAction.IsEndAction)
            {
                var compiledCommand = await actionService.CompileCommand(userActions, userResponse, account.System);
                await HandleCommand(account, compiledCommand, currentAction.FunctionId);
            }
            else
            {
                await SendNotRecognizedCommandMessage(account);
            }
        }

        public async Task HandleUserReplyByButton(BotEntity bot, long chatId, int messageId, string callbackId, CallbackData callbackData)
        {
            var account = await settingService.GetCustBotAccount(chatId, bot.BotId);
            if (IsAccountInvalid(account))
            {
                return;
            }

            Messenger messenger = new(bot.BotId, chatId);
            if (callbackData.IsStopUsingChatbot)
            {
                await Task.WhenAll(
                    messageLogger.LogMessage(account.UserId, CommonId.BotUserId, messenger, messageBuilder.BuildStopUsingChatbotMessage(account.Language), MessageSendStatus.Success),
                    settingService.Unlink(account.CustId, account.SubAccId, string.Empty));
                return;
            }

            if (callbackData.IsMainMenu)
            {
                await Task.WhenAll(
                    messageLogger.LogMessage(account.UserId, CommonId.BotUserId, messenger, messageBuilder.BuildMainMenuButtonText(account.Language), MessageSendStatus.Success),
                    SendMainMenuMessage(account));
                return;
            }

            if (callbackData.Id == ActionIds.MainMenu)
            {
                await Task.WhenAll(
                    messageLogger.LogMessage(account.UserId, CommonId.BotUserId, messenger, messageBuilder.BuildBackToMainMenuButtonText(account.Language), MessageSendStatus.Success),
                    SendMainMenuMessage(account, messageId));
                return;
            }

            var currentAction = await actionService.GetActionById(callbackData.Id, account.System);
            if (!IsValidAction(account, currentAction))
            {
                await SendNotRecognizedCommandMessage(account, messageId);
                return;
            }

            try
            {
                var userResponse = currentAction.Type == ActionTypes.Downline ? SplitCommand(callbackData.Cmd).Item2?[0] : (await actionService.GetActionName(currentAction, account));
                await actionService.InsertUserAction(account.UserId, currentAction.ActionId, userResponse);

                if (currentAction.IsEndAction && !string.IsNullOrEmpty(callbackData.Cmd))
                {
                    var parentAction = await actionService.GetActionById(currentAction.ParentId, account.System);
                    var functionId = parentAction != null ? parentAction.FunctionId : currentAction.FunctionId;
                    await Task.WhenAll(
                        messageLogger.LogMessage(account.UserId, CommonId.BotUserId, messenger, userResponse, MessageSendStatus.Success, functionId: functionId),
                        functionId == FunctionIds.Settings
                            ? HandleSettingsCommand(bot, account, callbackData.Cmd, messageId, callbackId)
                            : HandleCommand(account, callbackData.Cmd, currentAction.FunctionId, messageId));
                }
                else
                {
                    await Task.WhenAll(
                        messageLogger.LogMessage(account.UserId, CommonId.BotUserId, messenger, userResponse, MessageSendStatus.Success, functionId: currentAction.FunctionId),
                        SendNextActionsMessage(account, currentAction, messageId));
                }
            }
            catch (MinimizeUMException ex)
            {
                await SendMinimizeUMMessage(account, ex, messageId);
            }
        }

        private async Task SendNextActionsMessage(CustBotAccount account, BotAction currentAction, int messageId = 0)
            => await messageSender.SendMessage(
                account: account,
                message: await actionService.BuildNextActionsMessage(account, currentAction),
                functionId: currentAction.FunctionId,
                messageId: messageId);

        private static (string, string[]) SplitCommand(string command)
        {
            var firstSpaceIndex = command.IndexOf(" ", StringComparison.OrdinalIgnoreCase);

            if (firstSpaceIndex == -1)
            {
                return (command, new string[0]);
            }

            var commandName = command.Substring(0, firstSpaceIndex);
            var commandParam = command.Substring(firstSpaceIndex + 1).Split(" ");

            return (commandName, commandParam);
        }

        private async Task HandleCommand(CustBotAccount account, string command, short functionId, int messageId = 0)
        {
            var splittedCommand = SplitCommand(command);
            var rootAction = await actionService.GetRootActionByFunctionId(account, functionId);
            var result = await functionHandlerFactory.GetHandler(functionId).Handle(account, splittedCommand.Item2);
            var message = messageBuilder.BuildMessageWithBackButton(result.Message, new List<MessageButton>(), rootAction.IsEndAction ? (short)0 : rootAction.ActionId, account.Language, true);
            await Task.WhenAll(
                actionService.ResetUserActions(account.UserId),
                messageSender.SendMessage(
                    account: account,
                    message: message,
                    functionId: functionId,
                    messageId: messageId));
        }

        private async Task HandleSettingsCommand(BotEntity bot, CustBotAccount account, string compiledCommand, int messageId, string callbackId)
        {
            var splittedCommand = SplitCommand(compiledCommand);
            var parameter = splittedCommand.Item2?[0];
            if (parameter == "stop")
            {
                await HandleStopCommand(account, bot.DisplayName, messageId);
                return;
            }

            if (short.TryParse(parameter, out short settingCommandId))
            {
                var function = account.Functions.FirstOrDefault(f => f.FunctionId == settingCommandId);
                if (function == null)
                {
                    return;
                }

                await UpdateNotificationFunctionStatus(function, account, messageId, callbackId);
            }

            if (LanguageCodes.SupportLanguages.Contains(parameter))
            {
                await UpdateLanguage(parameter, account, messageId, callbackId);
            }
        }

        private async Task UpdateNotificationFunctionStatus(BotFunction function, CustBotAccount account, int messageId, string callbackId)
        {
            function.Enabled = !function.Enabled;
            var action = await actionService.GetActionById(ActionIds.Notifications, account.System);
            await Task.WhenAll(
               settingService.UpdateUserFunction(account.UserId, account.BotId, function, account.CustId, string.Empty),
               InsertSettingUserAction(account, action),
               SendNextActionsMessage(account, action, messageId),
               messageSender.Alert(account.BotId, messageBuilder.BuildUpdateNotificationSettingMessage(account.Language, function.Enabled), callbackId));
        }

        private async Task UpdateLanguage(string language, CustBotAccount account, int messageId, string callbackId)
        {
            if (!string.Equals(account.Language, language))
            {
                await settingService.UpdateUserLanguage(account.UserId, account.BotId, language, account.CustId, string.Empty);
            }

            account.Language = language;
            var action = await actionService.GetActionById(ActionIds.Language, account.System);
            await Task.WhenAll(
                InsertSettingUserAction(account, action),
                SendNextActionsMessage(account, action, messageId),
                messageSender.Alert(account.BotId, messageBuilder.BuildLanguageUpdatedMessage(account.Language), callbackId));
        }

        private async Task InsertSettingUserAction(CustBotAccount account, BotAction action)
        {
            await actionService.InsertUserAction(account.UserId, ActionIds.Settings, string.Empty);
            await actionService.InsertUserAction(account.UserId, action.ActionId, string.Empty);
        }

        private async Task HandleStopCommand(CustBotAccount account, string botName, int messageId = 0)
            => await messageSender.SendMessage(
                account: account,
                message: messageBuilder.BuildStopUsingChatbotConfirmMessage(account.Language, botName),
                messageId: messageId);

        private static bool IsValidAction(CustBotAccount account, BotAction action)
            => action == null || account.Functions.Any(f => f.FunctionId == action.FunctionId);

        public async Task SendMainMenuMessage(CustBotAccount account, int messageId = 0)
            => await Task.WhenAll(
                actionService.ResetUserActions(account.UserId),
                SendMessageWithRootActions(account, messageBuilder.BuildMenuMessage(account.Language), messageId));

        private async Task SendNotRecognizedCommandMessage(CustBotAccount account, int messageId = 0)
            => await SendMessageWithRootActions(account, messageBuilder.BuildNotRecognizedCommandMessage(account.Language), messageId);

        private async Task SendMessageWithRootActions(CustBotAccount account, string text, int messageId)
            => await messageSender.SendMessage(
                account,
                messageBuilder.BuildMessage(text, await actionService.BuildRootActionButtons(account), account.Language),
                messageId);

        private async Task SendMinimizeUMMessage(CustBotAccount account, MinimizeUMException ex, int messageId = 0)
            => await SendMessageWithRootActions(account, messageBuilder.BuildMinimizeUMMessage(account.Language, ex.StartTime, ex.EndTime), messageId);
    }
}