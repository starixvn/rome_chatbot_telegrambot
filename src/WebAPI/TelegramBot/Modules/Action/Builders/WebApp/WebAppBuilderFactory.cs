using Microsoft.Extensions.Configuration;
using TelegramBot.Modules.Action.Constants;

namespace TelegramBot.Modules.Action.Builders.WebApp;

public interface IWebAppBuilderFactory
{
    IWebAppBuilder GetBuilder(short actionId);
}

public class WebAppBuilderFactory : IWebAppBuilderFactory
{
    private readonly IDictionary<string, string> webAppUrls;

    public WebAppBuilderFactory(IConfiguration configuration)
        => webAppUrls = configuration.GetSection("WebAppUrls").GetChildren().ToDictionary(x => x.Key, x => x.Value);

    public IWebAppBuilder GetBuilder(short actionId)
    {
        switch(actionId) {
            case ActionIds.NewCustomers:
                return new NewCustomersWebAppBuilder(webAppUrls);
            case ActionIds.ViewAutoTransferMonitor:
                return new ViewAutoTransferMonitorWebAppBuilder(webAppUrls);
            case ActionIds.EditAutoTransfer:
                return new EditAutoTransferWebAppBuilder(webAppUrls);
            default:
                throw new InvalidOperationException($"Test ActionId: {actionId}");
        }
    }
}