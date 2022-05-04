using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using Newtonsoft.Json;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Infrastructure.Action.Constants;
using TelegramBot.Infrastructure.Action.Criteria;
using TelegramBot.Infrastructure.Action.Models;
using TelegramBot.Infrastructure.Message;
using TelegramBot.Infrastructure.Message.Models;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.Infrastructure.Shared.Resources;

namespace TelegramBot.Infrastructure.Action
{
    public interface IActionService
    {
        Task<BotAction> GetActionById(int id);

        Task<IEnumerable<BotAction>> GetChildActions(int parentId);

        Task SendRootActionsMessage(CustBotMessenger messenger, string message);

        Task SendNextActionsMessage(CustBotMessenger messenger, BotAction currentAction);
    }

    public class ActionService : IActionService
    {
        private const int NumberButtonInRow = 2;
        private readonly IDynamicRepository dynamicRepository;
        private readonly WebAPIConfiguration webAPIConfiguration;
        private readonly IMessageSender messageSender;
        private readonly IMessageBuilder messageBuilder;

        public ActionService(
            IDynamicRepository dynamicRepository,
            WebAPIConfiguration webAPIConfiguration,
            IMessageSender messageSender,
            IMessageBuilder messageBuilder)
        {
            this.dynamicRepository = dynamicRepository;
            this.webAPIConfiguration = webAPIConfiguration;
            this.messageSender = messageSender;
            this.messageBuilder = messageBuilder;
        }

        public async Task SendNextActionsMessage(CustBotMessenger messenger, BotAction currentAction)
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
                await messageSender.SendMessageWithBackToMainMenuButton(
                    messenger,
                    messageBuilder.BuildActionSelectedMessage(messenger.Language, currentAction.ActionName),
                    await BuildButtons(messenger.Language, nextActions));
            }
        }

        public async Task SendRootActionsMessage(CustBotMessenger messenger, string message)
        {
            var rootActions = await GetRootActions();
            await messageSender.SendMessage(messenger, message, await BuildButtons(messenger.Language, rootActions));
        }

        public async Task<BotAction> GetActionById(int id)
            => (await GetAllActions()).FirstOrDefault(action => action.ActionId == id);

        public async Task<IEnumerable<BotAction>> GetChildActions(int parentId)
            => (await GetAllActions()).Where(action => action.ParentId == parentId);

        private async Task<IEnumerable<BotAction>> GetRootActions()
            => (await GetAllActions()).Where(action => action.ParentId == 0);

        private async Task<IEnumerable<BotAction>> GetAllActions()
            => await dynamicRepository.FetchAsync<BotAction>(new BotActionSelCriteria(!webAPIConfiguration.IsProduction)).ConfigureAwait(false);

        private async Task<IList<MessageButton>> BuildButtons(string language, IEnumerable<BotAction> actions)
        {
            var buttons = new List<MessageButton>();

            foreach (var action in actions)
            {
                buttons.Add(new MessageButton(await messageBuilder.BuildActionName(language, action.ActionId), JsonConvert.SerializeObject(new { ActionId = action.ActionId })));
            }

            return buttons;
        }


        private static bool IsMessageAction(BotAction action)
            => action.Type == ActionTypes.Message;

        private async Task<IList<CustBotAction>> GetUserActions(int userId)
        {
            return (await dynamicRepository.FetchAsync<CustBotAction>(new CustBotActionInChainSelCriteria(userId)).ConfigureAwait(false))
                .DistinctBy(userAction => userAction.ActionId)
                .ToList();
        }

        private async Task InsertUserAction(int userId, short actionId, string userResponse, string compiledCommand = "")
        {
            await dynamicRepository.ExecuteAsync(new CustBotActionInsCriteria(userId, actionId, userResponse, compiledCommand)).ConfigureAwait(false);
        }
    }
}