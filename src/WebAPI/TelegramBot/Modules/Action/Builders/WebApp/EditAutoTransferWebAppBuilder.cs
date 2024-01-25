using Telegram.Bot.Types;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Constants;

namespace TelegramBot.Modules.Action.Builders.WebApp;

public class EditAutoTransferWebAppBuilder : IWebAppBuilder
{
    private readonly IDictionary<string, string> webAppUrls;

    public EditAutoTransferWebAppBuilder(IDictionary<string, string> webAppUrls)
    {
        this.webAppUrls = webAppUrls;
    }

    public WebAppInfo BuildWebAppInfo(CustBotAccount account)
    {
        return new WebAppInfo
        {
            Url = $"{webAppUrls[account.System]}/{LanguageCodes.MapWebAppLanguage(account.Language)}/editautotransfer?system={account.System}"
        };
    }
}