namespace TelegramBot.Modules.Bot.Models
{
    public class BotEntity
    {
        public BotEntity(int botId, string username, string system, string accessToken, bool status, bool isTest, string displayName)
        {
            BotId = botId;
            Username = username;
            System = system;
            AccessToken = accessToken;
            Status = status;
            IsTest = isTest;
            DisplayName = displayName;
        }

        public int BotId { get; }

        public long MessengerBotId { get; set; }

        public string Username { get; }

        public string System { get; }

        public string AccessToken { get; }

        public bool Status { get; }

        public bool IsTest { get; }

        public string DisplayName { get; }
    }
}