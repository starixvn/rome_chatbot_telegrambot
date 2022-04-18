namespace TelegramBot.Infrastructure.Bot.Models
{
    public class BotEntity
    {
        public BotEntity(int botId, string name, string system, string accessToken, bool status, bool isTest)
        {
            BotId = botId;
            Name = name;
            System = system;
            AccessToken = accessToken;
            Status = status;
            IsTest = isTest;
        }

        public int BotId { get; }

        public long MessengerBotId { get; set; }

        public string Name { get; }

        public string System { get; }

        public string AccessToken { get; }

        public bool Status { get; }

        public bool IsTest { get; }
    }
}