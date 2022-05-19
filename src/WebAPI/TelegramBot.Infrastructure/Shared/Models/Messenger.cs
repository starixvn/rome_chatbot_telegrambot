namespace TelegramBot.Infrastructure.Shared.Models
{
    public class Messenger
    {
        public Messenger(int botId, long chatId, string botName = "")
        {
            BotId = botId;
            ChatId = chatId;
        }

        public int BotId { get; }

        public long ChatId { get; }
    }
}