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

        private readonly IDictionary<short, IActionNameBuilder> builders = new Dictionary<short, IActionNameBuilder>();
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

            if (ActionIds.NotificationSettingActionIds.Contains(actionId))
            {
                return new UpdateNotificationStatusActionName(resourceService);
            }

            if (ActionIds.LanguageSettingActionIds.Contains(actionId))
            {
                return new UpdateLanguageActionName(resourceService);
            }

            return new DefaultActionNameBuilder(resourceService);
        }
    }
}