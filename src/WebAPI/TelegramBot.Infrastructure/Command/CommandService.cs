using Alpha.API.Client.Services;
using Fanex.Caching;
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
        private readonly IBuilderService builderService;
        private readonly ISenderService senderService;
        private readonly ISettingService settingService;
        private readonly IBotService botService;
        private readonly ICustomerService customerService;
        private readonly IActionService actionService;
        private readonly ICacheService cacheService;
        private readonly IFunctionService functionService;
        private readonly IFunctionHandlerFactory functionHandlerFactory;

        public CommandService(
            IBuilderService builderService,
            ISenderService senderService,
            ISettingService settingService,
            IBotService botService,
            ICustomerService customerService,
            IActionService actionService,
            ICacheService cacheService,
            IFunctionService functionService,
            IFunctionHandlerFactory functionHandlerFactory)
        {
            this.builderService = builderService;
            this.senderService = senderService;
            this.settingService = settingService;
            this.botService = botService;
            this.customerService = customerService;
            this.actionService = actionService;
            this.cacheService = cacheService;
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

            var message = builderService.BuildHelpMessage(
                messenger.Language,
                messenger.BotName,
                hasReportPermission);
            await senderService.SendMessage(messenger, message);
        }

        private async Task HandleUserReply(CustBotAccount account, CustBotMessenger messenger, string message, bool isCallback)
        {
            var encodedMessage = WebUtility.HtmlEncode(
                string.Join(
                    " ",
                    message.Split(
                        default(string[]),
                        StringSplitOptions.RemoveEmptyEntries)));

            if (isCallback)
            {
                await HandleUserReplyByButton(account, messenger, encodedMessage);
            }
            else
            {
                await HandleUserReplyByText(account, messenger, encodedMessage);
            }
        }

        private async Task HandleUserReplyByText(CustBotAccount account, CustBotMessenger messenger, string message)
        {


            var lastAction = actionService.GetActionById(await cacheService.GetAsync<int>($"{messenger.UserId}.ActionID"));
            if (lastAction == null || !IsValidAction(messenger, lastAction))
            {

                var function = await functionService.GetFunctionByCommand(message);
                if (function == null)
                {
                    await SendNotRecognizedCommandMessage(messenger);
                    return;
                }

                await HandleCommand(account, messenger, message, function.FunctionId);
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
                await cacheService.RemoveAsync($"{messenger.UserId}");
            }

            var currentAction = actionService.GetActionById(callBackData.ActionId);
            if (currentAction == null || !IsValidAction(messenger, currentAction))
            {
                await SendNotRecognizedCommandMessage(messenger);
                return;
            }

            var compiledCommand = await cacheService.GetAsync<string>($"{messenger.UserId}.command");
            await cacheService.SetAsync($"{messenger.UserId}.command", string.IsNullOrEmpty(compiledCommand) ? $"{compiledCommand} {currentAction.Command}" : currentAction.Command);
            await cacheService.SetAsync($"{messenger.UserId}.ActionId", currentAction.ActionId);

            if (currentAction.IsEndAction)
            {
                await HandleCommand(account, messenger, compiledCommand, currentAction.FunctionId);
            }
            else
            {
                await SendNextActionsMessage(messenger, currentAction);
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
            await functionHandlerFactory.GetHandler(functionId).Handle(account, messenger, splittedCommand.Item2);
        }

        private static bool IsValidAction(CustBotMessenger messenger, BotAction action)
            => messenger.Functions.Any(f => f.FunctionId == action.FunctionId);

        private static bool IsMessageAction(IEnumerable<BotAction> nextActions)
            => nextActions.Count() == 1 && nextActions.FirstOrDefault().Type == ActionTypes.Message;

        private async Task SendMainMenuMessage(CustBotMessenger messenger)
        {
            await senderService.SendMessage(
               messenger,
               builderService.BuildMenuMessage(messenger.Language),
               replyMarkup: builderService.BuildActionButtions(messenger.Language, actionService.GetRootActions()));
        }

        private async Task SendNotRecognizedCommandMessage(CustBotMessenger messenger)
            => await senderService.SendMessage(
                messenger,
                builderService.BuildNotRecognizedCommandMessage(messenger.Language),
                replyMarkup: builderService.BuildActionButtions(messenger.Language, actionService.GetRootActions()));

        private async Task SendNextActionsMessage(CustBotMessenger messenger, BotAction currentAction)
        {
            var nextActions = actionService.GetChildActions(currentAction.ActionId);
            if (IsMessageAction(nextActions))
            {
                await senderService.SendMessage(
                    messenger,
                    currentAction.ActionName,
                    replyMarkup: builderService.BuildCancelButtion(messenger.Language, currentAction.ParentId));
            }
            else
            {
                await senderService.SendMessage(
                    messenger,
                    builderService.BuildActionSelectedMessage(messenger.Language, currentAction.ActionName),
                    replyMarkup: builderService.BuildActionButtions(messenger.Language, actionService.GetChildActions(currentAction.ActionId), true));
            }
        }
    }
}