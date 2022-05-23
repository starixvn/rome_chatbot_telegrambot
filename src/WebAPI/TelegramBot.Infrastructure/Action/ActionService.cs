using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fanex.Caching;
using Fanex.Data.Repository;
using Newtonsoft.Json;
using TelegramBot.Infrastructure.Action.Builders.ActionName;
using TelegramBot.Infrastructure.Action.Constants;
using TelegramBot.Infrastructure.Action.Criteria;
using TelegramBot.Infrastructure.Action.Models;
using TelegramBot.Infrastructure.Function.Handlers;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Message.Models;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Resource.constants;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Caching;
using TelegramBot.Infrastructure.Shared.Configurations;

namespace TelegramBot.Infrastructure.Action
{
    public interface IActionService
    {
        Task<IList<MessageButton>> BuildRootActionButtons(CustBotAccount account);

        Task<Message.Models.Message> BuildNextActionsMessage(CustBotAccount account, BotAction currentAction);

        Task<BotAction> GetActionById(int id);

        Task<BotAction> GetRootActionByFunctionId(short functionId);

        Task<IEnumerable<BotAction>> GetChildActions(CustBotAccount account, int parentId);

        Task<BotAction> GetActionByCommand(string command);

        Task<IList<CustBotAction>> GetUserActions(int userId);

        bool IsMessageAction(BotAction action);

        Task UpdateUserAction(long id, string userResponse, string compiledCommand = "");

        Task InsertUserAction(int userId, short actionId, string userResponse, string compiledCommand = "");

        Task<string> CompileCommand(IEnumerable<CustBotAction> userActions, string userResponse);

        Task ResetUserActions(int userId);

        Task<IList<MessageButton>> BuildActionButtons(CustBotAccount account, IEnumerable<BotAction> actions);

        Task<string> GetActionDescription(short actionId, string language);
    }

    public class ActionService : IActionService
    {
        private const string AllActionsCacheKey = "TelegramBot.Infrastructure.Action.ActionService.AllActions";
        private readonly IDynamicRepository dynamicRepository;
        private readonly WebAPIConfiguration webAPIConfiguration;
        private readonly IMessageBuilder messageBuilder;
        private readonly IFunctionHandlerFactory functionHandlerFactory;
        private readonly IActionNameBuilderFactory actionNameBuilderFactory;
        private readonly ICacheService cacheService;
        private readonly IResourceService resourceService;

        public ActionService(
            IDynamicRepository dynamicRepository,
            WebAPIConfiguration webAPIConfiguration,
            IMessageBuilder messageBuilder,
            IFunctionHandlerFactory functionHandlerFactory,
            IActionNameBuilderFactory actionNameBuilderFactory,
            ICacheServiceFactory cacheServiceFactory,
            IResourceService resourceService)
        {
            this.dynamicRepository = dynamicRepository;
            this.webAPIConfiguration = webAPIConfiguration;
            this.messageBuilder = messageBuilder;
            this.functionHandlerFactory = functionHandlerFactory;
            this.actionNameBuilderFactory = actionNameBuilderFactory;
            this.cacheService = cacheServiceFactory.Get();
            this.resourceService = resourceService;
        }

        public async Task<string> GetActionDescription(short actionId, string language)
            => (await this.resourceService.GetResource(actionId.ToString(), ResourceType.ActionDescription, language))?.Value;

        public async Task<BotAction> GetRootActionByFunctionId(short functionId)
            => (await GetRootActions()).FirstOrDefault(a => a.FunctionId == functionId);

        public async Task<Message.Models.Message> BuildNextActionsMessage(CustBotAccount account, BotAction currentAction)
        {
            var nextActions = await GetChildActions(account, currentAction.ActionId);

            var text = string.Empty;
            IList<MessageButton> buttons = new List<MessageButton>();
            if (nextActions.Count() == 1 && IsMessageAction(nextActions.FirstOrDefault()))
            {
                var nextAction = nextActions.FirstOrDefault();
                await InsertUserAction(account.UserId, nextAction.ActionId, string.Empty);
                var builder = actionNameBuilderFactory.GetBuilder(nextAction.ActionId);
                text = await builder.BuildActionName(nextAction, account);
            }
            else
            {
                var builder = actionNameBuilderFactory.GetBuilder(currentAction.ActionId);
                var actionName = await builder.BuildActionName(currentAction, account);
                text = messageBuilder.BuildActionSelectedMessage(account.Language, actionName);
                buttons = await BuildActionButtons(account, nextActions);
            }

            return await messageBuilder.BuildMessageWithBackButton(text, buttons, currentAction.ParentId, account.Language);
        }

        public async Task<IList<MessageButton>> BuildActionButtons(CustBotAccount account, IEnumerable<BotAction> actions)
        {
            var buttons = new List<MessageButton>();
            foreach (var action in actions)
            {
                if (action.Type == ActionTypes.Downline)
                {
                    var usernames = await functionHandlerFactory.GetHandler(action.FunctionId).GetTopDownlineUsername(account);
                    foreach (var username in usernames)
                    {
                        buttons.Add(new MessageButton(
                            username,
                            JsonConvert.SerializeObject(new
                            {
                                ActionId = action.ActionId,
                                Value = username
                            })));
                    }
                }
                else
                {
                    var builder = actionNameBuilderFactory.GetBuilder(action.ActionId);
                    var actionName = await builder.BuildActionName(action, account);
                    buttons.Add(new MessageButton(
                        actionName,
                        JsonConvert.SerializeObject(new
                        {
                            ActionId = action.ActionId,
                            Value = actionName
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
                        ? (action.Command.StartsWith("&lt;") ? string.Empty : action.Command)
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

        public async Task<IList<MessageButton>> BuildRootActionButtons(CustBotAccount account)
        {
            var rootActions = await GetRootActions();
            var buttons = new List<MessageButton>();

            foreach (var action in rootActions)
            {
                var builder = actionNameBuilderFactory.GetBuilder(action.ActionId);
                var actionName = await builder.BuildActionName(action, account);
                buttons.Add(new MessageButton(actionName, JsonConvert.SerializeObject(new { ActionId = action.ActionId, Value = actionName })));
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

        public async Task<BotAction> GetActionByCommand(string command)
            => (await GetAllActions()).FirstOrDefault(action => action.Command == command);

        public async Task<IEnumerable<BotAction>> GetChildActions(CustBotAccount account, int parentId)
            => (await GetAllActions()).Where(action => action.ParentId == parentId && account.Functions.Any(f => f.FunctionId == action.FunctionId));

        private async Task<IEnumerable<BotAction>> GetRootActions()
            => (await GetAllActions()).Where(action => action.ParentId == 0);

        private async Task<IEnumerable<BotAction>> GetAllActions()
            => await cacheService.GetOrSetAsync(
                AllActionsCacheKey,
                () => dynamicRepository.FetchAsync<BotAction>(new BotActionSelCriteria(!webAPIConfiguration.IsProduction)),
                new CacheItemOptions().SetAbsoluteExpiration(new TimeSpan(1, 0, 0))).ConfigureAwait(false);

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