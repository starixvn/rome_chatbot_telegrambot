namespace TelegramBot.Infrastructure.Setting.Models
{
    public class Function
    {
        public Function(int functionId, string functionName, byte type, bool status)
        {
            FunctionId = functionId;
            FunctionName = functionName;
            Type = type;
            Status = status;
        }

        public int FunctionId { get; }

        public string FunctionName { get; }

        public byte Type { get; }

        public bool Status { get; }
    }
}