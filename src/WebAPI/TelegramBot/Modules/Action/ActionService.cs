using System.Text;
using Fanex.Caching;
using Fanex.Data.Repository;
using Newtonsoft.Json;
using TelegramBot.Modules.Action.Builders.ActionName;
using TelegramBot.Modules.Action.Builders.WebApp;
using TelegramBot.Modules.Action.Constants;
using TelegramBot.Modules.Action.Criteria;
using TelegramBot.Modules.Action.Models;
using TelegramBot.Modules.Function.Handlers;
using TelegramBot.Modules.Message;
using TelegramBot.Modules.Message.Models;
using TelegramBot.Modules.Resource;
using TelegramBot.Modules.Resource.constants;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Caching;
using TelegramBot.Shared.Configurations;

namespace TelegramBot.Modules.Action
{
    public interface IActionService
    {
        Task<IList<MessageButton>> BuildRootActionButtons(CustBotAccount account);

        Task<Message.Models.Message> BuildNextActionsMessage(CustBotAccount account, BotAction currentAction);

        Task<BotAction> GetActionById(int id, string system);

        Task<BotAction> GetRootActionByFunctionId(CustBotAccount account, short functionId);

        Task<IEnumerable<BotAction>> GetChildActions(CustBotAccount account, int parentId);

        Task<BotAction> GetActionByCommand(string command, string system);

        Task<IList<CustBotAction>> GetUserActions(int userId);

        Task UpdateUserAction(long id, string userResponse, string compiledCommand = "");

        Task InsertUserAction(int userId, short actionId, string userResponse, string compiledCommand = "");

        Task<string> CompileCommand(IEnumerable<CustBotAction> userActions, string userResponse, string system);

        Task ResetUserActions(int userId);

        Task<string> GetActionDescription(short actionId, string language);

        Task<string> GetActionName(BotAction action, CustBotAccount account);
    }

    public class ActionService : IActionService
    {
        private const string AllActionsCacheKey = "TelegramBot.Modules.Action.ActionService.AllActions";
        private readonly IDynamicRepository dynamicRepository;
        private readonly WebAPIConfiguration webAPIConfiguration;
        private readonly IMessageBuilder messageBuilder;
        private readonly IFunctionHandlerFactory functionHandlerFactory;
        private readonly IActionNameBuilderFactory actionNameBuilderFactory;
        private readonly ICacheService cacheService;
        private readonly IResourceService resourceService;
        private readonly IWebAppBuilderFactory webAppBuilderFactory;

        public ActionService(
            IDynamicRepository dynamicRepository,
            WebAPIConfiguration webAPIConfiguration,
            IMessageBuilder messageBuilder,
            IFunctionHandlerFactory functionHandlerFactory,
            IActionNameBuilderFactory actionNameBuilderFactory,
            IWebAppBuilderFactory webAppBuilderFactory,
            ICacheServiceFactory cacheServiceFactory,
            IResourceService resourceService)
        {
            this.dynamicRepository = dynamicRepository;
            this.webAPIConfiguration = webAPIConfiguration;
            this.messageBuilder = messageBuilder;
            this.functionHandlerFactory = functionHandlerFactory;
            this.actionNameBuilderFactory = actionNameBuilderFactory;
            this.webAppBuilderFactory = webAppBuilderFactory;
            cacheService = cacheServiceFactory.Get();
            this.resourceService = resourceService;
        }

        public async Task<string> GetActionDescription(short actionId, string language)
            => (await resourceService.GetResource(actionId.ToString(), ResourceType.ActionDescription, language))?.Value;

        public async Task<BotAction> GetRootActionByFunctionId(CustBotAccount account, short functionId)
            => (await GetRootActions(account)).FirstOrDefault(a => a.FunctionId == functionId);

