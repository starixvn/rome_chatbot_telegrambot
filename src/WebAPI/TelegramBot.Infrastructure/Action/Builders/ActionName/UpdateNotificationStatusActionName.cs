using System.Linq;
using System.Threading.Tasks;
using TelegramBot.Infrastructure.Action.Models;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Resource.constants;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Resources;

namespace TelegramBot.Infrastructure.Action.Builders.ActionName
{
    public class UpdateNotificationStatusActionName : IActionNameBuilder
    {
        private readonly IResourceService resourceService;

        public UpdateNotificationStatusActionName(IResourceService resourceService)
        {
            this.resourceService = resourceService;
        }

        public async Task<string> BuildActionName(BotAction action, CustBotAccount account)
        {
            var resource = await resourceService.GetResource(action.ActionId.ToString(), ResourceType.ActionName, account.Language);
            var function = account.Functions.FirstOrDefault(f => f.FunctionId == short.Parse(action.Command));

            return string.Format(resource.Value, ResourceHelper.Get(function.Enabled ? nameof(SharedResource.ON) : nameof(SharedResource.OFF)));
        }
    }
}