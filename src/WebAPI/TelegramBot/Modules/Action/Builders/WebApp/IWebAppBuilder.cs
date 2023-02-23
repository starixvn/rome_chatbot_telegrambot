using Telegram.Bot.Types;
using TelegramBot.Shared.Models;

namespace TelegramBot.Modules.Action.Builders.WebApp;

public interface IWebAppBuilder
{
    WebAppInfo BuildWebAppInfo(Messenger messenger);
}