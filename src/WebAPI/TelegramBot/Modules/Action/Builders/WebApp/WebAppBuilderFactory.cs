using Microsoft.Extensions.Configuration;
using TelegramBot.Modules.Action.Constants;

namespace TelegramBot.Modules.Action.Builders.WebApp;

public interface IWebAppBuilderFactory
{
    IWebAppBuilder GetBuilder(short actionId);
}

public class WebAppBuilderFactory : IWebAppBuilderFactory
{
    private readonly string webAppUrl;

    public WebAppBuilderFactory(IConfiguration configuration)
    {
        webAppUrl = configuration.GetValue<string>("WebAppUrl");
    }

    public IWebAppBuilder GetBuilder(short actionId)
    {
        if (actionId == ActionIds.NewCustomers)
        {
            return new NewCustomersWebAppBuilder(webAppUrl);
        }

        throw new InvalidOperationException($"ActionId: {actionId}");
    }
}