using Telegram.Bot.Types;

namespace TelegramBot.Modules.Action.Builders.WebApp;

public interface IWebAppBuilder
{
    WebAppInfo BuildWebAppInfo(string system);
}