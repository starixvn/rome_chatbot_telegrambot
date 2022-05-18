namespace TelegramBot.Infrastructure.Shared.Models
{
    public class Messenger
    {
        public Messenger(int botId, long chatId, string botName = "")
        {
            BotId = botId;
            ChatId = chatId;
            BotName = botName;
        }

        public int BotId { get; }

        public string BotName { get; }

        public long ChatId { get; }
    }
}