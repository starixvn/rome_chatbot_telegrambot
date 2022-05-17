using System.Threading.Tasks;
using TelegramBot.Infrastructure.Action.Models;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Resource.constants;
using TelegramBot.Infrastructure.Setting.Models;

namespace TelegramBot.Infrastructure.Action.Builders.ActionName
{
    public class UpdateNotificationStatusActionName : IActionNameBuilder
    {
        private readonly IResourceService resourceService;

        public UpdateNotificationStatusActionName(IResourceService resourceService)
        {
            this.resourceService = resourceService;
        }

        public async Task<string> BuildActionName(BotAction action, CustBotMessenger messenger)
        {
            var resource = await resourceService.GetResource(action.ActionId.ToString(), ResourceType.ActionName, messenger.Language);

            return resource.Value;
        }
    }
}