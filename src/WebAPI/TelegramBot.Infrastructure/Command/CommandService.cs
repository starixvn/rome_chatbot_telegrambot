using System.Net;
using System.Text;
using Fanex.Logging;
using TelegramBot.Infrastructure.Action;
using TelegramBot.Infrastructure.Action.Constants;
using TelegramBot.Infrastructure.Action.Models;
using TelegramBot.Infrastructure.Action.Validators;
using TelegramBot.Infrastructure.Bot.Models;
using TelegramBot.Infrastructure.Command.Models;
using TelegramBot.Infrastructure.Function.Constants;
using TelegramBot.Infrastructure.Function.Handlers;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Message.Constants;
using TelegramBot.Infrastructure.Message.Models;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Constants;
using TelegramBot.Infrastructure.Shared.Models;

namespace TelegramBot.Infrastructure.Command
{
    public interface ICommandService
    {
        Task HandleUserReplyByText(BotEntity bot, long chatId, string message);

        Task HandleUserReplyByButton(BotEntity bot, long chatId, int messageId, string callbackId, CallbackData callbackData);

        Task SendMainMenuMessage(CustBotAccount account, Messenger messenger, int messageId = 0);
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
        private readonly ILogger logger;

        public CommandService(
            IMessageBuilder messageBuilder,
            IMessageSender messageSender,
            IMessageLogger messageLogger,
            ISettingService settingService,
            IActionService actionService,
            IFunctionService functionService,
            IFunctionHandlerFactory functionHandlerFactory,
            IActionValidatorFactory actionValidatorFactory,
            ILogger logger)
        {
            this.messageBuilder = messageBuilder;
            this.messageSender = messageSender;
            this.messageLogger = messageLogger;
            this.settingService = settingService;
            this.actionService = actionService;
            this.functionService = functionService;
            this.functionHandlerFactory = functionHandlerFactory;
            this.actionValidatorFactory = actionValidatorFactory;
            this.logger = logger;
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
            if (IsAccountInvalid(account, chatId))
            {
                return;
            }

            Messenger messenger = new(bot.BotId, chatId, bot.System);

            if (IsCommand(userResponse))
            {
                var function = GetCommandFunction(account, userResponse);
                var handle = userResponse.Split(' ')[0] switch
                {
                    "/help" => HandleHelpCommand(account, messenger, bot.Name),
                    "/menu" => SendMainMenuMessage(account, messenger),
                    "/stop" => HandleStopCommand(account, messenger, bot.Name),
                    "/unlink" => HandleStopCommand(account, messenger, bot.Name),
                    "/settings" => HandleSettingsCommand(account, messenger),
                    "/setting" => HandleSettingsCommand(account, messenger),
                    _ => HandleOtherCommand(account, messenger, userResponse)
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

        private async Task HandleOtherCommand(CustBotAccount account, Messenger messenger, string command)
        {
            var function = GetCommandFunction(account, command);

            if (function == null)
            {
                await SendNotRecognizedCommandMessage(account, messenger);
                return;
            }

            var action = await actionService.GetActionByCommand(command, messenger.System);
            if (action != null)
            {
                await Task.WhenAll(
                    actionService.InsertUserAction(account.UserId, action.ActionId, string.Empty),
                    SendNextActionsMessage(account, messenger, action));
                return;
            }

            await HandleCommand(account, messenger, command, function.FunctionId);
        }

        private async Task HandleSettingsCommand(CustBotAccount account, Messenger messenger)
        {
            var settingsAction = await actionService.GetActionById(ActionIds.Settings, messenger.System);
            await Task.WhenAll(
                actionService.InsertUserAction(account.UserId, settingsAction.ActionId, string.Empty),
                SendNextActionsMessage(account, messenger, settingsAction));
        }

        private async Task HandleHelpCommand(CustBotAccount account, Messenger messenger, string botName)
        {
            var message = messageBuilder.BuildGeneralCommandsHelpMessage(account.Language, botName);
            var builder = new StringBuilder(message);
            var formatter = messageBuilder.GetFormatter();
            foreach (var report in account.Functions.Where(f => f.Type == FunctionTypes.Report))
            {
                builder.Append(formatter.NewLine());
                builder.Append($"{formatter.NewLine()}{formatter.Bold(await functionService.GetFunctionName(report.FunctionId, account.Language))}");
                var rootAction = await actionService.GetRootActionByFunctionId(account, report.FunctionId, messenger.System);
                builder.Append($"{formatter.NewLine()}{rootAction.Command} - {await actionService.GetActionDescription(rootAction.ActionId, account.Language)}");
                var childActions = (await actionService.GetChildActions(account, rootAction.ActionId, messenger.System)).Where(a => a.Type != ActionTypes.Downline);
                foreach (var childAction in childActions)
                {
                    builder.Append($"{formatter.NewLine()}{rootAction.Command} {childAction.Command} - {await actionService.GetActionDescription(childAction.ActionId, account.Language)}");
                }
            }

            await messageSender.SendMessage(messenger, builder.ToString());
        }

        private static bool IsCommand(string userResponse)
            => userResponse.StartsWith("/");

        private bool IsAccountInvalid(CustBotAccount account, long chatId)
        {
            if (account == null)
            {
                logger.Info($"ChatId: {chatId} doesn't exist");
                return true;
            }

            return false;
        }

        private static Function.Models.BotFunction GetCommandFunction(CustBotAccount account, string command)
            => account.Functions.FirstOrDefault(f => !string.IsNullOrEmpty(f.Command) && command.IndexOf(f.Command, StringComparison.OrdinalIgnoreCase) == 0);

        private async Task HandleOtherResponse(CustBotAccount account, Messenger messenger, string userResponse)
        {
            var userActions = await actionService.GetUserActions(account.UserId);
            var lastUserAction = userActions.LastOrDefault();
            var currentAction = lastUserAction != null ? await actionService.GetActionById(lastUserAction.ActionId, messenger.System) : null;
            if (lastUserAction == null || currentAction == null)
            {
                await Task.WhenAll(
                    messageLogger.LogMessage(account.UserId, CommonId.BotUserId, messenger, userResponse, MessageSendStatus.Success),
                    SendNotRecognizedCommandMessage(account, messenger));
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
                        var prevAction = await actionService.GetActionById(currentAction.ParentId, messenger.System);
                        await messageSender.SendMessage(
                            account.UserId,
                            messenger: messenger,
                            message: await messageBuilder.BuildMessageWithBackButton(validationResult.Message, new List<MessageButton>(), prevAction.ParentId, account.Language),
                            functionId: currentAction.FunctionId);
                        return;
                    }
                }
            }

            if (currentAction.IsEndAction)
            {
                var compiledCommand = await actionService.CompileCommand(userActions, userResponse, messenger.System);
                await HandleCommand(account, messenger, compiledCommand, currentAction.FunctionId);
            }
            else
            {
                await SendNotRecognizedCommandMessage(account, messenger);
            }
        }

        public async Task HandleUserReplyByButton(BotEntity bot, long chatId, int messageId, string callbackId, CallbackData callbackData)
        {
            var account = await settingService.GetCustBotAccount(chatId, bot.BotId);
            if (IsAccountInvalid(account, chatId))
            {
                return;
            }

            Messenger messenger = new(bot.BotId, chatId, bot.System);
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
                    SendMainMenuMessage(account, messenger));
                return;
            }

