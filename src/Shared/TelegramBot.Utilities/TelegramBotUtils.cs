
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Telegram.Bot;

namespace TelegramBot.Utilities;

[ExcludeFromCodeCoverage]
public static class TelegramBotUtils
{
    public static ITelegramBotClient InitBotClient(string accessToken, string proxyAddress = "")
    {
        if (!string.IsNullOrEmpty(proxyAddress))
        {
            var webProxy = new WebProxy(proxyAddress);
            var httpClient = new HttpClient(new HttpClientHandler { Proxy = webProxy, UseProxy = true });

            return new TelegramBotClient(accessToken, httpClient);
        }

        return new TelegramBotClient(accessToken);
    }
}
