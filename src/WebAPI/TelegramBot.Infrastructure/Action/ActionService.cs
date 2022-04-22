using System.Collections.Generic;
using System.Linq;
using TelegramBot.Infrastructure.Action.Models;

namespace TelegramBot.Infrastructure.Action
{
    public interface IActionService
    {
        IList<BotAction> GetRootActions();

        BotAction GetActionById(int id);

        IList<BotAction> GetChildActions(int parentId);
    }

    public class ActionService : IActionService
    {
        private const int NumberButtonInRow = 2;

        public BotAction GetActionById(int id)
            => GetAllActions().FirstOrDefault(action => action.ActionId == id);

        public IList<BotAction> GetChildActions(int parentId)
            => GetAllActions().Where(action => action.ParentId == parentId).ToList();

        public IList<BotAction> GetRootActions()
            => GetAllActions().Where(action => action.ParentId == 0).ToList();

        private IEnumerable<BotAction> GetAllActions()
        {
            return new List<BotAction>()
            {
                new BotAction(1, "Outstanding",1,"/os",1,0,false,1),
                new BotAction(2, "Balance Info",2,"/bl",1,0,false,1),
                new BotAction(3,"Today Win Loss",3,"/wl",1,0,false,1),
                new BotAction(4,"Yesterday Win Loss",4,"/ywl",1,0,false,1),
                new BotAction(5,"Settings",5,"/ywl",1,0,false,1),
                new BotAction(6, "My Outstanding",1,"me",1,1,true,1),
                new BotAction(6, "Enter downline username",1,"",1,1,false,1),
                new BotAction(6, "Top Outstanding",1,"50",1,1,true,1),
            };
        }
    }
}