using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using Newtonsoft.Json;
using TelegramBot.Infrastructure.Action.Constants;
using TelegramBot.Infrastructure.Action.Criteria;
using TelegramBot.Infrastructure.Action.Models;
using TelegramBot.Infrastructure.Command.Models;
using TelegramBot.Infrastructure.Function.Handlers;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Message.Models;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;

namespace TelegramBot.Infrastructure.Action
{
    public interface IActionService
    {
        Task<BotAction> GetActionById(int id);

        Task<IList<CustBotAction>> GetUserActions(int userId);

        Task SendRootActionsMessage(CustBotMessenger messenger, string message);

        Task HandleButtonAction(CustBotAccount account, CustBotMessenger messenger, BotAction currentAction);

        Task UpdateUserAction(long id, string userResponse, string compiledCommand = "");

        Task InsertUserAction(int userId, short actionId, string userResponse, string compiledCommand = "");

        Task SendNextActionsMessage(CustBotAccount account, CustBotMessenger messenger, BotAction currentAction);

        Task<string> CompileCommand(IEnumerable<CustBotAction> userActions, string userResponse);

        Task ResetUserActions(int userId);
    }

    public class ActionService : IActionService
    {
        private readonly IDynamicRepository dynamicRepository;
        private readonly WebAPIConfiguration webAPIConfiguration;
        private readonly IMessageSender messageSender;
        private readonly IMessageBuilder messageBuilder;
        private readonly IFunctionHandlerFactory functionHandlerFactory;

        public ActionService(
            IDynamicRepository dynamicRepository,
            WebAPIConfiguration webAPIConfiguration,
            IMessageSender messageSender,
            IMessageBuilder messageBuilder,
            IFunctionHandlerFactory functionHandlerFactory)
        {
            this.dynamicRepository = dynamicRepository;
            this.webAPIConfiguration = webAPIConfiguration;
            this.messageSender = messageSender;
            this.messageBuilder = messageBuilder;
            this.functionHandlerFactory = functionHandlerFactory;
        }

        public async Task HandleButtonAction(CustBotAccount account, CustBotMessenger messenger, BotAction currentAction)
        {
            await InsertUserAction(account.UserId, currentAction.ActionId, string.Empty);
            await SendNextActionsMessage(account, messenger, currentAction);
        }

        public async Task SendNextActionsMessage(CustBotAccount account, CustBotMessenger messenger, BotAction currentAction)
        {
            var nextActions = await GetChildActions(currentAction.ActionId);
            if (nextActions.Count() == 1 && IsMessageAction(nextActions.FirstOrDefault()))
            {
                var messageAction = nextActions.FirstOrDefault();
                await InsertUserAction(messenger.UserId, messageAction.ActionId, string.Empty);
                await messageSender.SendMessageWithCancelButtion(
                    messenger,
                    await messageBuilder.BuildActionName(messenger.Language, messageAction.ActionId),
                    currentAction.ParentId);
            }
            else
            {
                await SendMessageWithButtons(account, messenger, currentAction, nextActions);
            }
        }

        private async Task SendMessageWithButtons(CustBotAccount account, CustBotMessenger messenger, BotAction currentAction, IEnumerable<BotAction> nextActions)
        {
            var actionName = await messageBuilder.BuildActionName(messenger.Language, currentAction.ActionId);
            var message = messageBuilder.BuildActionSelectedMessage(messenger.Language, actionName);
            var userActions = await GetUserActions(messenger.UserId);
            var buttons = new List<MessageButton>();
            foreach (var action in nextActions)
            {
                if (action.Type == ActionTypes.Downline)
                {
                    var usernames = await functionHandlerFactory.GetHandler(currentAction.FunctionId).GetTopDownlineUsername(account);
                    foreach (var username in usernames)
                    {
                        buttons.Add(new MessageButton(
                            username,
                            JsonConvert.SerializeObject(new CallBackData { ActionId = action.ActionId, CompiledCommand = await CompileCommand(userActions, username) })
                            ));
                    }

                    return;
                }

                buttons.Add(new MessageButton(
                    await messageBuilder.BuildActionName(messenger.Language, action.ActionId),
                    JsonConvert.SerializeObject(new CallBackData { ActionId = action.ActionId, CompiledCommand = action.IsEndAction ? await CompileCommand(userActions, action.Command) : string.Empty })
                    ));
            }

            await messageSender.SendMessageWithBackToMainMenuButton(
                    messenger,
                    messageBuilder.BuildActionSelectedMessage(messenger.Language, currentAction.ActionName),
                    buttons);
        }

