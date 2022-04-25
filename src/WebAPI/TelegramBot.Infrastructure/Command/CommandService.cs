using Alpha.API.Client.Services;
using Fanex.Caching;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using TelegramBot.Infrastructure.Action;
using TelegramBot.Infrastructure.Action.Models;
using TelegramBot.Infrastructure.Bot;
using TelegramBot.Infrastructure.Command.Models;
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

        public CommandService(
            IBuilderService builderService,
            ISenderService senderService,
            ISettingService settingService,
            IBotService botService,
            ICustomerService customerService,
            IActionService actionService,
            ICacheService cacheService)
        {
            this.builderService = builderService;
            this.senderService = senderService;
            this.settingService = settingService;
            this.botService = botService;
            this.customerService = customerService;
            this.actionService = actionService;
            this.cacheService = cacheService;
        }

        public async Task ExecuteAsync(long messengerBotId, long chatId, string message, bool isCallback)
        {
            if (messengerBotId == 0 || chatId == 0 || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var bot = await botService.GetAsync(messengerBotId);
            var custBotAccount = await settingService.GetCustBotAccount(chatId, bot.BotId);
            var messenger = await settingService.GetCustBotMessenger(custBotAccount.CustId, custBotAccount.SubAccId);

            var action = message.Split(' ')[0] switch
            {
                "/help" => HandleHelpCommand(messenger, !custBotAccount.IsSubAccount || (await customerService.IsSubAccHasReportPermissionAsync(custBotAccount.CustId, custBotAccount.SubAccId))),
                "/menu" => HandleMenuCommand(messenger),
                _ => HandleCommand(messenger, message, isCallback),
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

        private async Task HandleMenuCommand(CustBotMessenger messenger)
        {
            await senderService.SendMessage(
                messenger,
                builderService.BuildMenuMessage(messenger.Language),
                replyMarkup: builderService.BuildActionButtions(actionService.GetRootActions()));
        }

        private async Task HandleCommand(CustBotMessenger messenger, string message, bool isCallback)
        {
            if (isCallback)
            {
                await HandleUserReplyByButton(messenger, message);
            }
        }

        private async Task HandleUserReplyByButton(CustBotMessenger messenger, string message)
        {

            var callBackData = JsonConvert.DeserializeObject<CallBackData>(message);

            if (callBackData == null)
            {
                throw new InvalidOperationException($"Wrong call back data format {message}");
            }

            var currentAction = actionService.GetActionById(callBackData.ActionId);
            if (currentAction == null || !IsValidAction(messenger, currentAction))
            {
                await SendNotRecognizedCommandMessage(messenger);
                return;
            }

            var compiledCommand = this.cacheService.Get<string>($"{messenger.UserId}.command");
            this.cacheService.Set($"{messenger.UserId}.command", string.IsNullOrEmpty(compiledCommand) ? $"{compiledCommand} {currentAction.Command}" : currentAction.Command);

            if (!currentAction.IsEndAction)
            {
                await SendNextActionsMessage(messenger, currentAction);
                return;
            }
        }

        private static bool IsValidAction(CustBotMessenger messenger, BotAction action)
            => messenger.Functions.Any(f => f.FunctionId == action.FunctionId);

        private async Task SendNotRecognizedCommandMessage(CustBotMessenger messenger)
            => await senderService.SendMessage(
                messenger,
                builderService.BuildNotRecognizedCommandMessage(messenger.Language),
                replyMarkup: builderService.BuildActionButtions(actionService.GetRootActions()));

        private async Task SendNextActionsMessage(CustBotMessenger messenger, BotAction currentAction)
            => await senderService.SendMessage(
                messenger,
                builderService.BuildActionSelectedMessage(messenger.Language, currentAction.ActionName),
                replyMarkup: builderService.BuildActionButtions(actionService.GetChildActions(currentAction.ActionId)));
    }
}