using System.Threading.Tasks;
using TelegramBot.Infrastructure.Action.Models;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Resource.constants;
using TelegramBot.Infrastructure.Setting.Models;

namespace TelegramBot.Infrastructure.Action.Builders.ActionName
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