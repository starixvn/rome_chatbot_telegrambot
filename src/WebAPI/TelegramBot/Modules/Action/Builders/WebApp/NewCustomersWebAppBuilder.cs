using Telegram.Bot.Types;
using TelegramBot.Modules.Setting.Models;
using TelegramBot.Shared.Constants;

namespace TelegramBot.Modules.Action.Builders.WebApp;

public class NewCustomersWebAppBuilder : IWebAppBuilder
{
    private readonly string webAppUrl;

    public NewCustomersWebAppBuilder(string webAppUrl)
    {
        this.webAppUrl = webAppUrl;
    }

    public WebAppInfo BuildWebAppInfo(CustBotAccount account)
    {
        return new WebAppInfo() { Url = $"{webAppUrl}/{LanguageCodes.MapWebAppLanguage(account.Language)}/newcustomer" };
    }
}