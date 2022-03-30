namespace TelegramBot.WebApi.Params
{
    public class ResendOtpParam
    {
        public int BotId { get; set; }

        public int CustId { get; set; }

        public bool IsTest { get; set; }

        public int SubAccId { get; set; }
    }
}
