namespace TelegramBot.Client.Models
{
    public class Function
    {
        public short FunctionId { get; set; }

        public string FunctionName { get; set; }

        public byte Type { get; set; }

        public bool Enabled { get; set; }
    }
}