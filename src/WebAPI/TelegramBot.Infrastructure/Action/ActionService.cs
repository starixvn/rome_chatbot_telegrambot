using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Infrastructure.Action.Models;

namespace TelegramBot.Infrastructure.Action
{
    public interface IActionService
    {
        InlineKeyboardMarkup GetRootActions();
    }

    public class ActionService : IActionService
    {
        private const int NumberButtonInRow = 2;


        public InlineKeyboardMarkup GetRootActions()
        {
            var rootActions = GetAllActions().ToList();

            var numberOfRows = rootActions.Count % NumberButtonInRow > 0 ? (rootActions.Count / NumberButtonInRow) + 1 : (rootActions.Count / NumberButtonInRow);
            var buttonRows = new InlineKeyboardButton[numberOfRows][];

            for (var i = 0; i < numberOfRows; i++)
            {
                var actionIndex = i * numberOfRows;
                if (actionIndex == rootActions.Count - 1)
                {
                    buttonRows[i] = new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData(rootActions[actionIndex].ActionName, JsonConvert.SerializeObject(new { ActionId= rootActions[actionIndex].ActionId}))
                    };
                }
                else
                {
                    buttonRows[i] = new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData(rootActions[actionIndex].ActionName, JsonConvert.SerializeObject(new { ActionId= rootActions[actionIndex].ActionId})),
                        InlineKeyboardButton.WithCallbackData(rootActions[actionIndex + 1].ActionName, JsonConvert.SerializeObject(new { ActionId= rootActions[actionIndex + 1].ActionId}))
                    };
                }

            }

            return buttonRows;
        }


        private IEnumerable<BotAction> GetAllActions()
        {
            return new List<BotAction>()
                    {
                        new BotAction(1, "Outstanding",1,"/os",1,0,1),
                        new BotAction(2,"Balance Info",2,"/bl",1,0,1),
                        new BotAction(3,"Today Win Loss",3,"/wl",1,0,1),
                        new BotAction(4,"Yesterday Win Loss",4,"/ywl",1,0,1),
                        new BotAction(5,"Settings",5,"/ywl",1,0,1),
                    };
        }
    }
}