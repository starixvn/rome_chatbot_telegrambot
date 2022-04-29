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
                await SendMessageActionMessage(messenger, nextActions.FirstOrDefault().ActionName, currentAction.ParentId);
            }
            else
            {
                await SendActionsMessage(
                    messenger,
                    messageBuilder.BuildActionSelectedMessage(messenger.Language, currentAction.ActionName),
                    nextActions);
            }
        }

        public async Task SendRootActionsMessage(CustBotMessenger messenger, string message)
        {
            var rootActions = await GetRootActions();
            await SendActionsMessage(messenger, message, rootActions);
        }

        public async Task<BotAction> GetActionById(int id)
            => (await GetAllActions()).FirstOrDefault(action => action.ActionId == id);

        public async Task<IEnumerable<BotAction>> GetChildActions(int parentId)
            => (await GetAllActions()).Where(action => action.ParentId == parentId);

        private async Task<IEnumerable<BotAction>> GetRootActions()
            => (await GetAllActions()).Where(action => action.ParentId == 0);

        private async Task<IEnumerable<BotAction>> GetAllActions()
            => await dynamicRepository.FetchAsync<BotAction>(new BotActionSelCriteria(!webAPIConfiguration.IsProduction)).ConfigureAwait(false);

        private InlineKeyboardMarkup BuildActionButtions(string language, IList<BotAction> actions, bool withBackToMainMenuButton = false)
        {
            var numberOfRows = actions.Count % NumberButtonInRow > 0 ? (actions.Count / NumberButtonInRow) + 1 : (actions.Count / NumberButtonInRow);
            numberOfRows = withBackToMainMenuButton ? numberOfRows + 1 : numberOfRows;
            var buttonRows = new InlineKeyboardButton[numberOfRows][];

            for (var i = 0; i < numberOfRows; i++)
            {
                if (withBackToMainMenuButton && i == numberOfRows - 1)
                {
                    buttonRows[i] = new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData(
                            ResourceHelper.Get(nameof(SharedResource.BackToMainMenu),language),
                            JsonConvert.SerializeObject(new {IsBackToMainMenu=true}))
                    };
                    break;
                }

                var actionIndex = i * NumberButtonInRow;
                if (actionIndex == actions.Count - 1)
                {
                    buttonRows[i] = new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData(actions[actionIndex].ActionName, JsonConvert.SerializeObject(new { ActionId= actions[actionIndex].ActionId}))
                    };
                }
                else
                {
                    buttonRows[i] = new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData(actions[actionIndex].ActionName, JsonConvert.SerializeObject(new { ActionId= actions[actionIndex].ActionId})),
                        InlineKeyboardButton.WithCallbackData(actions[actionIndex + 1].ActionName, JsonConvert.SerializeObject(new { ActionId= actions[actionIndex + 1].ActionId}))
                    };
                }
            }

            return buttonRows;
        }

        private async Task SendActionsMessage(CustBotMessenger messenger, string message, IEnumerable<BotAction> actions, bool withBackToMainMenuButton = false)
        {
            var actionButtons = BuildActionButtions(messenger.Language, actions.ToList(), withBackToMainMenuButton);
            await messageSender.SendMessage(messenger, message, replyMarkup: actionButtons);
        }

        private async Task SendMessageActionMessage(CustBotMessenger messenger, string actionName, short parentActionId)
        {
            await messageSender.SendMessage(
                messenger,
                actionName,
                replyMarkup: BuildCancelButtion(messenger.Language, parentActionId));
        }

        private static bool IsMessageAction(BotAction action)
            => action.Type == ActionTypes.Message;

        private InlineKeyboardMarkup BuildCancelButtion(string language, short parentActionId)
        {
            return new(new[] {
                new[] {
                    InlineKeyboardButton.WithCallbackData(
                        ResourceHelper.Get(nameof(SharedResource.Cancel)),
                        JsonConvert.SerializeObject(new { IsCancel = true, ActionId = parentActionId }))
                    }
                });
        }
    }
}