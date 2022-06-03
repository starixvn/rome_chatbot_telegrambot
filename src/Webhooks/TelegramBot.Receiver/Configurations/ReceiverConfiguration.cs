namespace TelegramBot.Receiver.Configurations
{
    public class ReceiverConfiguration
    {
        public bool IsMaintenance { get; set; }

        public string HostAddress { get; set; }

        public bool IsTest { get; set; }

        public string ProxyAddress { get; set; }

        public bool IsUseProxy => !string.IsNullOrEmpty(ProxyAddress);
    }
}