            if (callbackData.Id == ActionIds.MainMenu)
            {
                await Task.WhenAll(
                    messageLogger.LogMessage(account.UserId, CommonId.BotUserId, messenger, messageBuilder.BuildBackToMainMenuButtonText(account.Language), MessageSendStatus.Success),
                    SendMainMenuMessage(account, messenger, messageId));
                return;
            }

            var currentAction = await actionService.GetActionById(callbackData.Id, messenger.System);
            if (!IsValidAction(account, currentAction))
            {
                await SendNotRecognizedCommandMessage(account, messenger, messageId);
                return;
            }

            var userResponse = currentAction.Type == ActionTypes.Downline ? SplitCommand(callbackData.Cmd).Item2?[0] : (await actionService.GetActionName(currentAction, account));
            await actionService.InsertUserAction(account.UserId, currentAction.ActionId, userResponse);

            if (currentAction.IsEndAction && !string.IsNullOrEmpty(callbackData.Cmd))
            {
                var parentAction = await actionService.GetActionById(currentAction.ParentId, messenger.System);
                var functionId = parentAction != null ? parentAction.FunctionId : currentAction.FunctionId;
                await Task.WhenAll(
                    messageLogger.LogMessage(account.UserId, CommonId.BotUserId, messenger, userResponse, MessageSendStatus.Success, functionId: functionId),
                    functionId == FunctionIds.Settings
                        ? HandleSettingsCommand(bot, account, messenger, callbackData.Cmd, messageId, callbackId)
                        : HandleCommand(account, messenger, callbackData.Cmd, currentAction.FunctionId, messageId));
            }
            else
            {
                await Task.WhenAll(
                    messageLogger.LogMessage(account.UserId, CommonId.BotUserId, messenger, userResponse, MessageSendStatus.Success, functionId: currentAction.FunctionId),
                    SendNextActionsMessage(account, messenger, currentAction, messageId));
            }
        }

