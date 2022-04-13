namespace TelegramBot.Infrastructure.Shared.Configurations
{
    public class WebAPIConfiguration
    {
        public int OtpExpirationInMinutes { get; set; }

        public int OtpMaxFailedTimes { get; set; }
    }
}