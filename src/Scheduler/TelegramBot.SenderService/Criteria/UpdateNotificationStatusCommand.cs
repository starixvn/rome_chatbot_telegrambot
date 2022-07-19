using Fanex.Data.Repository;

namespace TelegramBot.SenderService.Criteria;

public sealed class UpdateNotificationStatusCommand : NonQueryCommand
{
    public UpdateNotificationStatusCommand(
        string xmlData,
        string xmlLastNotification,
        string environment)
    {
        XmlData = xmlData;
        XmlLastNotification = xmlLastNotification;
        Environment = environment;
    }

    public string XmlData { get; }

    public string XmlLastNotification { get; }

    public string Environment { get; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; } = default!;

    public override string GetSettingKey()
        => "Age_Chatbot_SendNotification_Status_Upd";

    public override bool IsValid()
        => !string.IsNullOrWhiteSpace(XmlData)
        && !string.IsNullOrWhiteSpace(XmlLastNotification);
}