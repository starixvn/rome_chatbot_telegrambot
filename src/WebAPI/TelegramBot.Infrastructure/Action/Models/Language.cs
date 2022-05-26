namespace TelegramBot.Infrastructure.Action.Models
{
    public class Language
    {
        public Language(string code, string emojiCharacter)
        {
            Code = code;
            EmojiCharacter = emojiCharacter;
        }

        public string Code { get; }

        public string EmojiCharacter { get; }
    }
}