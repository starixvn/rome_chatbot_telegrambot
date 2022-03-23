using System.Globalization;

namespace TelegramBot.Common.Resources
{
    public static class ResourceHelper
    {
        public static string Get(string key, string language)
            => SharedResource.ResourceManager.GetString(key, CultureInfo.CreateSpecificCulture(language));

        public static string Get(string key, CultureInfo culture)
            => SharedResource.ResourceManager.GetString(key, culture);
    }
}
