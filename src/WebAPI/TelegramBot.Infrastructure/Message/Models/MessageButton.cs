namespace TelegramBot.Infrastructure.Message.Models
{
    public class MessageButton
    {
        public MessageButton(string text, string callBackData)
        {
            Text = text;
            CallBackData = callBackData;
        }

        public string Text { get; }

        public string CallBackData { get; }
    }
}