using TelegramBot.Modules.Action.Constants;
using TelegramBot.Modules.Action.Models;
using TelegramBot.Modules.Resource;
using TelegramBot.Modules.Resource.constants;
using TelegramBot.Modules.Setting.Models;

namespace TelegramBot.Modules.Action.Builders.ActionName
{
    public class UpdateNotificationStatusActionName : IActionNameBuilder
    {
        private readonly IResourceService resourceService;

        public UpdateNotificationStatusActionName(IResourceService resourceService)
            => this.resourceService = resourceService;

        public async Task<string> BuildActionName(BotAction action, CustBotAccount account)
        {
            var resource = await resourceService.GetResource(action.ActionId.ToString(), ResourceType.ActionName, account.Language);
            var function = account.Functions.FirstOrDefault(f => f.FunctionId == short.Parse(action.Command));

            return (function.Enabled ? $"{Unicode.Bell}" : $"{Unicode.BellWithSlash}") + resource.Value;
        }
    }
}