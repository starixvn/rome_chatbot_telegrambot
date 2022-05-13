using System.Security;
using TelegramBot.Notifications.Shared.Constants;
using TelegramBot.Notifications.Shared.Extensions;

namespace TelegramBot.Notifications.Messages.Services;

public static class MessageTemplate
{
    private static readonly IDictionary<string, string> PersonalMessageTemplates = new Dictionary<string, string>
    {
        [LanguageCode.English] = "[Personal Message - {0}]\n{1}",
        [LanguageCode.ChineseTraditional] = "[個人訊息 - {0}]\n{1}",
        [LanguageCode.ChineseSimplified] = "[个人讯息 - {0}]\n{1}",
        [LanguageCode.Thai] = "[ข้อความส่วนตัว - {0}]\n{1}",
        [LanguageCode.Vietnamese] = "[Tin nhắn cá nhân - {0}]\n{1}"
    };

    public static string BuildPersonalMessage(string language, string content, string transId = "")
    {
        if (!PersonalMessageTemplates.TryGetValue(language, out var template))
        {
            template = PersonalMessageTemplates[LanguageCode.English];
        }

        return string.Format(
            template,
            DateTime.Today.ToSimpleDate(),
            SecurityElement.Escape(transId.Length == 0 ? content : content.Replace("@@", transId)));
    }
}