        private async Task SendNextActionsMessage(CustBotAccount account, Messenger messenger, BotAction currentAction, int messageId = 0)
            => await messageSender.SendMessage(
                account.UserId,
                messenger,
                await actionService.BuildNextActionsMessage(account, currentAction, messenger.System),
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

        private async Task HandleCommand(CustBotAccount account, Messenger messenger, string command, short functionId, int messageId = 0)
        {
            var splittedCommand = SplitCommand(command);
            var rootAction = await actionService.GetRootActionByFunctionId(account, functionId, messenger.System);
            var result = await functionHandlerFactory.GetHandler(functionId).Handle(account, splittedCommand.Item2);
            var message = await messageBuilder.BuildMessageWithBackButton(result.Message, new List<MessageButton>(), rootAction.ActionId, account.Language, true);
            await Task.WhenAll(
                actionService.ResetUserActions(account.UserId),
                messageSender.SendMessage(account.UserId, messenger: messenger, message: message, functionId: functionId, messageId: messageId));
        }

        private async Task HandleSettingsCommand(BotEntity bot, CustBotAccount account, Messenger messenger, string compiledCommand, int messageId, string callbackId)
        {
            var splittedCommand = SplitCommand(compiledCommand);
            var parameter = splittedCommand.Item2?[0];
            if (parameter == "stop")
            {
                await HandleStopCommand(account, messenger, bot.Name, messageId);
                return;
            }

            if (short.TryParse(parameter, out short settingCommandId))
            {
                var function = account.Functions.FirstOrDefault(f => f.FunctionId == settingCommandId);
                if (function == null)
                {
                    return;
                }

                function.Enabled = !function.Enabled;
                var action = await actionService.GetActionById(ActionIds.Notifications, messenger.System);
                await Task.WhenAll(
                    settingService.UpdateUserFunction(account.UserId, messenger.BotId, function, account.CustId, string.Empty),
                    InsertSettingUserAction(account, action),
                    SendNextActionsMessage(account, messenger, action, messageId),
                    messageSender.Alert(messenger, messageBuilder.BuildUpdateNotificationSettingMessage(account.Language, function.Enabled), callbackId));
            }

            if (LanguageCodes.SupportLanguages.Contains(parameter))
            {
                if (string.Equals(account.Language, parameter))
                {
                    return;
                }

                account.Language = parameter;
                var action = await actionService.GetActionById(ActionIds.Language, messenger.System);
                await Task.WhenAll(
                    settingService.UpdateUserLanguage(account.UserId, messenger.BotId, parameter, string.Empty),
                    InsertSettingUserAction(account, action),
                    SendNextActionsMessage(account, messenger, action, messageId),
                    messageSender.Alert(messenger, "Language Updated!", callbackId));
            }
        }

        private async Task InsertSettingUserAction(CustBotAccount account, BotAction action)
        {
            await actionService.InsertUserAction(account.UserId, ActionIds.Settings, string.Empty);
            await actionService.InsertUserAction(account.UserId, action.ActionId, string.Empty);
        }

        private async Task HandleStopCommand(CustBotAccount account, Messenger messenger, string botName, int messageId = 0)
            => await messageSender.SendMessage(
                userId: account.UserId,
                messenger: messenger,
                message: await messageBuilder.BuildStopUsingChatbotConfirmMessage(account.Language, botName),
                messageId: messageId);

        private static bool IsValidAction(CustBotAccount account, BotAction action)
            => action == null || account.Functions.Any(f => f.FunctionId == action.FunctionId);

        public async Task SendMainMenuMessage(CustBotAccount account, Messenger messenger, int messageId = 0)
            => await SendMessageWithRootActions(account, messenger, messageBuilder.BuildMenuMessage(account.Language), messageId);

        private async Task SendNotRecognizedCommandMessage(CustBotAccount account, Messenger messenger, int messageId = 0)
            => await SendMessageWithRootActions(account, messenger, messageBuilder.BuildNotRecognizedCommandMessage(account.Language), messageId);

        private async Task SendMessageWithRootActions(CustBotAccount account, Messenger messenger, string text, int messageId)
            => await Task.WhenAll(
                actionService.ResetUserActions(account.UserId),
                messageSender.SendMessage(
                    account.UserId,
                    messenger,
                    await messageBuilder.BuildMessage(text, await actionService.BuildRootActionButtons(account, messenger.System), account.Language),
                    messageId));
    }
}