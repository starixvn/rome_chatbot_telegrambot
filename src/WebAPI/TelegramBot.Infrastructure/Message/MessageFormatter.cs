using TelegramBot.Infrastructure.Shared.Constants;

namespace TelegramBot.Infrastructure.Message
{
    public interface IMessageFormatter
    {
        string NewLine();

        string Point();

        string Bold(string value);

        string FormatRow(string key, string value, string language);

        string FormatHeaderRow(string col1, string col2, string language);
    }

    public class TelegramFormatter : IMessageFormatter
    {
        public const int MaxRowLength = 30;

        public string NewLine() => "\n";

        public string Point() => "\u2022";

        public string Bold(string value) => $"<b>{value}</b>";

        public string FormatRow(string key, string value, string language)
        {
            var length = language switch
            {
                LanguageCodes.English => MaxRowLength - value.Length - key.Length,
                LanguageCodes.ChineseSimplified or LanguageCodes.ChineseTraditional => MaxRowLength - value.Length - key.Length * 2,
                LanguageCodes.Thai => MaxRowLength - value.Length - ThaiLength(key),
                _ => 1,
            };
            return $"{key}{" ".PadLeft(length > 0 ? length : 1)}{value}";
        }

        public string FormatHeaderRow(string col1, string col2, string language)
        {
            var length = language switch
            {
                LanguageCodes.English => MaxRowLength - col1.Length - col2.Length,
                LanguageCodes.ChineseSimplified or LanguageCodes.ChineseTraditional => MaxRowLength - col1.Length * 2 - col2.Length * 2,
                LanguageCodes.Thai => MaxRowLength - ThaiLength(col1) - ThaiLength(col2),
                _ => 1,
            };

            return $"{col1}{" ".PadLeft(length > 0 ? length : 1)}{col2}";
        }

        private static int ThaiLength(string text)
        {
            int c = 0;
            int l = text.Length;

            for (int i = 0; i < l; ++i)
            {
                if (char.GetUnicodeCategory(text[i]) != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    ++c;
                }
            }

            return c;
        }
    }
}