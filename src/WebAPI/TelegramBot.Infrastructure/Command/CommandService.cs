using Alpha.API.Client.Services;
using Fanex.Caching;
using Newtonsoft.Json;
using System;
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
using TelegramBot.Infrastructure.Setting;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Caching;

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
        private readonly ISettingService settingService;
        private readonly IBotService botService;
        private readonly ICustomerService customerService;
        private readonly IActionService actionService;
        private readonly IFunctionService functionService;
        private readonly IFunctionHandlerFactory functionHandlerFactory;
        private readonly ICacheService cacheService;

        public CommandService(
            IMessageBuilder messageBuilder,
            IMessageSender messageSender,
            ISettingService settingService,
            IBotService botService,
            ICustomerService customerService,
            IActionService actionService,
            ICacheServiceFactory cacheServiceFactory,
            IFunctionService functionService,
            IFunctionHandlerFactory functionHandlerFactory)
        {
            this.messageBuilder = messageBuilder;
            this.messageSender = messageSender;
            this.settingService = settingService;
            this.botService = botService;
            this.customerService = customerService;
            this.actionService = actionService;
            this.functionService = functionService;
            this.functionHandlerFactory = functionHandlerFactory;
            this.cacheService = cacheServiceFactory.Get(CacheProvider.Redis);
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
                var encodedMessage = WebUtility.HtmlEncode(
                string.Join(
                    " ",
                    message.Split(
                        default(string[]),
                        StringSplitOptions.RemoveEmptyEntries)));
                await HandleUserReplyByText(account, messenger, encodedMessage);
            }
        }

        private async Task HandleUserReplyByText(CustBotAccount account, CustBotMessenger messenger, string userResponse)
        {
            var userActions = await actionService.GetUserActions(account.UserId);
            var lastUserAction = userActions.LastOrDefault();
            if (lastUserAction == null)
            {
                await SendMainMenuMessage(messenger);
                return;
            }

            var currentAction = await actionService.GetActionById(lastUserAction.ActionId);
            if (currentAction == null || !IsValidAction(messenger, currentAction))
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
                await actionService.SendNextActionsMessage(account, messenger, currentAction);
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

            if (!string.IsNullOrEmpty(callBackData.CompiledCommand))
            {
                await HandleCommand(account, messenger, callBackData.CompiledCommand, currentAction.FunctionId);
            }
            else
            {
                await actionService.InsertUserAction(account.UserId, currentAction.ActionId, string.Empty);
                await actionService.SendNextActionsMessage(account, messenger, currentAction);
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
            await actionService.ResetUserActions(account.UserId);
            var splittedCommand = SplitCommand(command);
            await functionHandlerFactory.GetHandler(functionId).Handle(account, messenger, splittedCommand.Item1, splittedCommand.Item2);
        }

        private static bool IsValidAction(CustBotMessenger messenger, BotAction action)
            => messenger.Functions.Any(f => f.FunctionId == action.FunctionId);

        private async Task SendMainMenuMessage(CustBotMessenger messenger)
        {
            var message = messageBuilder.BuildMenuMessage(messenger.Language);
            await actionService.SendRootActionsMessage(messenger, message);
        }

        private async Task SendNotRecognizedCommandMessage(CustBotMessenger messenger)
        {
            var message = messageBuilder.BuildNotRecognizedCommandMessage(messenger.Language);
            await actionService.SendRootActionsMessage(messenger, message);
        }
    }
}