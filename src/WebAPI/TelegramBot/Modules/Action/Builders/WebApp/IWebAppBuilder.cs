using Telegram.Bot.Types;
using TelegramBot.Modules.Setting.Models;

namespace TelegramBot.Modules.Action.Builders.WebApp;

public interface IWebAppBuilder
{
    WebAppInfo BuildWebAppInfo(CustBotAccount account);
}