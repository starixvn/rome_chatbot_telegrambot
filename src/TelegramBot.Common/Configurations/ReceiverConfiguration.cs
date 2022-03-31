namespace TelegramBot.Common.Configurations
{
    public class ReceiverConfiguration
    {
        public string EasyNetQConnectionString { get; set; }

        public bool IsMaintenance { get; set; }

        public string HostAddress { get; set; }

        public bool IsTest { get; set; }

        public bool IsUsingWebhook => !string.IsNullOrWhiteSpace(HostAddress);
    }
}
