namespace TelegramBot.Common.Entities
{
    public class BotEntity
    {
        public int BotId { get; set; }

        public long MessengerBotId { get; set; }

        public string Name { get; set; }

        public string System { get; set; }

        public string AccessToken { get; set; }

        public bool Status { get; set; }

        public bool IsTest { get; set; }
    }
}