        public async Task<Message.Models.Message> BuildNextActionsMessage(CustBotAccount account, BotAction currentAction)
        {
            var nextActions = await GetChildActions(account, currentAction.ActionId);
            IList<MessageButton> buttons = new List<MessageButton>();

            string text;
            if (nextActions.Count() == 1 && IsMessageAction(nextActions.FirstOrDefault()))
            {
                var nextAction = nextActions.FirstOrDefault();
                await InsertUserAction(account.UserId, nextAction.ActionId, string.Empty);
                text = await GetActionName(nextAction, account);
            }
            else
            {
                var actionName = await GetActionName(currentAction, account);
                text = messageBuilder.BuildActionSelectedMessage(account.Language, actionName);
                buttons = await BuildActionButtons(account, nextActions);
            }

            return messageBuilder.BuildMessageWithBackButton(text, buttons, currentAction.ParentId, account.Language);
        }

        public async Task<string> GetActionName(BotAction action, CustBotAccount account)
        {
            var builder = actionNameBuilderFactory.GetBuilder(action.ActionId);
            return await builder.BuildActionName(action, account);
        }

        private async Task<IList<MessageButton>> BuildActionButtons(CustBotAccount account, IEnumerable<BotAction> actions)
        {
            var buttons = new List<MessageButton>();
            var userActions = await GetUserActions(account.UserId);

            foreach (var action in actions)
            {
                var nextUserAction = new CustBotAction(long.MaxValue, account.UserId, action.ActionId, string.Empty, string.Empty, DateTime.Now);
                var nextUserActions = userActions.Concat(new List<CustBotAction> { nextUserAction }).ToList();
                switch (action.Type)
                {
                    case ActionTypes.Downline:
                        var usernames = await functionHandlerFactory.GetHandler(action.FunctionId).GetTopDownlineUsername(account);
                        foreach (var username in usernames)
                        {

                            buttons.Add(new MessageButton(
                                username,
                                JsonConvert.SerializeObject(new
                                {
                                    Id = action.ActionId,
                                    Cmd = await CompileCommand(nextUserActions, username, account.System)
                                })));
                        }
                        break;

                    case ActionTypes.WebApp:
                        var builder = webAppBuilderFactory.GetBuilder(action.ActionId);
                        buttons.Add(new MessageButton(text: await GetActionName(action, account), webAppInfo: builder.BuildWebAppInfo(account)));
                        break;

                    default:
                        buttons.Add(new MessageButton(
                            await GetActionName(action, account),
                            JsonConvert.SerializeObject(new
                            {
                                Id = action.ActionId,
                                Cmd = action.IsEndAction ? await CompileCommand(nextUserActions, action.Command, account.System) : string.Empty
                            })));
                        break;
                }
            }

            return buttons;
        }

        public async Task<string> CompileCommand(IEnumerable<CustBotAction> userActions, string userResponse, string system)
        {
            var lastAction = userActions.LastOrDefault();
            if (lastAction != null)
            {
                lastAction.UserResponse = userResponse;
                var actionChain = await GetActionChain(lastAction.ActionId, system);

                var commandBuilder = new StringBuilder();
                foreach (var action in actionChain)
                {
                    var userAction = userActions.FirstOrDefault(x => x.ActionId == action.ActionId);
                    var actionCommand = action.Type switch
                    {
                        ActionTypes.Button => action.Command,
                        ActionTypes.WebApp => string.Empty,
                        _ => userAction?.UserResponse ?? throw new InvalidOperationException($"Cannot found response of action {action.ActionId}")
                    };

                    if (!string.IsNullOrEmpty(actionCommand) && !actionCommand!.StartsWith("&lt;"))
                    {
                        commandBuilder.Append(actionCommand).Append(' ');
                    }
                }

                return commandBuilder.ToString().TrimEnd();
            }

            return string.Empty;
        }

