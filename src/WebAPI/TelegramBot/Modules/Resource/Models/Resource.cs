namespace TelegramBot.Modules.Resource.Models
{
    public class BotResource
    {
        public BotResource(string type, string key, string language, string value)
        {
            Type = type;
            Key = key;
            Language = language;
            Value = value;
        }

        public string Type { get; }

        public string Key { get; }

        public string Language { get; }

        public string Value { get; }
    }
}