        public async Task<string> CompileCommand(IEnumerable<CustBotAction> userActions, string userResponse)
        {
            var lastAction = userActions.LastOrDefault();
            var actionChain = await GetActionChain(lastAction.ActionId);

            var commandBuilder = new StringBuilder();
            foreach (var userAction in userActions.Where(userAction => actionChain.Any(action => action.ActionId == userAction.ActionId)))
            {
                var action = actionChain.FirstOrDefault(act => act.ActionId == userAction.ActionId)
                            ?? throw new InvalidOperationException($"Cannot found action {userAction.ActionId} of user {userAction.UserId}");
                var actionCommand = action.Type == ActionTypes.Button
                    ? action.Command
                    : userAction.UserResponse;

                if (!string.IsNullOrEmpty(actionCommand))
                {
                    commandBuilder.Append(actionCommand).Append(" ");
                }
            }

            commandBuilder.Append(userResponse);

            return commandBuilder.ToString().TrimEnd();
        }

        public async Task SendRootActionsMessage(CustBotMessenger messenger, string message)
        {
            await ResetUserActions(messenger.UserId);
            var rootActions = await GetRootActions();
            var buttons = new List<MessageButton>();

            foreach (var action in rootActions)
            {
                buttons.Add(new MessageButton(
                    await messageBuilder.BuildActionName(messenger.Language, action.ActionId),
                    JsonConvert.SerializeObject(new CallBackData { ActionId = action.ActionId, CompiledCommand = action.IsEndAction ? action.Command : string.Empty })
                    ));
            }

            await messageSender.SendMessage(messenger, message, buttons);
        }

        private async Task<IEnumerable<BotAction>> GetActionChain(int actionId)
        {
            var endAction = (await GetAllActions()).FirstOrDefault(action => action.ActionId == actionId);
            var actionChain = new List<BotAction>();
            if (endAction != null)
            {
                var tempAction = endAction;
                while (tempAction != null)
                {
                    actionChain.Add(tempAction);
                    tempAction = await GetPrevAction(tempAction.ParentId);
                }

                actionChain.Reverse();
            }

            return actionChain;
        }

        private async Task<BotAction> GetPrevAction(short parentId)
          => (await GetAllActions()).FirstOrDefault(action => action.ActionId == parentId);

        public async Task<BotAction> GetActionById(int id)
            => (await GetAllActions()).FirstOrDefault(action => action.ActionId == id);

        private async Task<IEnumerable<BotAction>> GetChildActions(int parentId)
            => (await GetAllActions()).Where(action => action.ParentId == parentId);

        private async Task<IEnumerable<BotAction>> GetRootActions()
            => (await GetAllActions()).Where(action => action.ParentId == 0);

        private async Task<IEnumerable<BotAction>> GetAllActions()
            => await dynamicRepository.FetchAsync<BotAction>(new BotActionSelCriteria(!webAPIConfiguration.IsProduction)).ConfigureAwait(false);


        private static bool IsMessageAction(BotAction action)
            => action.Type == ActionTypes.Message;

        public async Task<IList<CustBotAction>> GetUserActions(int userId)
        {
            return (await dynamicRepository.FetchAsync<CustBotAction>(new CustBotActionInChainSelCriteria(userId)).ConfigureAwait(false))
                .DistinctBy(userAction => userAction.ActionId)
                .ToList();
        }

        public async Task InsertUserAction(int userId, short actionId, string userResponse, string compiledCommand = "")
        {
            await dynamicRepository.ExecuteAsync(new CustBotActionInsCriteria(userId, actionId, userResponse, compiledCommand)).ConfigureAwait(false);
        }

        public async Task UpdateUserAction(long id, string userResponse, string compiledCommand = "")
        {
            await dynamicRepository.ExecuteAsync(new CustBotActionCommandUpdCriteria(id, userResponse, compiledCommand)).ConfigureAwait(false);
        }

        public async Task ResetUserActions(int userId)
        {
            await dynamicRepository.ExecuteAsync(new CustBotActionStatusUpdCriteria(userId)).ConfigureAwait(false);
        }
    }
}