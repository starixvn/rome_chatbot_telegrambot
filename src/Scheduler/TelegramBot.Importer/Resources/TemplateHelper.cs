using System.Globalization;
using System.Resources;
using TelegramBot.Importer.Constants;

namespace TelegramBot.Importer.Resources;

public static class TemplateHelper
{
    private static readonly ResourceManager resourceManager = new("TelegramBot.Importer.Resources.Template", typeof(TemplateHelper).Assembly);

    public static string GetNoInfo(string language)
        => Get("NoInfo", language);

    public static string Get(string key, string language)
    {
        var value = resourceManager.GetString(key, CultureInfo.CreateSpecificCulture(language));

        return string.IsNullOrWhiteSpace(value)
            ? resourceManager.GetString(key, CultureInfo.CreateSpecificCulture(LanguageCode.English))
            : value;
    }
}