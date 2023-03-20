using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types;
using TelegramBot.Modules.Setting.Models;

namespace TelegramBot.Modules.Action.Builders.WebApp;

public class NewCustomersWebAppBuilder : IWebAppBuilder
{
    private readonly IConfiguration configuration;

    public NewCustomersWebAppBuilder(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public WebAppInfo BuildWebAppInfo(CustBotAccount account)
    {
        var url = configuration.GetValue<string>($"NewCustomerUrl:{account.System}");
        return new WebAppInfo() { Url = $"{url}" };
    }
}