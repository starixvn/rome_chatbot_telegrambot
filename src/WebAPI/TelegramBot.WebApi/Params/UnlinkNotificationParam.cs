namespace TelegramBot.WebApi.Params
{
    public class UnlinkNotificationParam
    {
        public long ChatId { get; set; }

        public int BotId { get; set; }

        public string Language { get; set; }
    }
}