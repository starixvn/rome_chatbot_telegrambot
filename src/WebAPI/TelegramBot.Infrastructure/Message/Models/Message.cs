using System.Collections.Generic;

namespace TelegramBot.Infrastructure.Message.Models
{
    public class Message
    {
        public Message(string text, IList<MessageButton> buttons)
        {
            Text = text;
            Buttons = buttons;
        }

        public string Text { get; }

        public IList<MessageButton> Buttons { get; }
    }
}