namespace TelegramBot.WebApi.Params;

public class CopyAccountNotifyParam
{
    public int CustId { get; set; }

    public int SubAccId { get; set; }

    public string Message { get; set; }
}