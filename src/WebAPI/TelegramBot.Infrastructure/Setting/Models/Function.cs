namespace TelegramBot.Infrastructure.Setting.Models
{
    public class Function
    {
        public short FunctionId { get; set; }

        public string FunctionName { get; set; }

        public byte Type { get; set; }

        public string Command { get; set; }

        public bool Enabled { get; set; } = true;

        public byte Status { get; set; }
    }
}