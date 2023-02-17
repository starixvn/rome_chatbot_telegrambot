using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types;

namespace TelegramBot.Modules.Action.Builders.WebApp;

public class NewCustomersWebAppBuilder : IWebAppBuilder
{
    private readonly IConfiguration configuration;

    public NewCustomersWebAppBuilder(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public WebAppInfo BuildWebAppInfo(string system)
    {
        var url = configuration.GetValue<string>($"NewCustomerUrl:{system}");
        return new WebAppInfo() { Url = url };
    }
}