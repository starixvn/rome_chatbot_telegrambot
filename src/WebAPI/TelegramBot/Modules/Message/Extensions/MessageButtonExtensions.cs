using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Modules.Message.Models;

namespace TelegramBot.Modules.Message.Extensions;

public static class MessageButtonExtensions
{
    public static InlineKeyboardButton ToInlineButton(this MessageButton button)
    {
        return button.IsWebApp
            ? InlineKeyboardButton.WithWebApp(button.Text, button.WebAppInfo)
            : InlineKeyboardButton.WithCallbackData(button.Text, button.CallBackData);
    }
}