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
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Command.Models;
using TelegramBot.Infrastructure.Function.Handlers;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Message.Models;
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Setting.Models;

namespace TelegramBot.Infrastructure.Command
{
    public interface ICommandService
    {
        Task ExecuteAsync(long messengerBotId, long chatId, string message, bool isCallback);
    }

    public class CommandService : ICommandService
    {
        private readonly IMessageBuilder messageBuilder;
        private readonly IMessageSender messageSender;
        private readonly IMessageFormatter messageFormatter;
        private readonly ISettingService settingService;
        private readonly IBotService botService;
        private readonly ICustomerService customerService;
        private readonly IActionService actionService;
        private readonly IFunctionService functionService;
        private readonly IFunctionHandlerFactory functionHandlerFactory;

        public CommandService(
            IMessageBuilder messageBuilder,
            IMessageSender messageSender,
            IMessageFormatter messageFormatter,
            ISettingService settingService,
            IBotService botService,
            ICustomerService customerService,
            IActionService actionService,
            IFunctionService functionService,
            IFunctionHandlerFactory functionHandlerFactory)
        {
            this.messageBuilder = messageBuilder;
            this.messageSender = messageSender;
            this.messageFormatter = messageFormatter;
            this.settingService = settingService;
            this.botService = botService;
            this.customerService = customerService;
            this.actionService = actionService;
            this.functionService = functionService;
            this.functionHandlerFactory = functionHandlerFactory;
        }

