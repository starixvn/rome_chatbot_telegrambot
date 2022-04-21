namespace TelegramBot.WebApi.Params
{
    public class StartParam
    {
        public long MessengerBotId { get; set; }

        public long ChatId { get; set; }

        public string Message { get; set; }

        public string Language { get; set; }

        public string IP { get; set; }
    }
}