using TelegramBot.Modules.Action.Models;
using TelegramBot.Modules.Resource;
using TelegramBot.Modules.Resource.constants;
using TelegramBot.Modules.Setting.Models;

namespace TelegramBot.Modules.Action.Builders.ActionName
{
    public class DefaultActionNameBuilder : IActionNameBuilder
    {
        private readonly IResourceService resourceService;

        public DefaultActionNameBuilder(IResourceService resourceService)
        {
            this.resourceService = resourceService;
        }

        public async Task<string> BuildActionName(BotAction action, CustBotAccount account)
            => (await resourceService.GetResource(action.ActionId.ToString(), ResourceType.ActionName, account.Language))?.Value;
    }
}