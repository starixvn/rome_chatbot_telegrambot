namespace TelegramBot.Modules.Action.Models
{
    public class Language
    {
        public Language(string code, string emojiUnicode)
        {
            Code = code;
            EmojiUnicode = emojiUnicode;
        }

        public string Code { get; }

        public string EmojiUnicode { get; }
    }
}