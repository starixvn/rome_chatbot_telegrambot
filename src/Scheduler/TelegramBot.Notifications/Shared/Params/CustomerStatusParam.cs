namespace TelegramBot.Notifications.Shared.Params;

public record CustomerStatusParam(int CustId, int SubAccId)
{
    public string ToXml()
        => $"<r id=\"{CustId}\" sub=\"{SubAccId}\"/>";
}