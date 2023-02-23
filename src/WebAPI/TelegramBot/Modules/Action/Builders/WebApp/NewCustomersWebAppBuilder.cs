using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types;
using TelegramBot.Shared.Models;

namespace TelegramBot.Modules.Action.Builders.WebApp;

public class NewCustomersWebAppBuilder : IWebAppBuilder
{
    private readonly IConfiguration configuration;

    public NewCustomersWebAppBuilder(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public WebAppInfo BuildWebAppInfo(Messenger messenger)
    {
        var url = configuration.GetValue<string>($"NewCustomerUrl:{messenger.System}");
        return new WebAppInfo() { Url = $"{url}?botId={messenger.BotId}" };
    }
}