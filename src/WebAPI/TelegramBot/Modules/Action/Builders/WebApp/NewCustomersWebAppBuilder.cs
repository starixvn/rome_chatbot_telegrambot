using Telegram.Bot.Types;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Constants;

namespace TelegramBot.Modules.Action.Builders.WebApp;

public class NewCustomersWebAppBuilder : IWebAppBuilder
{
    private readonly IDictionary<string, string> webAppUrls;

    public NewCustomersWebAppBuilder(IDictionary<string, string> webAppUrls)
    {
        this.webAppUrls = webAppUrls;
    }

    public WebAppInfo BuildWebAppInfo(CustBotAccount account)
    {
        return new WebAppInfo()
        {
            Url = $"{webAppUrls[account.System]}/{LanguageCodes.MapWebAppLanguage(account.Language)}/newcustomer?system=${account.System}"
        };
    }
}