using Fanex.Caching;
using Newtonsoft.Json;
using System;
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
        private static readonly string AllActionsCacheKey = "TelegramBot.Infrastructure.Action.AllActions";

        public ActionService()
        {
        }

        public InlineKeyboardMarkup GetRootActions()
        {
            var rootActions = GetAllActions();

            var numberOfRows = rootActions.Count() % 2 > 0 ? rootActions.Count() / 2 : (rootActions.Count() / 2) + 1;
            var rowIndex = 0;
            var columnIndex = 0;
            var buttonRows = new InlineKeyboardButton[numberOfRows][];

            foreach (var action in rootActions)
            {
                buttonRows[rowIndex][columnIndex] = InlineKeyboardButton.WithCallbackData(action.ActionName, JsonConvert.SerializeObject(new { ActionId = action.ActionId }));
                columnIndex++;
                if (columnIndex == 2)
                {
                    rowIndex++;
                    columnIndex = 0;
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