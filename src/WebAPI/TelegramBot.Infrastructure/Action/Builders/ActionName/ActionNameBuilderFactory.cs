using System.Collections.Generic;
using TelegramBot.Infrastructure.Action.Constants;
using TelegramBot.Infrastructure.Resource;

namespace TelegramBot.Infrastructure.Action.Builders.ActionName
{
    public interface IActionNameBuilderFactory
    {
        IActionNameBuilder GetBuilder(short actionId);
    }

    public class ActionNameBuilderFactory : IActionNameBuilderFactory
    {
        private IDictionary<short, IActionNameBuilder> builders = new Dictionary<short, IActionNameBuilder>();
        private readonly IResourceService resourceService;

        public ActionNameBuilderFactory(IResourceService resourceService)
        {
            this.resourceService = resourceService;
        }

        public IActionNameBuilder GetBuilder(short actionId)
        {
            if (builders.ContainsKey(actionId))
            {
                return builders[actionId];
            }

            IActionNameBuilder builder = null;
            switch (actionId)
            {
                case ActionIds.YesterdayWinLossNotification:
                case ActionIds.PersonalMessageNotification:
                case ActionIds.ProblemAccountNotification:
                case ActionIds.MemberWinLossLimitNotification:
                case ActionIds.ChangePasswordAlert:
                case ActionIds.ChangeSecurityCodeAlert:
                case ActionIds.AbnormalLoginAlert:
                    builder = new UpdateNotificationStatusActionName(resourceService);
                    break;
                default:
                    builder = new DefaultActionNameBuilder(resourceService);
                    break;
            }
            builders[actionId] = builder;

            return builder;
        }
    }
}