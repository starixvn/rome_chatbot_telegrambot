namespace TelegramBot.Infrastructure.Shared.Constants
{
    public static class LanguageCodes
    {
        public const string English = "en-US";
        public const string ChineseTraditional = "zh-TW";
        public const string ChineseSimplified = "zh-CN";
        public const string Thai = "th-TH";
        public const string Vietnamese = "vi-VN";
        public static string[] SupportLanguages => new[] { English, ChineseTraditional, ChineseSimplified, Thai, Vietnamese };
    }
}