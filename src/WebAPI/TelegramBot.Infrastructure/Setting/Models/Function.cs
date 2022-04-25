namespace TelegramBot.Infrastructure.Setting.Models
{
    public class Function
    {
        public Function()
        {
        }

        public short FunctionId { get; set; }

        public string FunctionName { get; set; }

        public byte Type { get; set; }

        public string Command { get; set; }

        public bool Enabled { get; set; }

        public byte Status { get; set; }
    }
}