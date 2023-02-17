using Microsoft.Extensions.Configuration;
using TelegramBot.Modules.Action.Constants;

namespace TelegramBot.Modules.Action.Builders.WebApp;

public interface IWebAppBuilderFactory
{
    IWebAppBuilder GetBuilder(short actionId);
}

public class WebAppBuilderFactory : IWebAppBuilderFactory
{
    private readonly IConfiguration configuration;

    public WebAppBuilderFactory(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public IWebAppBuilder GetBuilder(short actionId)
    {
        if (actionId == ActionIds.NewCustomers)
        {
            return new NewCustomersWebAppBuilder(configuration);
        }

        throw new InvalidOperationException($"ActionId: {actionId}");
    }
}