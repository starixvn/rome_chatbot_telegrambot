namespace TelegramBot.WebApi.Params;

public class VerifyOTPParam
{
    public int CustId { get; set; }

    public int SubAccId { get; set; }

    public string OTP { get; set; }
}

