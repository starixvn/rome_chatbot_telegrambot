using Alpha.API.Client.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TelegramBot.Infrastructure.Action;
using TelegramBot.Infrastructure.Action.Constants;
using TelegramBot.Infrastructure.Action.Models;
using TelegramBot.Infrastructure.Action.Validators;
using TelegramBot.Infrastructure.Bot;
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
        Task ExecuteAsync(long messengerBotId, long chatId, string message, bool isCallback, int messageId);
        Task SendMainMenuMessage(CustBotAccount account, Messenger messenger, int messageId = 0);
    }

    public class CommandService : ICommandService
    {
        private readonly IMessageBuilder messageBuilder;
        private readonly IMessageSender messageSender;
        private readonly IMessageLogger messageLogger;
        private readonly ISettingService settingService;
        private readonly IBotService botService;
        private readonly ICustomerService customerService;
        private readonly IActionService actionService;
        private readonly IFunctionService functionService;
        private readonly IFunctionHandlerFactory functionHandlerFactory;
        private readonly IActionValidatorFactory actionValidatorFactory;

        public CommandService(
            IMessageBuilder messageBuilder,
            IMessageSender messageSender,
            IMessageLogger messageLogger,
            ISettingService settingService,
            IBotService botService,
            ICustomerService customerService,
            IActionService actionService,
            IFunctionService functionService,
            IFunctionHandlerFactory functionHandlerFactory,
            IActionValidatorFactory actionValidatorFactory)
        {
            this.messageBuilder = messageBuilder;
            this.messageSender = messageSender;
            this.messageLogger = messageLogger;
            this.settingService = settingService;
            this.botService = botService;
            this.customerService = customerService;
            this.actionService = actionService;
            this.functionService = functionService;
            this.functionHandlerFactory = functionHandlerFactory;
            this.actionValidatorFactory = actionValidatorFactory;
        }

        public async Task ExecuteAsync(long messengerBotId, long chatId, string message, bool isCallback, int messageId)
        {
            if (messengerBotId == 0 || chatId == 0 || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var bot = await botService.GetAsync(messengerBotId);
            var account = await settingService.GetCustBotAccount(chatId, bot.BotId);
            var messenger = new Messenger(bot.BotId, chatId, bot.Name);

            if (isCallback)
            {
                await HandleUserReplyByButton(account, messenger, message, messageId);
            }
            else
            {
                await HandleUserReplyByText(account, messenger, message, messageId);
            }
        }

        private async Task HandleHelpCommand(CustBotAccount account, Messenger messenger, bool hasReportPermission)
        {
            var message = messageBuilder.BuildHelpMessage(
                account.Language,
                string.Empty,
                hasReportPermission);
            await messageSender.SendMessage(messenger, message);
        }

        private async Task HandleUserReplyByText(CustBotAccount account, Messenger messenger, string message, int messageId)
        {
            var userResponse = WebUtility.HtmlEncode(
                string.Join(
                    " ",
                    message.Split(
                        default(string[]),
                        StringSplitOptions.RemoveEmptyEntries)));

            await Task.Run(() => messageLogger.LogMessage(account.UserId, CommonId.BotUserId, messenger, message, MessageSendStatus.Success));

            if (userResponse.Split(' ')[0] == "/help")
            {
                await HandleHelpCommand(account, messenger, !account.IsSubAccount || (await customerService.GetCustomerStatus(account.CustId, account.SubAccId)).HasReportPermission);
                return;
            }

            if (userResponse.Split(' ')[0] == "/menu")
            {
                await SendMainMenuMessage(account, messenger, messageId);
                return;
            }

            if (userResponse.Split(' ')[0] == "/stop")
            {
                await HandleStopCommand(account, messenger, messageId);
                return;
            }

            var userActions = await actionService.GetUserActions(account.UserId);
            var lastUserAction = userActions.LastOrDefault();
            var currentAction = lastUserAction != null ? await actionService.GetActionById(lastUserAction.ActionId) : null;
            if (currentAction == null || currentAction.Type != ActionTypes.Message)
            {
                var function = await functionService.GetFunctionByCommand(userResponse);
                if (function == null)
                {
                    await SendNotRecognizedCommandMessage(account, messenger, messageId);
                    return;
                }

                var action = await actionService.GetActionByCommand(userResponse);
                if (action != null)
                {
                    await SendNextActionsMessage(account, messenger, action);
                    return;
                }

                await HandleCommand(account, messenger, userResponse, function.FunctionId, messageId);
                return;
            }

            if (currentAction.Type == ActionTypes.Message)
            {
                await actionService.UpdateUserAction(lastUserAction.Id, userResponse);
                var validator = actionValidatorFactory.GetValidator(currentAction.ActionId);
                if (validator != null)
                {
                    var validationResult = await validator.Validate(account, userResponse);
                    if (validationResult.IsNotOk)
                    {
                        var prevAction = await actionService.GetActionById(currentAction.ParentId);
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
                var compiledCommand = await actionService.CompileCommand(userActions, userResponse);
                await HandleCommand(account, messenger, compiledCommand, currentAction.FunctionId, messageId);
            }
            else
            {
                await SendNextActionsMessage(account, messenger, currentAction);
            }
        }

        private async Task HandleUserReplyByButton(CustBotAccount account, Messenger messenger, string message, int messageId)
        {
            var callBackData = JsonConvert.DeserializeObject<CallBackData>(message);
            if (callBackData == null)
            {
                throw new InvalidOperationException($"Wrong call back data format {message}");
            }

            await Task.Run(() => messageLogger.LogMessage(account.UserId, CommonId.BotUserId, messenger, callBackData.Value, MessageSendStatus.Success));

            if (callBackData.IsStopUsingChatbot)
            {
                await settingService.Unlink(messenger.BotId, account.CustId, account.SubAccId, string.Empty);
                return;
            }

            if (callBackData.ActionId == ActionIds.MainMenu)
            {
                await SendMainMenuMessage(account, messenger, messageId);
                return;
            }

            var currentAction = await actionService.GetActionById(callBackData.ActionId);

            if (currentAction == null || !IsValidAction(account, currentAction))
            {
                await SendNotRecognizedCommandMessage(account, messenger, messageId);
                return;
            }

            var userResponse = currentAction.Type == ActionTypes.Downline ? callBackData.Value : string.Empty;
            await actionService.InsertUserAction(account.UserId, currentAction.ActionId, userResponse);

            if (currentAction.IsEndAction)
            {
                var userActions = await actionService.GetUserActions(account.UserId);
                var compiledCommand = await actionService.CompileCommand(userActions, userResponse);
                await HandleCommand(account, messenger, compiledCommand, currentAction.FunctionId, messageId);
            }
            else
            {
                await SendNextActionsMessage(account, messenger, currentAction, messageId);
            }
        }

        private async Task SendNextActionsMessage(CustBotAccount account, Messenger messenger, BotAction currentAction, int messageId = 0)
        {
            var message = await actionService.BuildNextActionsMessage(account, currentAction);
            await messageSender.SendMessage(account.UserId, messenger, message, functionId: currentAction.FunctionId, messageId: messageId);
        }

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

        private async Task HandleCommand(CustBotAccount account, Messenger messenger, string command, short functionId, int messageId)
        {
            var splittedCommand = SplitCommand(command);
            var rootAction = await actionService.GetRootActionByFunctionId(functionId);
            if (functionId == FunctionIds.Settings)
            {
                await HandleSettingsCommand(account, messenger, rootAction, splittedCommand.Item2?[0], messageId);
            }
            else
            {
                var result = await functionHandlerFactory.GetHandler(functionId).Handle(account, splittedCommand.Item2);
                var message = await messageBuilder.BuildMessageWithBackButton(result.Message, new List<MessageButton>(), rootAction.ActionId, account.Language);
                await messageSender.SendMessage(
                    account.UserId,
                    messenger: messenger,
                    message: message,
                    functionId: functionId,
                    messageId: messageId);
            }
        }

        private async Task HandleSettingsCommand(CustBotAccount account, Messenger messenger, BotAction rootAction, string parameter, int messageId)
        {
            if (parameter == "stop")
            {
                await HandleStopCommand(account, messenger, messageId);
                return;
            }

            if (short.TryParse(parameter, out short id))
            {
                if (account.IsFunctionAvailable(id))
                {
                    var function = account.Functions.FirstOrDefault(f => f.FunctionId == id);
                    function.Enabled = !function.Enabled;
                    var result = await settingService.UpdateUserFunction(account.UserId, function, account.CustId, string.Empty);
                    await SendNextActionsMessage(account, messenger, rootAction, messageId);
                }
            }
        }

        private async Task HandleStopCommand(CustBotAccount account, Messenger messenger, int messageId = 0)
        {
            var message = await messageBuilder.BuildStopUsingChatbotConfirmMessage(account.Language, messenger.BotName);
            await messageSender.SendMessage(
                userId: account.UserId,
                messenger: messenger,
                message: message,
                messageId: messageId);
        }

        private static bool IsValidAction(CustBotAccount account, BotAction action)
            => account.Functions.Any(f => f.FunctionId == action.FunctionId);

        public async Task SendMainMenuMessage(CustBotAccount account, Messenger messenger, int messageId = 0)
        {
            var text = messageBuilder.BuildMenuMessage(account.Language);
            await SendMessageWithRootActions(account, messenger, text, messageId);
        }

        private async Task SendNotRecognizedCommandMessage(CustBotAccount account, Messenger messenger, int messageId)
        {
            var text = messageBuilder.BuildNotRecognizedCommandMessage(account.Language);
            await SendMessageWithRootActions(account, messenger, text, messageId);
        }

        private async Task SendMessageWithRootActions(CustBotAccount account, Messenger messenger, string text, int messageId)
        {
            await actionService.ResetUserActions(account.UserId);
            var rootActionButtons = await actionService.BuildRootActionButtons(account);
            var message = await messageBuilder.BuildMessage(text, rootActionButtons, account.Language);
            await messageSender.SendMessage(account.UserId, messenger, message, messageId);
        }
    }
}