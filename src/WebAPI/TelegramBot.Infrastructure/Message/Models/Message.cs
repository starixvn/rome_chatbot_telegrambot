using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Infrastructure.Message.Models
{
    public class Message
    {
        public Message(string text, InlineKeyboardMarkup replyMarkup = null)
        {
            Text = text;
            ReplyMarkup = replyMarkup;
        }

        public string Text { get; }

        public InlineKeyboardMarkup ReplyMarkup { get; }
    }
}