using Newtonsoft.Json.Linq;
using TelegramBot.Shared.Constants;

namespace TelegramBot.Modules.Message
{
    public interface IMessageFormatter
    {
        string NewLine();

        string Bold(string value);

        string FormatRow(string key, string value, string value3, string language);

        string FormatHeaderRow(string col1, string col2, string col3, string language);

        string FormatRow(string key, string value, string language);

        string FormatHeaderRow(string col1, string col2, string language);
    }

    public class TelegramFormatter : IMessageFormatter
    {
        public const int MaxRowLength = 31;

        public string NewLine() => "\n";

        public string Bold(string value) => $"<b>{value}</b>";

        public string FormatRow(string key, string value, string value3, string language)
        {
            var twoColumn = FormatRow(key, value, language);

            return twoColumn + " ".PadLeft(10) + value3;
        }

        public string FormatHeaderRow(string col1, string col2, string col3, string language)
        {
            var twoColumn = FormatHeaderRow(col1, col2, language);

            return twoColumn + " ".PadLeft(10) + col3;
        }

        public string FormatRow(string key, string value, string language)
        {
            var length = language switch
            {
                LanguageCodes.ChineseSimplified or LanguageCodes.ChineseTraditional => MaxRowLength - value.Length - key.Length * 2,
                LanguageCodes.Thai => MaxRowLength - value.Length - ThaiLength(key),
                _ => MaxRowLength - value.Length - key.Length,
            };

            return $"{key}{" ".PadLeft(length > 0 ? length : 0)} {value}";
        }

        public string FormatHeaderRow(string col1, string col2, string language)
        {
            var length = language switch
            {
                LanguageCodes.ChineseSimplified or LanguageCodes.ChineseTraditional => MaxRowLength - col1.Length * 2 - col2.Length * 2,
                LanguageCodes.Thai => MaxRowLength - ThaiLength(col1) - ThaiLength(col2),
                _ => MaxRowLength - col1.Length - col2.Length,
            };

            return $"{col1}{" ".PadLeft(length > 0 ? length : 0)} {col2}";
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