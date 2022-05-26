using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramBot.Infrastructure.Action.Constants;
using TelegramBot.Infrastructure.Action.Models;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Resource.constants;
using TelegramBot.Infrastructure.Setting.Models;

namespace TelegramBot.Infrastructure.Action.Builders.ActionName
{
    public class UpdateLanguageActionName : IActionNameBuilder
    {
        private readonly IResourceService resourceService;

        public UpdateLanguageActionName(IResourceService resourceService)
        {
            this.resourceService = resourceService;
        }

        public async Task<string> BuildActionName(BotAction action, CustBotAccount account)
        {
            var resource = await resourceService.GetResource(action.ActionId.ToString(), ResourceType.ActionName, account.Language);
            var language = GetLanguageCode(action.ActionId);
            var isCurrentLanguage = account.Language.Equals(language.Code);

            return
                (isCurrentLanguage ? "\u2705 " : string.Empty) +
                resource.Value +
                (!string.IsNullOrEmpty(language.EmojiCharacter) ? $" {language.EmojiCharacter}" : string.Empty);
        }

        public static Language GetLanguageCode(int actionId)
        {
            return actionId switch
            {
                ActionIds.English => new Language("en-US", "\uD83C\uDDEC\uD83C\uDDE7"),
                ActionIds.ChineseTraditional => new Language("zh-TW", "\uD83C\uDDE8\uD83C\uDDF3"),
                ActionIds.ChineseSimplified => new Language("zh-CN", "\uD83C\uDDE8\uD83C\uDDF3"),
                ActionIds.Thai => new Language("th-TH", "\uD83C\uDDF9\uD83C\uDDED"),
                ActionIds.Vietnamese => new Language("vi-VN", "\uD83C\uDDFB\uD83C\uDDF3"),
                _ => new Language("", ""),
            };
        }
    }
}