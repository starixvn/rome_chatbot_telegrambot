using System.Globalization;
using System.Reflection;
using MemberAPIs.Constants;
using MemberAPIs.Resources;

namespace MemberAPIs.Models;

public class LocalizedMessagesModel
{
    private static readonly (string culture, string property)[] Cultures = new[]
    {
        (LanguageCodeConstants.English, "En"),
        (LanguageCodeConstants.ChineseTraditional, "Ch"),
        (LanguageCodeConstants.ChineseSimplified, "Cs"),
        (LanguageCodeConstants.Thai, "Th"),
        (LanguageCodeConstants.Vietnamese, "Vi"),
        (LanguageCodeConstants.Japanese, "Jp"),
        (LanguageCodeConstants.Korean, "Ko")
    };

    private static readonly PropertyInfo[] LocalizedMessagesProperties = typeof(LocalizedMessagesModel)
        .GetProperties()
        .Where(p => Cultures.Select(c => c.property).Contains(p.Name))
        .ToArray();

    public string En { get; set; }
    public string Ch { get; set; }
    public string Cs { get; set; }
    public string Th { get; set; }
    public string Vi { get; set; }
    public string Jp { get; set; }
    public string Ko { get; set; }

    public static LocalizedMessagesModel GetLocalizedMessages(string messageKey, object[] messageParams)
    {
        if (string.IsNullOrEmpty(messageKey))
        {
            throw new ArgumentNullException(nameof(messageKey), "Message key cannot be null or empty.");
        }

        var messages = new LocalizedMessagesModel();
        messageParams ??= Array.Empty<object>();

        foreach (var (culture, property) in Cultures)
        {
            var resourceString = Labels.ResourceManager.GetString(messageKey, CultureInfo.CreateSpecificCulture(culture)) ?? string.Empty;
            var message = string.Format(resourceString, messageParams);
            var propertyInfo = LocalizedMessagesProperties.FirstOrDefault(p => p.Name == property);
            propertyInfo?.SetValue(messages, message);
        }

        return messages;
    }
}