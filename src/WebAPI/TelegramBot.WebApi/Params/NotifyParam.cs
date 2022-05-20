namespace TelegramBot.WebApi.Params
{
    public class NotifyParam
    {
        public int BotId { get; set; }

        public long ChatId { get; set; }

        public string Message { get; set; }
    }
}