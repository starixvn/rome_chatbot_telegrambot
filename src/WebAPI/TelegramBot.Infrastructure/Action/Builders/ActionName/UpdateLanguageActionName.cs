using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramBot.Infrastructure.Action.Constants;
using TelegramBot.Infrastructure.Action.Models;
using TelegramBot.Infrastructure.Resource;
using TelegramBot.Infrastructure.Resource.constants;
using TelegramBot.Infrastructure.Setting.Models;
using TelegramBot.Infrastructure.Shared.Constants;

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
                (isCurrentLanguage ? $"{Unicode.GreenTick} " : string.Empty) +
                resource.Value +
                (!string.IsNullOrEmpty(language.EmojiUnicode) ? $" {language.EmojiUnicode}" : string.Empty);
        }

        public static Language GetLanguageCode(int actionId)
        {
            return actionId switch
            {
                ActionIds.English => new Language(LanguageCodes.English, Unicode.UKFlag),
                ActionIds.ChineseTraditional => new Language(LanguageCodes.ChineseTraditional, Unicode.ChinaFlag),
                ActionIds.ChineseSimplified => new Language(LanguageCodes.ChineseSimplified, Unicode.ChinaFlag),
                ActionIds.Thai => new Language(LanguageCodes.Thai, Unicode.ThaiFlag),
                ActionIds.Vietnamese => new Language(LanguageCodes.Vietnamese, Unicode.VietnamFlag),
                _ => new Language("", ""),
            };
        }
    }
}