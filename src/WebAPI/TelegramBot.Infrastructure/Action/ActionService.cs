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
using TelegramBot.Infrastructure.Function.Handlers;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Message.Models;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;

namespace TelegramBot.Infrastructure.Action
{
    public interface IActionService
    {
        Task<IList<MessageButton>> BuildRootActionButtons(string language);

        Task<IList<MessageButton>> BuildNextActionButtons(CustBotAccount account, short currentActionId);

        Task<BotAction> GetActionById(int id);

        Task<BotAction> GetRootActionByFunctionId(short functionId);

        Task<IEnumerable<BotAction>> GetChildActions(int parentId);

        Task<IList<CustBotAction>> GetUserActions(int userId);

        bool IsMessageAction(BotAction action);

        Task UpdateUserAction(long id, string userResponse, string compiledCommand = "");

        Task InsertUserAction(int userId, short actionId, string userResponse, string compiledCommand = "");

        Task<string> CompileCommand(IEnumerable<CustBotAction> userActions, string userResponse);

        Task ResetUserActions(int userId);

        Task<IList<MessageButton>> BuildActionButtons(CustBotAccount account, IEnumerable<BotAction> actions);
    }

    public class ActionService : IActionService
    {
        private readonly IDynamicRepository dynamicRepository;
        private readonly WebAPIConfiguration webAPIConfiguration;
        private readonly IMessageBuilder messageBuilder;
        private readonly IMessageFormatter messageFormatter;
        private readonly IFunctionHandlerFactory functionHandlerFactory;

        public ActionService(
            IDynamicRepository dynamicRepository,
            WebAPIConfiguration webAPIConfiguration,
            IMessageBuilder messageBuilder,
            IMessageFormatter messageFormatter,
            IFunctionHandlerFactory functionHandlerFactory)
        {
            this.dynamicRepository = dynamicRepository;
            this.webAPIConfiguration = webAPIConfiguration;
            this.messageBuilder = messageBuilder;
            this.messageFormatter = messageFormatter;
            this.functionHandlerFactory = functionHandlerFactory;
        }

        public async Task<BotAction> GetRootActionByFunctionId(short functionId)
            => (await GetRootActions()).FirstOrDefault(a => a.FunctionId == functionId);

        public async Task<IList<MessageButton>> BuildNextActionButtons(CustBotAccount account, short currentActionId)
        {
            var actions = await GetChildActions(currentActionId);
            return await BuildActionButtons(account, actions);
        }

        public async Task<IList<MessageButton>> BuildActionButtons(CustBotAccount account, IEnumerable<BotAction> actions)
        {
            var buttons = new List<MessageButton>();
            var userActions = await GetUserActions(account.UserId);
            foreach (var action in actions)
            {
                if (action.Type == ActionTypes.Downline)
                {

                    var usernames = await functionHandlerFactory.GetHandler(action.FunctionId).GetTopDownlineUsername(account);
                    foreach (var username in usernames)
                    {
                        var nextUserAction = new CustBotAction(int.MaxValue, 0, action.ActionId, username, string.Empty, DateTime.Now);
                        var nextUserActions = userActions.Concat(new List<CustBotAction> { nextUserAction }).ToList();
                        buttons.Add(new MessageButton(
                            username,
                            JsonConvert.SerializeObject(new
                            {
                                ActionId = action.ActionId,
                                CompiledCommand = await CompileCommand(nextUserActions, string.Empty)
                            })));
                    }
                }
                else
                {
                    var nextUserAction = new CustBotAction(int.MaxValue, 0, action.ActionId, action.Command, string.Empty, DateTime.Now);
                    var nextUserActions = userActions.Concat(new List<CustBotAction> { nextUserAction }).ToList();
                    buttons.Add(new MessageButton(
                        await messageBuilder.BuildActionName(account.Language, action.ActionId),
                        JsonConvert.SerializeObject(new
                        {
                            ActionId = action.ActionId,
                            CompiledCommand = action.IsEndAction ? await CompileCommand(nextUserActions, string.Empty) : string.Empty
                        })));
                }
            }

            return buttons;
        }

        public async Task<string> CompileCommand(IEnumerable<CustBotAction> userActions, string userResponse)
        {
            var lastAction = userActions.LastOrDefault();
            if (lastAction != null)
            {
                lastAction.UserResponse = userResponse;
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

                return commandBuilder.ToString().TrimEnd();
            }

            return string.Empty;
        }

        public async Task<IList<MessageButton>> BuildRootActionButtons(string language)
        {
            var rootActions = await GetRootActions();
            var buttons = new List<MessageButton>();

            foreach (var action in rootActions)
            {
                buttons.Add(new MessageButton(
                    await messageBuilder.BuildActionName(language, action.ActionId),
                    JsonConvert.SerializeObject(new { ActionId = action.ActionId, CompiledCommand = action.IsEndAction ? action.Command : string.Empty })
                    ));
            }

            return buttons;
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

        public async Task<IEnumerable<BotAction>> GetChildActions(int parentId)
            => (await GetAllActions()).Where(action => action.ParentId == parentId);

        private async Task<IEnumerable<BotAction>> GetRootActions()
            => (await GetAllActions()).Where(action => action.ParentId == 0);

        private async Task<IEnumerable<BotAction>> GetAllActions()
            => await dynamicRepository.FetchAsync<BotAction>(new BotActionSelCriteria(!webAPIConfiguration.IsProduction)).ConfigureAwait(false);

        public bool IsMessageAction(BotAction action)
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