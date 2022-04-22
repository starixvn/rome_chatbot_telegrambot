namespace TelegramBot.Infrastructure.Setting.Models
{
    public class Function
    {
        public Function(short functionId, string functionName, byte type, bool enabled)
        {
            FunctionId = functionId;
            FunctionName = functionName;
            Type = type;
            Enabled = enabled;
        }

        public short FunctionId { get; }

        public string FunctionName { get; }

        public byte Type { get; }

        public bool Enabled { get; }

        public int Environment { get; }
    }
}