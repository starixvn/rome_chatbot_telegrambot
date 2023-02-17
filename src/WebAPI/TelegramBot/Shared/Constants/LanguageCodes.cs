namespace TelegramBot.Shared.Constants
{
    public static class LanguageCodes
    {
        public const string English = "en-US";
        public const string ChineseTraditional = "zh-Hant";
        public const string ChineseSimplified = "zh-Hans";
        public const string Thai = "th-TH";
        public const string Vietnamese = "vi-VN";
        public static string[] SupportLanguages => new[] { English, ChineseTraditional, ChineseSimplified, Thai, Vietnamese };

        private static readonly IDictionary<string, string> LanguageCodeMapper = new Dictionary<string, string>
        {
            ["zh-CN"] = ChineseSimplified,
            ["zh-TW"] = ChineseTraditional
        };

        public static string MapLanguage(string language)
        {
            if (LanguageCodeMapper.ContainsKey(language))
            {
                return LanguageCodeMapper[language];
            }

            return SupportLanguages.Contains(language) ? language : English;
        }
    }
}