        public async Task<IList<MessageButton>> BuildRootActionButtons(CustBotAccount account)
        {
            var rootActions = await GetRootActions(account);
            var buttons = new List<MessageButton>();

            foreach (var action in rootActions)
            {
                var builder = actionNameBuilderFactory.GetBuilder(action.ActionId);
                var actionName = await builder.BuildActionName(action, account);
                buttons.Add(new MessageButton(actionName, JsonConvert.SerializeObject(new { Id = action.ActionId, cmd = action.IsEndAction ? action.Command : string.Empty })));
            }

            return buttons;
        }

        private async Task<IEnumerable<BotAction>> GetActionChain(int actionId, string system)
        {
            var endAction = (await GetAllActions(system)).FirstOrDefault(action => action.ActionId == actionId);
            var actionChain = new List<BotAction>();
            if (endAction != null)
            {
                var tempAction = endAction;
                while (tempAction != null)
                {
                    actionChain.Add(tempAction);
                    tempAction = await GetPrevAction(tempAction.ParentId, system);
                }

                actionChain.Reverse();
            }

            return actionChain;
        }

        private async Task<BotAction> GetPrevAction(short parentId, string system)
            => (await GetAllActions(system)).FirstOrDefault(action => action.ActionId == parentId);

        public async Task<BotAction> GetActionById(int id, string system)
            => (await GetAllActions(system)).FirstOrDefault(action => action.ActionId == id);

        public async Task<BotAction> GetActionByCommand(string command, string system)
            => (await GetAllActions(system)).FirstOrDefault(action => action.Command?.ToLowerInvariant() == command.ToLowerInvariant());

        public async Task<IEnumerable<BotAction>> GetChildActions(CustBotAccount account, int parentId)
            => (await GetAllActions(account.System))
                .Where(action => action.ParentId == parentId && account.Functions.Any(f => f.FunctionId == action.FunctionId))
                .OrderBy(action => action.Order);

        private async Task<IEnumerable<BotAction>> GetRootActions(CustBotAccount account)
            => (await GetAllActions(account.System))
                .Where(action => action.ParentId == 0 && account.Functions.Any(f => f.FunctionId == action.FunctionId))
                .OrderBy(action => action.Order);

        private async Task<IEnumerable<BotAction>> GetAllActions(string system)
        {
            var hiddenActions = webAPIConfiguration.HiddenActionsBySystem.ContainsKey(system)
                ? webAPIConfiguration.HiddenActionsBySystem[system]
                : Array.Empty<short>();

            var actions = await cacheService.GetOrSetAsync(
                AllActionsCacheKey,
                () => dynamicRepository.FetchAsync<BotAction>(new BotActionSelCriteria(!webAPIConfiguration.IsProduction)),
                new CacheItemOptions().SetAbsoluteExpiration(new TimeSpan(1, 0, 0))).ConfigureAwait(false);

            return actions.Where(action => !hiddenActions.Contains(action.ActionId));
        }

        public async Task<IList<CustBotAction>> GetUserActions(int userId)
            => (await dynamicRepository.FetchAsync<CustBotAction>(new CustBotActionInChainSelCriteria(userId)).ConfigureAwait(false))
                .DistinctBy(userAction => userAction.ActionId)
                .ToList();

        public async Task InsertUserAction(int userId, short actionId, string userResponse, string compiledCommand = "")
            => await dynamicRepository.ExecuteAsync(new CustBotActionInsCriteria(userId, actionId, userResponse, compiledCommand)).ConfigureAwait(false);

        public async Task UpdateUserAction(long id, string userResponse, string compiledCommand = "")
            => await dynamicRepository.ExecuteAsync(new CustBotActionCommandUpdCriteria(id, userResponse, compiledCommand)).ConfigureAwait(false);

        public async Task ResetUserActions(int userId)
            => await dynamicRepository.ExecuteAsync(new CustBotActionStatusUpdCriteria(userId)).ConfigureAwait(false);

        private static bool IsMessageAction(BotAction action)
            => action.Type == ActionTypes.Message;
    }
}