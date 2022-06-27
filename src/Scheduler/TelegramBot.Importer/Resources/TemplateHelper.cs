using System.Globalization;
using System.Resources;
using System.Xml.Linq;
using TelegramBot.Importer.Constants;

namespace TelegramBot.Importer.Resources;

public static class TemplateHelper
{
    private static readonly ResourceManager resourceManager = new("TelegramBot.Importer.Resources.Template", typeof(TemplateHelper).Assembly);

    public static string Get(string key, string language)
    {
        var value = resourceManager.GetString(key, CultureInfo.CreateSpecificCulture(language));

        return string.IsNullOrWhiteSpace(value)
            ? resourceManager.GetString(key, CultureInfo.CreateSpecificCulture(LanguageCode.English))
            : value;
    }

    public static string BuildTitle(string language, string resourceKey, string date)
        => $"[{Get(resourceKey, language)} - {date}]";

    public static string BuildTitleWithoutStrikeThrough(string language, string resourceKey, string date)
        => $"[{Get(resourceKey, language)} {date}]";

    public static string BuildBoldContent(string language, string resourceKey)
        => $"<b>{Get(resourceKey, language)}:</b>";

    public static string EncodeXml(string text, bool isAttribute = true, int attrSubStringLength = 5)
        => isAttribute
        ? new XAttribute("__n",text).ToString().Substring(attrSubStringLength).TrimEnd('\"')
        : new XElement("t", text).LastNode.ToString();
}