using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml.Linq;
using TelegramBot.Importer.Constants;

namespace TelegramBot.Importer.Resources;

[ExcludeFromCodeCoverage]
public static class TemplateHelper
{
    private const int MaxRowLength = 30;

    public static string Get(string key, string language)
    {
        var value = Template.ResourceManager.GetString(key, CultureInfo.CreateSpecificCulture(language));

        return string.IsNullOrWhiteSpace(value)
            ? Template.ResourceManager.GetString(key, CultureInfo.CreateSpecificCulture(LanguageCode.English))
            : value;
    }

    public static string BuildTitle(string language, string resourceKey, string date)
        => $"[{Get(resourceKey, language)} - {date}]";

    public static string BuildTitleWithoutStrikeThrough(string language, string resourceKey, string date)
        => $"[{Get(resourceKey, language)} {date}]";

    public static string BuildContent(string language, string resourceKey)
        => Get(resourceKey, language);

    public static string FormatRow(string key, string value, string language)
    {
        var length = language switch
        {
            LanguageCode.ChineseSimplified or LanguageCode.ChineseTraditional => MaxRowLength - value.Length - key.Length * 2,
            LanguageCode.Thai => MaxRowLength - value.Length - ThaiLength(key),
            _ => MaxRowLength - value.Length - key.Length,
        };
        return $"{key}{" ".PadLeft(length > 0 ? length : 0)} {value}";
    }

    public static string EncodeXml(string text, bool isAttribute = true, int attrSubStringLength = 5)
        => isAttribute
        ? new XAttribute("__n", text).ToString().Substring(attrSubStringLength).TrimEnd('\"')
        : new XElement("t", text).LastNode.ToString();

    private static int ThaiLength(string text)
    {
        int c = 0;
        int l = text.Length;

        for (int i = 0; i < l; ++i)
        {
            if (char.GetUnicodeCategory(text[i]) != UnicodeCategory.NonSpacingMark)
            {
                ++c;
            }
        }

        return c;
    }
}