        public async Task ExecuteAsync(long messengerBotId, long chatId, string message, bool isCallback)
        {
            if (messengerBotId == 0 || chatId == 0 || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var bot = await botService.GetAsync(messengerBotId);
            var account = await settingService.GetCustBotAccount(chatId, bot.BotId);
            var messenger = await settingService.GetCustBotMessenger(account.CustId, account.SubAccId);

            var action = message.Split(' ')[0] switch
            {
                "/help" => HandleHelpCommand(messenger, !account.IsSubAccount || (await customerService.GetCustomerStatus(account.CustId, account.SubAccId)).HasReportPermission),
                "/menu" => SendMainMenuMessage(messenger),
                _ => HandleUserReply(account, messenger, message, isCallback),
            };

            await action;
        }

        private async Task HandleHelpCommand(CustBotMessenger messenger, bool hasReportPermission)
        {
            var message = messageBuilder.BuildHelpMessage(
                messenger.Language,
                messenger.BotName,
                hasReportPermission);
            await messageSender.SendMessage(messenger, message);
        }

        private async Task HandleUserReply(CustBotAccount account, CustBotMessenger messenger, string message, bool isCallback)
        {
            if (isCallback)
            {
                await HandleUserReplyByButton(account, messenger, message);
            }
            else
            {
                await HandleUserReplyByText(account, messenger, message);
            }
        }

        private async Task HandleUserReplyByText(CustBotAccount account, CustBotMessenger messenger, string message)
        {
            var userResponse = WebUtility.HtmlEncode(
                string.Join(
                    " ",
                    message.Split(
                        default(string[]),
                        StringSplitOptions.RemoveEmptyEntries)));
            var userActions = await actionService.GetUserActions(account.UserId);
            var lastUserAction = userActions.LastOrDefault();
            var currentAction = lastUserAction != null ? await actionService.GetActionById(lastUserAction.ActionId) : null;
            if (currentAction == null || currentAction.Type != ActionTypes.Message)
            {
                var function = await functionService.GetFunctionByCommand(userResponse);
                if (function == null)
                {
                    await SendNotRecognizedCommandMessage(messenger);
                    return;
                }

                await HandleCommand(account, messenger, userResponse, function.FunctionId);
                return;
            }

            if (currentAction.Type == ActionTypes.Message)
            {
                await actionService.UpdateUserAction(lastUserAction.Id, userResponse);
            }

            if (currentAction.IsEndAction)
            {
                var compiledCommand = await actionService.CompileCommand(userActions, userResponse);
                await HandleCommand(account, messenger, compiledCommand, currentAction.FunctionId);
            }
            else
            {
                await SendNextActionsMessage(account, messenger, currentAction);
            }
        }

        private async Task HandleUserReplyByButton(CustBotAccount account, CustBotMessenger messenger, string message)
        {
            var callBackData = JsonConvert.DeserializeObject<CallBackData>(message);
            if (callBackData == null)
            {
                throw new InvalidOperationException($"Wrong call back data format {message}");
            }

            if (callBackData.IsBackToMainMenu)
            {
                await SendMainMenuMessage(messenger);
                return;
            }

            if (callBackData.IsCancel)
            {
                await actionService.ResetUserActions(account.UserId);
            }

            var currentAction = await actionService.GetActionById(callBackData.ActionId);
            if (currentAction == null || !IsValidAction(messenger, currentAction))
            {
                await SendNotRecognizedCommandMessage(messenger);
                return;
            }

            await actionService.InsertUserAction(account.UserId, currentAction.ActionId, string.Empty, callBackData.CompiledCommand ?? string.Empty);

            if (!string.IsNullOrEmpty(callBackData.CompiledCommand))
            {
                await HandleCommand(account, messenger, callBackData.CompiledCommand, currentAction.FunctionId);
            }
            else
            {
                await SendNextActionsMessage(account, messenger, currentAction);
            }
        }

        private async Task SendNextActionsMessage(CustBotAccount account, CustBotMessenger messenger, BotAction currentAction)
        {
            var language = account.Language;
            var nextActions = await actionService.GetChildActions(currentAction.ActionId);
            var actionSelectedMessage = messageBuilder.BuildActionSelectedMessage(language, await messageBuilder.BuildActionName(language, currentAction.ActionId));

            if (nextActions.Count() == 1 && actionService.IsMessageAction(nextActions.FirstOrDefault()))
            {
                var messageAction = nextActions.FirstOrDefault();
                await actionService.InsertUserAction(account.UserId, messageAction.ActionId, string.Empty);
                var message = new Message.Models.Message(
                    string.Join(
                        messageFormatter.NewLine(),
                        actionSelectedMessage,
                        await messageBuilder.BuildActionName(language, messageAction.ActionId)),
                    new List<MessageButton> { messageBuilder.BuildCancelButton(language, messageAction.BackToActionId) });
                await messageSender.SendMessage(messenger, message, messageAction.FunctionId);
            }
            else
            {
                var message = new Message.Models.Message(actionSelectedMessage, await actionService.BuildActionButtons(account, nextActions));
                await messageSender.SendMessageWithBackToMainMenuButton(messenger, message, currentAction.FunctionId);
            }
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

        private async Task HandleCommand(CustBotAccount account, CustBotMessenger messenger, string command, short functionId)
        {

            var splittedCommand = SplitCommand(command);
            var result = await functionHandlerFactory.GetHandler(functionId).Handle(account, splittedCommand.Item2);
            var rootAction = await actionService.GetRootActionByFunctionId(functionId);
            if (result.IsOk)
            {
                await actionService.ResetUserActions(account.UserId);
                await actionService.InsertUserAction(account.UserId, rootAction.ActionId, string.Empty);
                var buttons = await actionService.BuildNextActionButtons(account, rootAction.ActionId);
                await messageSender.SendMessageWithBackToMainMenuButton(messenger, new Message.Models.Message(result.Message, buttons), functionId);
            }
            else
            {
                var message = new Message.Models.Message(result.Message, new List<MessageButton> { messageBuilder.BuildCancelButton(account.Language, rootAction.ActionId) });
                await messageSender.SendMessage(messenger, message, functionId);
            }
        }

        private static bool IsValidAction(CustBotMessenger messenger, BotAction action)
            => messenger.Functions.Any(f => f.FunctionId == action.FunctionId);

        private async Task SendMainMenuMessage(CustBotMessenger messenger)
        {
            var text = messageBuilder.BuildMenuMessage(messenger.Language);
            await SendMessageWithRootActions(messenger, text);
        }

        private async Task SendNotRecognizedCommandMessage(CustBotMessenger messenger)
        {
            var text = messageBuilder.BuildNotRecognizedCommandMessage(messenger.Language);
            await SendMessageWithRootActions(messenger, text);
        }

        private async Task SendMessageWithRootActions(CustBotMessenger messenger, string text)
        {
            await actionService.ResetUserActions(messenger.UserId);
            var rootActionButtons = await actionService.BuildRootActionButtons(messenger.Language);
            var message = new Message.Models.Message(text, rootActionButtons);
            await messageSender.SendMessage(messenger, message);
        }
    }
}