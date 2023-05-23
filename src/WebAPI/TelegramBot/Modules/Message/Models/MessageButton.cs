using Telegram.Bot.Types;

namespace TelegramBot.Modules.Message.Models;

public class MessageButton
{
    public MessageButton(string text, string callBackData = "", WebAppInfo webAppInfo = null)
    {
        Text = text;
        CallBackData = callBackData;
        WebAppInfo = webAppInfo;
    }

    public string Text { get; }

    public string CallBackData { get; }

    public WebAppInfo WebAppInfo { get; }

    public bool IsWebApp => WebAppInfo != null;
}
